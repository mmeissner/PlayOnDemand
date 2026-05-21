#region Licence
/****************************************************************
 *  Filename: SteamAppListResultContainer.cs
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

namespace SteamWebAPI2.Models
{
    internal class SteamApp
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    internal class SteamAppListResult
    {
        [JsonProperty("apps")]
        public IList<SteamApp> Apps { get; set; }
    }

    internal class SteamAppListResultContainer
    {
        [JsonProperty("applist")]
        public SteamAppListResult Result { get; set; }
    }
}