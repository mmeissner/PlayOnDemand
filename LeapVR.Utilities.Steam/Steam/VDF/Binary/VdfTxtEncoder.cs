#region Licence
/****************************************************************
 *  Filename: VdfTxtEncoder.cs
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
    /// <summary>
    /// Class to save data to txt .vdf file
    /// </summary>
    public class VdfTxtEncoder
    {
        enum BlockType
        {
            Inner = 1,
            Outer = 2
        }

        VdfTxtDecoder vdfData;
        int indent = -1;
        const string spliter = "\t\t";

        public VdfTxtEncoder(VdfTxtDecoder v)
        {
            vdfData = v;
        }

        public string Encoder()
        {
            string output = "";
            output = FormatData(vdfData.Data);
            return output;
        }

        private string FormatData(VdfData data)
        {
            string output = "";
            foreach (KeyValuePair<string, object> d in data)
            {
                string key = d.Key;
                object value = d.Value;
                if (value.GetType() == typeof(VdfData))
                {
                    indent++;
                    output += MakeBlock(key, FormatData((VdfData)value), BlockType.Outer);
                    indent--;
                }
                else if (value.GetType() == typeof(string))
                {
                    indent++;
                    output += MakeBlock(key, (string)value, BlockType.Inner);
                    indent--;
                }
                else
                {
                    throw new Exception("Unknown value type: " + value.GetType().ToString());
                }
            }
            return output;
        }

        private string MakeBlock(string key, string value, BlockType type)
        {
            string tabs = "";
            for (int i = 0; i < indent; i++)
            {
                tabs += "\t";
            }
            if (type == BlockType.Inner)
            {
                return tabs + AddQuote(key) + spliter + AddQuote(value) + "\n";
            }
            else
            {
                return tabs + AddQuote(key) + "\n" + tabs + "{" + "\n" + value + tabs + "}" + "\n";
            }
        }

        private string AddQuote(string s)
        {
            return "\"" + s + "\"";
        }
    }
}
