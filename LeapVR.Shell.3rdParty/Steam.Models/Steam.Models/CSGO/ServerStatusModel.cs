#region Licence
/****************************************************************
 *  Filename: ServerStatusModel.cs
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

namespace Steam.Models.CSGO
{
    public class ServerStatusModel
    {
        public ServerStatusAppModel App { get; set; }
        public ServerStatusServicesModel Services { get; set; }
        public IReadOnlyCollection<ServerStatusDatacenterModel> Datacenters { get; set; }
        public ServerStatusMatchmakingModel Matchmaking { get; set; }
    }
}