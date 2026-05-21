#region Licence
/****************************************************************
 *  Filename: VDFBinaryUtils.cs
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
using System.IO;

namespace LeapVR.Utilities.Steam.Steam.VDF.Binary
{
    public class VdfBinaryUtils
    {

        /// <summary>
        /// Load binary vdf content into Dictionary.
        /// </summary>
        /// <param name="filePath">Full path of vdf file</param>
        /// <returns>A Dictionary with Appinfo data.</returns>
        public static AppinfoDecoder LoadBinaryVdf(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                AppinfoDecoder decoder = new AppinfoDecoder(br);
                decoder.Decode();
                return decoder;
            }
        }

        /// <summary>
        /// Save dictionary into binary vdf file.
        /// </summary>
        /// <param name="appinfo">An appinfo object to serialize.</param>
        /// <param name="filePath">Serialized vdf file path.</param>
        public static void SaveBinaryVdf(AppinfoDecoder appinfo, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                AppinfoEncoder encoder = new AppinfoEncoder(appinfo, bw);
                encoder.Encode();
            }
        }

        public static VdfTxtDecoder LoadTxtVdf(string filePath)
        {
            VdfTxtDecoder decoder = new VdfTxtDecoder(filePath);
            decoder.Decode();
            return decoder;
        }

        public static void SaveTxtVdf(VdfTxtDecoder vdfData, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                VdfTxtEncoder encoder = new VdfTxtEncoder(vdfData);
                string vdfString = encoder.Encoder();
                sw.Write(vdfString);
            }
        }
    }
}
