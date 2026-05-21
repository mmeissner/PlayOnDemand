#region Licence
/****************************************************************
 *  Filename: EnumerationItem.cs
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
namespace FFmpeg.AutoGen.CppSharpUnsafeGenerator.Definitions
{
    internal class EnumerationItem : ICanGenerateXmlDoc
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Content { get; set; }
    }
}