#region Licence
/****************************************************************
 *  Filename: MacroProcessor.cs
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
using System.Linq;
using CppSharp.AST;

namespace FFmpeg.AutoGen.CppSharpUnsafeGenerator.Processors
{
    internal class MacroProcessor
    {
        private readonly ASTProcessor _context;

        public MacroProcessor(ASTProcessor context)
        {
            _context = context;
        }

        public void Process(TranslationUnit translationUnit)
        {
            foreach (var macro in translationUnit.PreprocessedEntities.OfType<MacroDefinition>().Where(x => !string.IsNullOrWhiteSpace(x.Expression)))
            {
                var macroDefinition = new Definitions.MacroDefinition
                {
                    Name = macro.Name,
                    Expression = macro.Expression
                };
                _context.AddUnit(macroDefinition);
            }
        }
    }
}