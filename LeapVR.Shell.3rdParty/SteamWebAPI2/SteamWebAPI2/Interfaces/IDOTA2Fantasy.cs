#region Licence
/****************************************************************
 *  Filename: IDOTA2Fantasy.cs
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
using System.Threading.Tasks;
using SteamWebAPI2.Models.DOTA2;
using Steam.Models.DOTA2;

namespace SteamWebAPI2.Interfaces
{
    public interface IDOTA2Fantasy
    {
        Task<PlayerOfficialInfoModel> GetPlayerOfficialInfo(long steamId);
        Task<ProPlayerDetailModel> GetProPlayerList();
    }
}