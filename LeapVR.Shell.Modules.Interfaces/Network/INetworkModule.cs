#region Licence
/****************************************************************
 *  Filename: INetworkModule.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Modules.Interfaces.Network
{
    public interface INetworkModule : IDisposable
    {
        IObservable<NetworkConnectionStatus> WhenNetworkConnectionChanged { get; }
        NetworkConnectionStatus NetworkConnectionStatus { get; }
    }

}
