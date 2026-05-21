#region Licence
/****************************************************************
 *  Filename: FixedArrayDefinition.cs
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
    internal class FixedArrayDefinition : TypeDefinition, IDefinition
    {
        public TypeDefinition ElementType { get; set; }
        public int Size { get; set; }
        public bool IsPrimitive { get; set; }
    }
}