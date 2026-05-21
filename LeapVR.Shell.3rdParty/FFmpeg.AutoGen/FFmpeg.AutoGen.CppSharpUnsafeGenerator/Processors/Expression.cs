#region Licence
/****************************************************************
 *  Filename: Expression.cs
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
namespace FFmpeg.AutoGen.CppSharpUnsafeGenerator.Processors
{
    internal class Expression
    {
        public object Value { get; set; }
        public string TypeName { get; set; }

        internal class Constant : Expression
        {
        }

        internal class Group : Expression
        {
        }

        internal class Binary : Expression
        {
            public string Operator { get; set; }
            public Expression Left { get; set; }
            public Expression Right { get; set; }
        }
    }
}