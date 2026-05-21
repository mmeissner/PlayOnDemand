#region Licence
/****************************************************************
 *  Filename: AppinfoEncoder.cs
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
using System.Linq;
using System.Text;

namespace LeapVR.Utilities.Steam.Steam.VDF.Binary {
    /// <summary>
    /// Class to save data to binary .vdf file
    /// </summary>
    public class AppinfoEncoder
    {
        // Some mark byte
        const byte LastSection = 0x00;
        const byte SectionEnd = 0x08;
        readonly byte[] LastApp = { 0x00, 0x00, 0x00, 0x00 };
        const byte TypeSection = 0x00;
        const byte TypeString = 0x01;
        const byte TypeInt32 = 0x02;
        const byte TypeInt64 = 0x07;

        BinaryWriter _bw = null;
        Dictionary<string, GameData> _data = null;
        uint _version;
        uint _universe;

        public AppinfoEncoder(AppinfoDecoder appinfo, BinaryWriter bw)
        {
            _bw = bw;
            _data = appinfo.Data;
            _version = appinfo.Version;
            _universe = appinfo.Universe;
        }

        public void Encode()
        {
            // VDF Header
            _bw.Write(_version);
            _bw.Write(_universe);
            foreach (KeyValuePair<string, GameData> app in _data)
            {
                string appid = app.Key;
                GameData appdata = app.Value;
                // Game Header
                _bw.Write(Convert.ToUInt32(appid));
                _bw.Write(appdata.Size);
                _bw.Write(appdata.State);
                _bw.Write(appdata.LastUpdate);
                _bw.Write(appdata.AccessToken);
                _bw.Write(appdata.CheckSum);
                _bw.Write(appdata.ChangeNumber);
                if (_version == 0x07564427)
                {
                    EncodeSection(appdata.Sections);
                }
                else
                {
                    foreach (KeyValuePair<string, object> section in appdata.Sections)
                    {
                        string sectionName = section.Key;
                        Dictionary<string, string> sectionData = (Dictionary<string, string>)section.Value;
                        _bw.Write(EncodeString(sectionData["sectionID"]));
                        EncodeSection((Dictionary<string, object>)section.Value, true);
                    }
                    _bw.Write(LastSection);
                }
            }
            _bw.Write(LastApp);
        }

        private void EncodeSection(Dictionary<string, object> sectionData, bool rootSection = false)
        {
            foreach (KeyValuePair<string, object> data in sectionData)
            {
                string key = data.Key;
                object value = data.Value;
                if (key == "sectionID")
                    continue;
                //if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                if (value.GetType() == typeof(VdfData))
                {
                    _bw.Write(TypeSection);
                    _bw.Write(EncodeString(key));
                    EncodeSection((Dictionary<string, object>)value);
                }
                else if (value.GetType() == typeof(string))
                {
                    _bw.Write(TypeString);
                    _bw.Write(EncodeString(key));
                    _bw.Write(EncodeString((string)value));
                }
                else if (value.GetType() == typeof(uint))
                {
                    _bw.Write(TypeInt32);
                    _bw.Write(EncodeString(key));
                    _bw.Write((uint)value);
                }
                else if (value.GetType() == typeof(ulong))
                {
                    _bw.Write(TypeInt64);
                    _bw.Write(EncodeString(key));
                    _bw.Write((ulong)value);
                }
                else
                {
                    throw new Exception("Unknown value type: " + value.GetType().ToString());
                }
            }
            _bw.Write(SectionEnd);
            if (rootSection)
            {
                // One additional 0x08 byte at the end of the root subsection.
                _bw.Write(SectionEnd);
            }
        }

        private byte[] EncodeString(string s)
        {
            // Remove additional random string
            byte[] b = Encoding.Default.GetBytes(System.Text.RegularExpressions.Regex.Replace(s, @"__\d+", ""));
            List<byte> l = b.ToList();
            l.Add(0x00);
            b = l.ToArray();
            return b;
        }
    }
}