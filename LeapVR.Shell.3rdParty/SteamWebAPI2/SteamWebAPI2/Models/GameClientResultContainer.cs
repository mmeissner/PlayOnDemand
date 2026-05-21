#region Licence
/****************************************************************
 *  Filename: GameClientResultContainer.cs
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

namespace SteamWebAPI2.Models
{
    internal class GameClientResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("deploy_version")]
        public int DeployVersion { get; set; }

        [JsonProperty("active_version")]
        public int ActiveVersion { get; set; }
    }

    internal class GameClientResultContainer
    {
        [JsonProperty("result")]
        public GameClientResult Result { get; set; }
    }
}