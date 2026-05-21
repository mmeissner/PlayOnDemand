#region Licence
/****************************************************************
 *  Filename: FunctionDefinition.cs
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
    internal class FunctionDefinition : IDefinition, ICanGenerateXmlDoc
    {
        public TypeDefinition ReturnType { get; set; }
        public string LibraryName { get; set; }
        public int LibraryVersion { get; set; }
        public FunctionParameter[] Parameters { get; set; }
        public bool IsObsolete { get; set; }
        public bool SuppressUnmanagedCodeSecurity { get; set; }
        public string ObsoleteMessage { get; set; }
        public string Content { get; set; }
        public string ReturnComment { get; set; }
        public string Name { get; set; }
    }
}