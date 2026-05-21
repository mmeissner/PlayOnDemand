#region Licence
/****************************************************************
 *  Filename: VDFFile.cs
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
namespace LeapVR.Utilities.Steam.Steam.VDF
{
    public class VDFFile : NestedElementFile
    {
        public VDFFile(string file_name) : base(file_name)
        {
        }


        protected static string DeEscapeString(string value)
        {
            return value.Replace(@"\\", @"\");
        }
    }
}