#region Licence
/****************************************************************
 *  Filename: IDOTA2Match.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;
using SteamWebAPI2.Models.DOTA2;
using Steam.Models.DOTA2;

namespace SteamWebAPI2.Interfaces
{
    public interface IDOTA2Match
    {
        Task<IReadOnlyCollection<LeagueModel>> GetLeagueListingAsync(string language);
        Task<IReadOnlyCollection<LiveLeagueGameModel>> GetLiveLeagueGamesAsync(int? leagueId = default(int?), long? matchId = default(long?));
        Task<MatchDetailModel> GetMatchDetailsAsync(long matchId);
        Task<MatchHistoryModel> GetMatchHistoryAsync(int? heroId = default(int?), int? gameMode = default(int?), int? skill = default(int?), string minPlayers = "", string accountId = "", string leagueId = "", long? startAtMatchId = default(long?), string matchesRequested = "", string tournamentGamesOnly = "");
        Task<IReadOnlyCollection<MatchHistoryMatchModel>> GetMatchHistoryBySequenceNumberAsync(long? startAtMatchSequenceNumber = default(long?), int? matchesRequested = default(int?));
        Task<IReadOnlyCollection<TeamInfoModel>> GetTeamInfoByTeamIdAsync(long? startAtTeamId = default(long?), int? teamsRequested = default(int?));
    }
}