#region Licence
/****************************************************************
 *  Filename: VdfTxtDecoder.cs
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
using System.IO;

namespace LeapVR.Utilities.Steam.Steam.VDF.Binary {
    /// <summary>
    /// Class to load txt vdf file
    /// </summary>
    public class VdfTxtDecoder
    {
        string[] lines;
        string filePath;
        int i = 0;
        VdfData data = new VdfData();
        public VdfData Data { get => data; }

        public VdfTxtDecoder(string path)
        {
            filePath = path;
        }

        public void Decode()
        {
            lines = File.ReadAllLines(filePath);
            i = 0;
            data = FormatLines();
        }

        private VdfData FormatLines()
        {
            string dictKey = "";
            VdfData mainData = new VdfData();
            while (i < lines.Length)
            {
                string line = lines[i++];
                while (line.StartsWith("\t"))
                {
                    line = line.Substring(1);
                }
                line = line.Replace("\"", "");
                switch (line.Replace("\t", ""))
                {
                    case "{":
                        object dictValue = FormatLines();
                        mainData.Add(dictKey, dictValue);
                        break;
                    case "}":
                        return mainData;
                    default:
                        // Dict name
                        if (line.IndexOf('\t') == -1)
                        {
                            dictKey = line;
                        }
                        // Main data
                        else
                        {
                            // Be aware that there are some value contain '\t'
                            // So split line with "\t\t" instead of '\t'
                            string[] d = line.Split(new string[] { "\t\t" }, StringSplitOptions.RemoveEmptyEntries);
                            mainData.Add(d[0], d[1]);
                        }
                        break;
                }
            }
            return mainData;
        }
    }
}