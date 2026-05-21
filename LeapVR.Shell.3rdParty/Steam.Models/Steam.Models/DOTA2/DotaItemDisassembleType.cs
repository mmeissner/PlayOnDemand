#region Licence
/****************************************************************
 *  Filename: DotaItemDisassembleType.cs
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
namespace Steam.Models.DOTA2
{
    public sealed class DotaItemDisassembleType : DotaEnumType
    {
        public static readonly DotaItemDisassembleType UNKNOWN = new DotaItemDisassembleType("DOTA_ITEM_DISASSEMBLE_UNKNOWN", "Unknown", "It is unknown if this item can be disassembled.");
        public static readonly DotaItemDisassembleType NEVER = new DotaItemDisassembleType("DOTA_ITEM_DISASSEMBLE_NEVER", "Never", "This item can never be disassembled.");
        public static readonly DotaItemDisassembleType ALWAYS = new DotaItemDisassembleType("DOTA_ITEM_DISASSEMBLE_ALWAYS", "Always", "This item can always be disassembled.");

        public DotaItemDisassembleType(string key, string displayName, string description)
            : base(key, displayName, description)
        { }
    }
}