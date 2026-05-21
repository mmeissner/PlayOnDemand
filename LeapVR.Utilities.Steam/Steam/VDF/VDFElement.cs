#region Licence
/****************************************************************
 *  Filename: VDFElement.cs
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
#region Using Directives
using System.Collections.Generic;
#endregion

namespace LeapVR.Utilities.Steam.Steam.VDF
{
    public class VDFElement : NestedElement
    {
        public new Dictionary<string, VDFElement> Children { get; protected set; }

        public VDFElement(Queue<string> read)
            : base(read)
        {
        }
    }
}