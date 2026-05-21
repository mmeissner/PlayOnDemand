#region Licence
/****************************************************************
 *  Filename: AppinfoDecoder.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NLog;

namespace LeapVR.Utilities.Steam.Steam.VDF.Binary {
    /// <summary>
    /// Binary <c>appinfo.vdf</c> reader for Steam's per-app metadata cache.
    /// </summary>
    /// <remarks>
    /// Steam has shipped four magic numbers for this file:
    /// <list type="bullet">
    /// <item><c>0x07564426</c> — original, multiple inline-string KV1 sections per app.</item>
    /// <item><c>0x07564427</c> — single root KV1 subsection per app (inline strings).</item>
    /// <item><c>0x07564428</c> — adds an end-of-file string table; KV1 keys become 32-bit
    /// indices into that table. Header carries a <c>uint64</c> offset to the table.</item>
    /// <item><c>0x07564429</c> — same as <c>0x28</c> plus an extra 20-byte SHA1 hash
    /// (of the binary VDF payload) per app, sitting between <c>ChangeNumber</c> and the
    /// KV1 block.</item>
    /// </list>
    /// All four are supported. Unsupported versions throw on <see cref="Decode"/>.
    /// </remarks>
    public class AppinfoDecoder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Magic constants documented in the public Steam community wiki and
        // SteamRE reverse-engineering work. The high 24 bits are ASCII "VD\x07"
        // (little-endian "VDF" minus the F); the low byte is the schema version.
        private const uint VersionV26 = 0x07564426;
        private const uint VersionV27 = 0x07564427;
        private const uint VersionV28 = 0x07564428;
        private const uint VersionV29 = 0x07564429;
        private const uint UniverseExpected = 0x00000001;
        private const byte EndOfSection = 0x08;

        private readonly BinaryReader _br;
        private uint _version;
        private uint _universe;
        private string[] _stringTable;
        private Dictionary<string, GameData> _data;

        public AppinfoDecoder(BinaryReader br)
        {
            _br = br ?? throw new ArgumentNullException(nameof(br));
        }

        public Dictionary<string, GameData> Data => _data;
        public uint Version => _version;
        public uint Universe => _universe;

        /// <summary>True when the file uses the post-2022 string-table layout (v0x28+).</summary>
        public bool HasStringTable => _version >= VersionV28;

        public void Decode()
        {
            _version = _br.ReadUInt32();
            _universe = _br.ReadUInt32();

            if (_version != VersionV26 && _version != VersionV27 &&
                _version != VersionV28 && _version != VersionV29)
            {
                throw new Exception(string.Format("Unknown VDF_VERSION: 0x{0}", _version.ToString("X8")));
            }
            if (_universe != UniverseExpected)
            {
                throw new Exception(string.Format("Unknown VDF_UNIVERSE: 0x{0}", _universe.ToString("X8")));
            }

            if (HasStringTable)
            {
                // v0x28+ header carries a uint64 offset to the string table at the
                // tail of the file. Jump there, load it, then return to the start of
                // the per-app records (right after the offset we just consumed).
                long stringTableOffset = _br.ReadInt64();
                long afterHeader = _br.BaseStream.Position;
                LoadStringTable(stringTableOffset);
                _br.BaseStream.Position = afterHeader;
            }

            _data = ParseApps();
        }

        // -------------------------------------------------------------------
        // String table (v0x28+ only)
        // -------------------------------------------------------------------

        private void LoadStringTable(long offset)
        {
            _br.BaseStream.Position = offset;
            uint count = _br.ReadUInt32();
            _stringTable = new string[count];
            for (uint i = 0; i < count; i++)
            {
                _stringTable[i] = ReadNullTerminatedString();
            }
        }

        // -------------------------------------------------------------------
        // Per-app records
        // -------------------------------------------------------------------

        private Dictionary<string, GameData> ParseApps()
        {
            var parsed = new Dictionary<string, GameData>();
            while (true)
            {
                uint appid = _br.ReadUInt32();
                // Sentinel: appid == 0 ends the per-app section.
                if (appid == 0) break;

                var appdata = new GameData
                {
                    Size = _br.ReadUInt32(),
                    State = _br.ReadUInt32(),
                    LastUpdate = _br.ReadUInt32(),
                    AccessToken = _br.ReadUInt64(),
                    CheckSum = _br.ReadBytes(20),
                    ChangeNumber = _br.ReadUInt32(),
                };

                if (_version == VersionV29)
                {
                    // v0x29 adds a second 20-byte SHA1 (of the binary VDF payload) here.
                    // Drained but not currently surfaced — the consumer doesn't need it.
                    _br.ReadBytes(20);
                }

                appdata.Sections = (_version == VersionV26)
                    ? ParseV26TopLevel()
                    : ParseSubsection(rootSection: true);

                parsed[appid.ToString()] = appdata;
            }
            return parsed;
        }

        /// <summary>
        /// v0x26 wrapped per-app KV1 in multiple named root sections (e.g. "appinfo",
        /// "common"). Each section starts with a section-id byte then a name string
        /// then a subsection. Loop until the first byte isn't a section-id.
        /// </summary>
        private VdfData ParseV26TopLevel()
        {
            var sections = new VdfData();
            while (true)
            {
                byte sectionId = _br.ReadByte();
                if (sectionId != 0) break;
                // The legacy format inserted a 0x00 byte before the section name.
                _br.ReadByte();
                string sectionName = ReadNullTerminatedString();
                sections[sectionName] = ParseSubsection(rootSection: true);
            }
            return sections;
        }

        // -------------------------------------------------------------------
        // KV1 binary subsection parser
        //
        //   ValueType (1 byte):
        //     0x00 = nested subsection
        //     0x01 = inline null-terminated string
        //     0x02 = uint32
        //     0x07 = uint64
        //     0x08 = end-of-subsection
        //   Key:
        //     v0x26/27: inline null-terminated string
        //     v0x28+ : uint32 index into the file-level string table
        //   Value:
        //     varies by ValueType
        // -------------------------------------------------------------------

        private VdfData ParseSubsection(bool rootSection = false)
        {
            var subsection = new VdfData();
            while (true)
            {
                byte valueType = _br.ReadByte();
                if (valueType == EndOfSection)
                {
                    break;
                }

                string key = ReadKey();
                object value;
                switch (valueType)
                {
                    case 0x00:
                        value = ParseSubsection();
                        break;
                    case 0x01:
                        value = ReadNullTerminatedString();
                        break;
                    case 0x02:
                        value = _br.ReadUInt32();
                        break;
                    case 0x07:
                        value = _br.ReadUInt64();
                        break;
                    default:
                        throw new Exception(string.Format(
                            "Cannot parse appinfo.vdf entry: unsupported KV1 value type 0x{0} at offset 0x{1}",
                            valueType.ToString("X2"),
                            (_br.BaseStream.Position - 1).ToString("X")));
                }

                // Same-key collisions are legal in KV1; suffix duplicates so we
                // don't lose data. The kiosk only reads well-defined keys
                // ("common", "config", etc.) so this is purely defensive.
                if (subsection.ContainsKey(key))
                {
                    key = key + "__dup_" + subsection.Count;
                }
                subsection.Add(key, value);
            }
            return subsection;
        }

        private string ReadKey()
        {
            if (HasStringTable)
            {
                uint index = _br.ReadUInt32();
                if (_stringTable == null || index >= _stringTable.Length)
                {
                    throw new Exception(string.Format(
                        "appinfo.vdf string-table index {0} out of range (table size {1})",
                        index, _stringTable == null ? 0 : _stringTable.Length));
                }
                return _stringTable[index];
            }
            return ReadNullTerminatedString();
        }

        private string ReadNullTerminatedString()
        {
            // KV1 strings are byte sequences terminated by 0x00. The encoding is
            // technically implementation-defined; ASCII covers everything the
            // SteamAppInfo consumer reads (type, gameid, oslist, openvrsupport,
            // installdir, executable, ...), and SteamAppInfo re-encodes display
            // names via Encoding.Default→UTF-8 itself.
            var bytes = new List<byte>(32);
            byte b;
            while ((b = _br.ReadByte()) != 0)
            {
                bytes.Add(b);
            }
            return Encoding.Default.GetString(bytes.ToArray());
        }
    }
}
