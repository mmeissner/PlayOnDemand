#region Licence
/****************************************************************
 *  Filename: PlayingSharedGameResultContainer.cs
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

namespace SteamWebAPI2.Models.SteamPlayer
{
    internal class PlayingSharedGameResult
    {
        [JsonProperty("lender_steamid")]
        public string LenderSteamId { get; set; }
    }

    internal class PlayingSharedGameResultContainer
    {
        [JsonProperty("response")]
        public PlayingSharedGameResult Result { get; set; }
    }
}