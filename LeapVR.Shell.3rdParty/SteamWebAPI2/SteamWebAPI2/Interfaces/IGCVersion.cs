#region Licence
/****************************************************************
 *  Filename: IGCVersion.cs
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
using SteamWebAPI2.Models;
using Steam.Models;

namespace SteamWebAPI2.Interfaces
{
    public interface IGCVersion
    {
        Task<GameClientResultModel> GetClientVersionAsync();
        Task<GameClientResultModel> GetServerVersionAsync();
    }
}