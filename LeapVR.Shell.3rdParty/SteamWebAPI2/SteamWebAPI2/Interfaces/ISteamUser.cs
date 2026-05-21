#region Licence
/****************************************************************
 *  Filename: ISteamUser.cs
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
using Steam.Models.SteamCommunity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamWebAPI2.Interfaces
{
    public interface ISteamUser
    {
        Task<IReadOnlyCollection<FriendModel>> GetFriendsListAsync(long steamId, string relationship = "");

        Task<IReadOnlyCollection<PlayerBansModel>> GetPlayerBansAsync(long steamId);

        Task<IReadOnlyCollection<PlayerBansModel>> GetPlayerBansAsync(IReadOnlyCollection<long> steamIds);

        Task<PlayerSummaryModel> GetPlayerSummaryAsync(long steamId);

        Task<List<PlayerSummaryModel>> GetPlayerSummariesAsync(List<long> steamIds);

        Task<IReadOnlyCollection<long>> GetUserGroupsAsync(long steamId);

        Task<ulong> ResolveVanityUrlAsync(string vanityUrl, int? urlType = default(int?));

        Task<SteamCommunityProfileModel> GetCommunityProfileAsync(long steamId);
    }
}