#region Licence
/****************************************************************
 *  Filename: LinuxNativeMethods.cs
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
using System.Runtime.InteropServices;

namespace FFmpeg.AutoGen.Native
{
    internal static class LinuxNativeMethods
    {
        public const int RTLD_NOW = 0x002;

        private const string Libdl = "libdl.so.2";

        [DllImport(Libdl)]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(Libdl)]
        public static extern IntPtr dlopen(string fileName, int flag);
    }
}