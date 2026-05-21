#region Licence
/****************************************************************
 *  Filename: LeagueModel.cs
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
using System;

namespace Steam.Models.DOTA2
{
    public class LeagueModel
    {
        public string Name { get; set; }
        public int LeagueId { get; set; }
        public string Description { get; set; }
        public string TournamentUrl { get; set; }
        public int ItemDef { get; set; }
        public string ImageInventoryPath { get; private set; }
        public string ImageBannerPath { get; private set; }
        public string NameLocalized { get; private set; }
        public string DescriptionLocalized { get; private set; }
        public string TypeName { get; private set; }
        public string Tier { get; private set; }
        public string Location { get; private set; }

        public override string ToString()
        {
            string result = String.Format("Name: {0}", NameLocalized);
            if (!String.IsNullOrEmpty(Tier))
            {
                result += String.Format(", Tier: {0}", Tier);
            }
            if (!String.IsNullOrEmpty(Location))
            {
                result += String.Format(", Location: {0}", Location);
            }
            return result;
        }
    }
}