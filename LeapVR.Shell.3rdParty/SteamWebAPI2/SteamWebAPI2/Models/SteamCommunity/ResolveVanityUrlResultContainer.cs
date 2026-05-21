#region Licence
/****************************************************************
 *  Filename: ResolveVanityUrlResultContainer.cs
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

namespace SteamWebAPI2.Models.SteamCommunity
{
    internal class ResolveVanityUrlResult
    {
        [JsonProperty("steamid")]
        public ulong SteamId { get; set; }

        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    internal class ResolveVanityUrlResultContainer
    {
        [JsonProperty("response")]
        public ResolveVanityUrlResult Result { get; set; }
    }
}