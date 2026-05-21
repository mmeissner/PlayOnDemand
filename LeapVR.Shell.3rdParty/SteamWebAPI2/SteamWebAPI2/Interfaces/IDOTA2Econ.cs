#region Licence
/****************************************************************
 *  Filename: IDOTA2Econ.cs
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
using Steam.Models.DOTA2;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamWebAPI2.Interfaces
{
    public interface IDOTA2Econ
    {
        Task<IReadOnlyCollection<GameItemModel>> GetGameItemsAsync(string language = "");

        Task<IReadOnlyCollection<HeroModel>> GetHeroesAsync(string language = "", bool itemizedOnly = false);

        Task<string> GetItemIconPathAsync(string iconName, string iconType = "");

        Task<IReadOnlyCollection<RarityModel>> GetRaritiesAsync(string language = "");

        Task<int> GetTournamentPrizePoolAsync(int? leagueId = null);
    }
}