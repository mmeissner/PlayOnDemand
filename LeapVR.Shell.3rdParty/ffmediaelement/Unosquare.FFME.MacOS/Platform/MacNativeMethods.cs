#region Licence
/****************************************************************
 *  Filename: MacNativeMethods.cs
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
namespace Unosquare.FFME.MacOS.Platform
{
    using System;
    using FFmpeg.AutoGen;
    using Unosquare.FFME.Shared;

    public class MacNativeMethods : INativeMethods
    {
        public bool SetDllDirectory(string lpPathName)
        {
            if (lpPathName != null)
                ffmpeg.RootPath = lpPathName ?? string.Empty;
            return true;
        }

        public void CopyMemory(IntPtr destination, IntPtr source, uint length)
        {
            throw new NotImplementedException();
        }

        public void FillMemory(IntPtr destination, uint length, byte fill)
        {
            throw new NotImplementedException();
        }
    }
}
