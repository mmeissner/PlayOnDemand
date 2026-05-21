#region Licence
/****************************************************************
 *  Filename: ServerStatusMatchmakingModel.cs
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
    public class ServerStatusMatchmakingModel
    {
        public string Scheduler { get; set; }

        public int OnlineServers { get; set; }

        public int OnlinePlayers { get; set; }

        public int SearchingPlayers { get; set; }

        public int SearchSecondsAverage { get; set; }
    }
}