#region Licence
/****************************************************************
 *  Filename: IPlayerService.cs
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
    public interface IPlayerService
    {
        Task<BadgesResultModel> GetBadgesAsync(long steamId);

        Task<IReadOnlyCollection<BadgeQuestModel>> GetCommunityBadgeProgressAsync(long steamId, int? badgeId = default(int?));

        Task<OwnedGamesResultModel> GetOwnedGamesAsync(long steamId, bool? includeAppInfo = default(bool?), bool? includeFreeGames = default(bool?), IReadOnlyCollection<int> appIdsToFilter = null);

        Task<RecentlyPlayedGamesResultModel> GetRecentlyPlayedGamesAsync(long steamId);

        Task<int> GetSteamLevelAsync(long steamId);

        Task<string> IsPlayingSharedGameAsync(long steamId, int appId);
    }
}