#region Licence
/****************************************************************
 *  Filename: FriendListResultContainer.cs
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
using SteamWebAPI2.Utilities.JsonConverters;
using System;
using System.Collections.Generic;

namespace SteamWebAPI2.Models.SteamCommunity
{
    internal class Friend
    {
        [JsonProperty("steamid")]
        public string SteamId { get; set; }

        [JsonProperty("relationship")]
        public string Relationship { get; set; }

        [JsonProperty("friend_since")]
        [JsonConverter(typeof(UnixTimeJsonConverter))]
        public DateTime FriendSince { get; set; }
    }

    internal class FriendsList
    {
        [JsonProperty("friends")]
        public IList<Friend> Friends { get; set; }
    }

    internal class FriendsListResultContainer
    {
        [JsonProperty("friendslist")]
        public FriendsList Result { get; set; }
    }
}