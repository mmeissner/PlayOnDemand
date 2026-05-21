#region Licence
/****************************************************************
 *  Filename: Kernel32Util.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Win.WinApi.Win32;

namespace LeapVR.Shared.Lib.Win.WinApi
{
    public static class Kernel32Util
    {
        public static bool DriveFreeBytes(string folderName, out ulong freespace)
        {
            freespace = 0;
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException($"{nameof(folderName)} can not be null or empty");
            }

            if (!folderName.EndsWith("\\"))
            {
                folderName += '\\';
            }

            ulong dummy1 = 0, dummy2 = 0;

            if (Kernel32.GetDiskFreeSpaceEx(folderName, out var free, out dummy1, out dummy2))
            {
                freespace = free;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
