#region Licence
/****************************************************************
 *  Filename: TeamInfoModel.cs
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
using System.Collections.Generic;

namespace Steam.Models.DOTA2
{
    public class TeamInfoModel
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public long TimeCreated { get; set; }
        public string Rating { get; set; }
        public long Logo { get; set; }
        public long LogoSponsor { get; set; }
        public string CountryCode { get; set; }
        public string Url { get; set; }
        public int GamesPlayedWithCurrentRoster { get; set; }
        public uint AdminAccountId { get; set; }
        public IReadOnlyCollection<int> PlayerIds { get; set; }
        public IReadOnlyCollection<int> LeagueIds { get; set; }
    }
}