#region Licence
/****************************************************************
 *  Filename: HeroResultContainer.cs
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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamWebAPI2.Models.DOTA2
{
    internal class Hero
    {
        public string Name { get; set; }
        public int Id { get; set; }
        
        [JsonProperty(PropertyName = "localized_name")]
        public string LocalizedName { get; set; }
    }

    internal class HeroResult
    {
        public IList<Hero> Heroes { get; set; }
    }

    internal class HeroResultContainer
    {
        public HeroResult Result { get; set; }
    }
}