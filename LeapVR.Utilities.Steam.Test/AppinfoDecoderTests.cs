#region Licence
/****************************************************************
 *  Filename: AppinfoDecoderTests.cs
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
using LeapVR.Utilities.Steam.Steam.VDF.Binary;
using Xunit;

namespace LeapVR.Utilities.Steam.Test
{
    /// <summary>
    /// Characterization tests for the binary appinfo.vdf reader. Each test
    /// synthesises a tiny well-formed binary blob for one of the four published
    /// Steam schema versions (0x26/27/28/29), feeds it through AppinfoDecoder,
    /// and asserts the parsed shape. The regression that triggered this rewrite
    /// was a real-world v0x29 file on the user's machine that the original
    /// decoder rejected with "Unknown VDF_VERSION".
    /// </summary>
    public class AppinfoDecoderTests
    {
        private const uint MagicV26 = 0x07564426;
        private const uint MagicV27 = 0x07564427;
        private const uint MagicV28 = 0x07564428;
        private const uint MagicV29 = 0x07564429;
        private const uint UniverseExpected = 0x00000001;
        private const uint TestAppId = 250820u;

        // -- Header behaviour -------------------------------------------------

        [Fact]
        public void Decode_throws_on_unknown_magic()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(0xDEADBEEF);
            bw.Write(UniverseExpected);
            ms.Position = 0;

            var decoder = new AppinfoDecoder(new BinaryReader(ms));
            var ex = Assert.Throws<Exception>(() => decoder.Decode());
            Assert.Contains("DEADBEEF", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Decode_throws_on_unknown_universe()
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(MagicV27);
            bw.Write(0xFFu);
            ms.Position = 0;

            var decoder = new AppinfoDecoder(new BinaryReader(ms));
            Assert.Throws<Exception>(() => decoder.Decode());
        }

        // -- v0x27 (single root subsection, inline-string keys) ---------------

        [Fact]
        public void Decode_v27_reads_single_app_with_common_section()
        {
            var bytes = BuildAppinfoV27WithSingleGame(
                appId: TestAppId,
                gameName: "SteamVR",
                gameType: "Game");

            var decoder = DecodeBytes(bytes);

            Assert.Equal(MagicV27, decoder.Version);
            Assert.False(decoder.HasStringTable);
            Assert.True(decoder.Data.TryGetValue(TestAppId.ToString(), out var app));
            var common = app.Sections.GetVdf("appinfo/common");
            Assert.Equal("SteamVR", common["name"]);
            Assert.Equal("Game", common["type"]);
        }

        // -- v0x26 (multiple root sections, inline-string keys) ---------------

        [Fact]
        public void Decode_v26_reads_legacy_multi_section_layout()
        {
            var bytes = BuildAppinfoV26WithSingleGame(
                appId: TestAppId,
                gameName: "Legacy Game",
                gameType: "Demo");

            var decoder = DecodeBytes(bytes);

            Assert.Equal(MagicV26, decoder.Version);
            Assert.True(decoder.Data.TryGetValue(TestAppId.ToString(), out var app));
            // v0x26 stores sections directly at the root, not under "appinfo".
            var common = app.Sections.GetVdf("common");
            Assert.Equal("Legacy Game", common["name"]);
            Assert.Equal("Demo", common["type"]);
        }

        // -- v0x28 (string-table keys, no per-app extra hash) -----------------

        [Fact]
        public void Decode_v28_resolves_keys_via_string_table()
        {
            var bytes = BuildAppinfoV28OrV29WithSingleGame(
                magic: MagicV28,
                appId: TestAppId,
                gameName: "Dota 2",
                gameType: "Game");

            var decoder = DecodeBytes(bytes);

            Assert.Equal(MagicV28, decoder.Version);
            Assert.True(decoder.HasStringTable);
            Assert.True(decoder.Data.TryGetValue(TestAppId.ToString(), out var app));
            var common = app.Sections.GetVdf("appinfo/common");
            Assert.Equal("Dota 2", common["name"]);
            Assert.Equal("Game", common["type"]);
        }

        // -- v0x29 (string-table + extra per-app binary VDF SHA1) -------------

        [Fact]
        public void Decode_v29_consumes_extra_binary_vdf_sha1_header_field()
        {
            var bytes = BuildAppinfoV28OrV29WithSingleGame(
                magic: MagicV29,
                appId: TestAppId,
                gameName: "Half-Life: Alyx",
                gameType: "Game");

            var decoder = DecodeBytes(bytes);

            Assert.Equal(MagicV29, decoder.Version);
            Assert.True(decoder.HasStringTable);
            Assert.True(decoder.Data.TryGetValue(TestAppId.ToString(), out var app));
            // Header is the same shape as v28 *except* for the extra 20-byte SHA1
            // we must skip per app. If we don't skip it, the next byte read as
            // KV1 value-type would be garbage and the parser would either throw
            // or return wrong data. Reaching this assertion proves we skipped.
            var common = app.Sections.GetVdf("appinfo/common");
            Assert.Equal("Half-Life: Alyx", common["name"]);
            Assert.Equal("Game", common["type"]);
        }

        [Fact]
        public void Decode_v29_parses_multiple_games()
        {
            var bytes = BuildAppinfoV28OrV29WithGames(
                magic: MagicV29,
                games: new[]
                {
                    new SyntheticGame(440u, "Team Fortress 2", "Game"),
                    new SyntheticGame(570u, "Dota 2", "Game"),
                    new SyntheticGame(250820u, "SteamVR", "Tool"),
                });

            var decoder = DecodeBytes(bytes);

            Assert.Equal(3, decoder.Data.Count);
            Assert.Equal("Team Fortress 2", decoder.Data["440"].Sections.GetVdf("appinfo/common")["name"]);
            Assert.Equal("Dota 2", decoder.Data["570"].Sections.GetVdf("appinfo/common")["name"]);
            Assert.Equal("SteamVR", decoder.Data["250820"].Sections.GetVdf("appinfo/common")["name"]);
            Assert.Equal("Tool", decoder.Data["250820"].Sections.GetVdf("appinfo/common")["type"]);
        }

        [Fact]
        public void Decode_v29_skips_unknown_keys_via_dup_suffix()
        {
            // Force the writer to repeat a key inside one subsection. The
            // original AppinfoDecoder dropped duplicates via random suffix; the
            // new one uses a deterministic "__dup_N" suffix so callers can
            // recover both values if they care (the SteamAppInfo consumer
            // doesn't, but we promise no data loss).
            var bytes = BuildAppinfoV28OrV29WithDuplicateCommonKey(
                magic: MagicV29,
                appId: TestAppId);

            var decoder = DecodeBytes(bytes);

            var common = decoder.Data[TestAppId.ToString()].Sections.GetVdf("appinfo/common");
            Assert.Contains("name", common.Keys);
            Assert.Contains(common.Keys, k => k.StartsWith("name__dup_"));
        }

        // -- Test helpers / builders -----------------------------------------

        private static AppinfoDecoder DecodeBytes(byte[] bytes)
        {
            var ms = new MemoryStream(bytes);
            var decoder = new AppinfoDecoder(new BinaryReader(ms));
            decoder.Decode();
            return decoder;
        }

        private sealed class SyntheticGame
        {
            public SyntheticGame(uint appId, string name, string type)
            {
                AppId = appId;
                Name = name;
                Type = type;
            }
            public uint AppId { get; }
            public string Name { get; }
            public string Type { get; }
        }

        private static byte[] BuildAppinfoV27WithSingleGame(uint appId, string gameName, string gameType)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(MagicV27);
            bw.Write(UniverseExpected);
            WriteAppHeader(bw, appId, hasBinaryVdfSha1: false);
            // Inline-string KV1: { "appinfo": { "common": { "name": ..., "type": ... } } }
            WriteInlineSubsectionBegin(bw, "appinfo");
            WriteInlineSubsectionBegin(bw, "common");
            WriteInlineStringEntry(bw, "name", gameName);
            WriteInlineStringEntry(bw, "type", gameType);
            WriteSubsectionEnd(bw); // end common
            WriteSubsectionEnd(bw); // end appinfo
            WriteSubsectionEnd(bw); // end root
            bw.Write(0u);           // sentinel appid
            return ms.ToArray();
        }

        private static byte[] BuildAppinfoV26WithSingleGame(uint appId, string gameName, string gameType)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(MagicV26);
            bw.Write(UniverseExpected);
            WriteAppHeader(bw, appId, hasBinaryVdfSha1: false);
            // v0x26: per-app sections live as `(sectionId=0)(0x00)(name)(subsection)` repeated.
            bw.Write((byte)0x00);             // sectionId — start of "common" section
            bw.Write((byte)0x00);             // legacy 0x00 padding before name
            WriteNullTerminatedString(bw, "common");
            WriteInlineStringEntry(bw, "name", gameName);
            WriteInlineStringEntry(bw, "type", gameType);
            WriteSubsectionEnd(bw);           // 0x08 — end of "common" subsection body
            // v0x26 section loop terminates when the next byte isn't a 0x00
            // sectionId. We write 0xFF to break the loop, then the u32 appid=0
            // sentinel for the app loop.
            bw.Write((byte)0xFF);
            bw.Write(0u);
            return ms.ToArray();
        }

        private static byte[] BuildAppinfoV28OrV29WithSingleGame(uint magic, uint appId, string gameName, string gameType)
        {
            return BuildAppinfoV28OrV29WithGames(magic, new[] { new SyntheticGame(appId, gameName, gameType) });
        }

        private static byte[] BuildAppinfoV28OrV29WithGames(uint magic, SyntheticGame[] games)
        {
            // The string table is referenced by every KV1 key in v0x28+, so we
            // need the table populated BEFORE writing per-app records. We build
            // the per-app bodies into a separate buffer, then assemble: header,
            // bodies, string table.
            var strings = new List<string> { "appinfo", "common", "name", "type" };
            var perAppBuffer = new MemoryStream();
            var perAppWriter = new BinaryWriter(perAppBuffer);

            foreach (var g in games)
            {
                WriteAppHeader(perAppWriter, g.AppId, hasBinaryVdfSha1: magic == MagicV29);
                // KV1 root: { "appinfo": { "common": { "name": <str>, "type": <str> } } }
                WriteIndexedSubsectionBegin(perAppWriter, "appinfo", strings);
                WriteIndexedSubsectionBegin(perAppWriter, "common", strings);
                WriteIndexedStringEntry(perAppWriter, "name", g.Name, strings);
                WriteIndexedStringEntry(perAppWriter, "type", g.Type, strings);
                WriteSubsectionEnd(perAppWriter); // end common
                WriteSubsectionEnd(perAppWriter); // end appinfo
                WriteSubsectionEnd(perAppWriter); // end root
            }
            perAppWriter.Write(0u); // sentinel appid

            return AssembleV28V29(magic, perAppBuffer.ToArray(), strings);
        }

        private static byte[] BuildAppinfoV28OrV29WithDuplicateCommonKey(uint magic, uint appId)
        {
            var strings = new List<string> { "appinfo", "common", "name" };
            var perAppBuffer = new MemoryStream();
            var perAppWriter = new BinaryWriter(perAppBuffer);

            WriteAppHeader(perAppWriter, appId, hasBinaryVdfSha1: magic == MagicV29);
            WriteIndexedSubsectionBegin(perAppWriter, "appinfo", strings);
            WriteIndexedSubsectionBegin(perAppWriter, "common", strings);
            WriteIndexedStringEntry(perAppWriter, "name", "FirstName", strings);
            WriteIndexedStringEntry(perAppWriter, "name", "SecondName", strings);
            WriteSubsectionEnd(perAppWriter); // common
            WriteSubsectionEnd(perAppWriter); // appinfo
            WriteSubsectionEnd(perAppWriter); // root
            perAppWriter.Write(0u);           // sentinel

            return AssembleV28V29(magic, perAppBuffer.ToArray(), strings);
        }

        private static byte[] AssembleV28V29(uint magic, byte[] perAppPayload, List<string> strings)
        {
            // Layout:
            //   uint32 magic
            //   uint32 universe
            //   int64  stringTableOffset
            //   <per-app records + appid=0 sentinel>
            //   <stringTable: uint32 count + null-terminated entries>
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(magic);
            bw.Write(UniverseExpected);
            long stringTableOffset = 16 /*header*/ + perAppPayload.Length;
            bw.Write(stringTableOffset);
            bw.Write(perAppPayload);
            // string table
            Assert.Equal(stringTableOffset, ms.Position);
            bw.Write((uint)strings.Count);
            foreach (var s in strings) WriteNullTerminatedString(bw, s);
            return ms.ToArray();
        }

        // -- Per-app header (shared across versions) -------------------------

        private static void WriteAppHeader(BinaryWriter bw, uint appId, bool hasBinaryVdfSha1)
        {
            bw.Write(appId);              // appid
            bw.Write(123u);               // size (unused by tests)
            bw.Write(0u);                 // state
            bw.Write(0u);                 // lastUpdate
            bw.Write((ulong)0);           // accessToken
            bw.Write(new byte[20]);       // text VDF sha1
            bw.Write(0u);                 // changeNumber
            if (hasBinaryVdfSha1) bw.Write(new byte[20]);
        }

        // -- KV1 binary writers ---------------------------------------------

        private static void WriteInlineSubsectionBegin(BinaryWriter bw, string key)
        {
            bw.Write((byte)0x00);        // value type: nested subsection
            WriteNullTerminatedString(bw, key);
        }

        private static void WriteInlineStringEntry(BinaryWriter bw, string key, string value)
        {
            bw.Write((byte)0x01);        // value type: string
            WriteNullTerminatedString(bw, key);
            WriteNullTerminatedString(bw, value);
        }

        private static void WriteIndexedSubsectionBegin(BinaryWriter bw, string key, List<string> strings)
        {
            bw.Write((byte)0x00);
            bw.Write((uint)IndexOf(strings, key));
        }

        private static void WriteIndexedStringEntry(BinaryWriter bw, string key, string value, List<string> strings)
        {
            bw.Write((byte)0x01);
            bw.Write((uint)IndexOf(strings, key));
            WriteNullTerminatedString(bw, value);
        }

        private static void WriteSubsectionEnd(BinaryWriter bw)
        {
            bw.Write((byte)0x08);
        }

        private static void WriteNullTerminatedString(BinaryWriter bw, string s)
        {
            bw.Write(Encoding.Default.GetBytes(s));
            bw.Write((byte)0);
        }

        private static int IndexOf(List<string> strings, string s)
        {
            int idx = strings.IndexOf(s);
            if (idx < 0)
            {
                strings.Add(s);
                idx = strings.Count - 1;
            }
            return idx;
        }
    }
}
