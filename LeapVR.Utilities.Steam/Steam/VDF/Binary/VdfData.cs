#region Licence
/****************************************************************
 *  Filename: VdfData.cs
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

namespace LeapVR.Utilities.Steam.Steam.VDF.Binary
{
    public class VdfData : Dictionary<string, object>
    {
        public string GetString(string path)
        {
            string[] keys = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            VdfData data = Get(keys);
            if (data == null)
                return null;
            if (data.ContainsKey(keys[keys.Length - 1]) && data[keys[keys.Length - 1]].GetType() == typeof(string))
            {
                return (string)data[keys[keys.Length - 1]];
            }
            return null;
        }

        public VdfData GetVdf(string path)
        {
            string[] keys = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            VdfData data = Get(keys);
            if (data == null)
                return null;
            if (data.ContainsKey(keys[keys.Length - 1]) && data[keys[keys.Length - 1]].GetType() == typeof(VdfData))
            {
                return (VdfData)data[keys[keys.Length - 1]];
            }
            return null;
        }

        private VdfData Get(string[] keys)
        {
            VdfData data = this;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (data.ContainsKey(keys[i]) && data[keys[i]].GetType() == typeof(VdfData))
                {
                    data = (VdfData)data[keys[i]];
                }
                else
                {
                    return null;
                }
            }
            return data;
        }
    }

    /// <summary>
    /// Save vdf game data.
    /// </summary>
    public struct GameData
    {
        public uint Size;
        public uint State;
        public uint LastUpdate;
        public ulong AccessToken;
        public byte[] CheckSum;
        public uint ChangeNumber;
        public VdfData Sections;
    }
}
