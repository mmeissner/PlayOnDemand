#region Licence
/****************************************************************
 *  Filename: UserGroupListResultContainer.cs
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

namespace SteamWebAPI2.Models.SteamCommunity
{
    internal class UserGroupGid
    {
        [JsonProperty("gid")]
        public long Gid { get; set; }
    }

    internal class UserGroupListResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("groups")]
        public IList<UserGroupGid> Groups { get; set; }
    }

    internal class UserGroupListResultContainer
    {
        [JsonProperty("response")]
        public UserGroupListResult Result { get; set; }
    }
}