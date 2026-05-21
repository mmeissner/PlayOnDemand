#region Licence
/****************************************************************
 *  Filename: DelegateDefinition.cs
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
    internal class DelegateDefinition : TypeDefinition, IDefinition, ICanGenerateXmlDoc
    {
        public string FunctionName { get; set; }
        public TypeDefinition ReturnType { get; set; }
        public FunctionParameter[] Parameters { get; set; }
        public string Content { get; set; }
    }
}