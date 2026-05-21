#region Licence
/****************************************************************
 *  Filename: MD5.cs
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
using System.Security.Cryptography;

namespace LeapVR.Shared.Lib.Win
{
    public static class MD5Algorithm
    {
        public static string GetFileHash(string filePathName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePathName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", String.Empty).ToLower();
                }
            }
        }
    }
}
