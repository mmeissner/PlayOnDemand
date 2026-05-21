#region Licence
/****************************************************************
 *  Filename: AbilitySpecialSchemaItemModel.cs
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
    public class AbilitySpecialSchemaItemModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string VarType { get; set; }
        public string LinkedSpecialBonus { get; set; }
    }
}