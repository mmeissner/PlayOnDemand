#region Licence
/****************************************************************
 *  Filename: SteamAppUpToDateCheckResultContainer.cs
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
    internal class SteamAppUpToDateCheckResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("up_to_date")]
        public bool UpToDate { get; set; }

        [JsonProperty("version_is_listable")]
        public bool VersionIsListable { get; set; }

        [JsonProperty("required_version")]
        public int RequiredVersion { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    internal class SteamAppUpToDateCheckResultContainer
    {
        [JsonProperty("response")]
        public SteamAppUpToDateCheckResult Result { get; set; }
    }
}