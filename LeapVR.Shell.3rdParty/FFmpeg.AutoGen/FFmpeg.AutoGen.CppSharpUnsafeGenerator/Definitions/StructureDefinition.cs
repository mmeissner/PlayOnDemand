#region Licence
/****************************************************************
 *  Filename: StructureDefinition.cs
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
    internal class StructureDefinition : NamedDefinition, IDefinition
    {
        public StructureField[] Fileds { get; set; } = { };
        public bool IsComplete { get; set; }
    }
}