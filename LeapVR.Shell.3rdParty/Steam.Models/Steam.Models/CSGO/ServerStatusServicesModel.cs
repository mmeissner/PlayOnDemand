#region Licence
/****************************************************************
 *  Filename: ServerStatusServicesModel.cs
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
namespace Steam.Models.CSGO
{
    public class ServerStatusServicesModel
    {
        public string SessionsLogon { get; set; }
        public string SteamCommunity { get; set; }
        public string IEconItems { get; set; }
        public string Leaderboards { get; set; }
    }
}