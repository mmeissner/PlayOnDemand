#region Licence
/****************************************************************
 *  Filename: IShellConnectionController.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
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
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Domain.Models.Controllers
{
    public interface IShellConnectionController : IController, IStationMessageReceiver
    {
        IShellClientDisplayInfo ClientDisplayInfo { get; }
        ServerConnectionStatus ServerConnectionStatus { get; }
        NetworkConnectionStatus NetworkConnectionStatus { get; }
        ShellVersionStatus ShellVersionStatus { get; }
        LicenseStatus LicenseStatus { get; }
        ISessionSettings SessionSettings { get; }
        IObservable<ShellVersionStatus> WhenShellVersionStatusChanged { get; }
        IObservable<LicenseStatus> WhenLicenseStatusChanged { get; }
        IObservable<IShellClientDisplayInfo> WhenShellClientDisplayInfoUpdated { get; }
        IObservable<NetworkConnectionStatus> WhenNetworkConnectionChanged { get; }
        IObservable<ServerConnectionStatus> WhenServerConnectionChanged { get; }
        IObservable<ISessionSettings> WhenSessionSetupChanged { get; }
    }
}
