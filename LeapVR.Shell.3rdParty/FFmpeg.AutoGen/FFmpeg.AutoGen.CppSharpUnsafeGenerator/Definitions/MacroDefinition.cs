#region Licence
/****************************************************************
 *  Filename: MacroDefinition.cs
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
    internal class MacroDefinition : IDefinition, ICanGenerateXmlDoc
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public string TypeName { get; set; }
        public bool IsValid { get; set; }
        public bool IsConst { get; set; }
        public string Content { get; set; }
    }
}