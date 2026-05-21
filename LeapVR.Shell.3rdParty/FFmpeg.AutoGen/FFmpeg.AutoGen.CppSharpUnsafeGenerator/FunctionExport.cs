#region Licence
/****************************************************************
 *  Filename: FunctionExport.cs
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
using System.Diagnostics;

namespace FFmpeg.AutoGen.CppSharpUnsafeGenerator
{
    [DebuggerDisplay("{Name}, {Library}")]
    internal class FunctionExport
    {
        public string Name { get; set; }
        public string LibraryName { get; set; }
        public int LibraryVersion { get; set; }
    }
}