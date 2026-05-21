#region Licence
/****************************************************************
 *  Filename: SchemaItemToolUsageModel.cs
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
    public class SchemaItemToolUsageModel
    {
        public int LeagueId { get; set; }

        public string Tier { get; set; }

        public string Location { get; set; }

        public bool Admin { get; set; }
    }
}