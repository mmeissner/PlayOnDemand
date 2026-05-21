#region Licence
/****************************************************************
 *  Filename: IConfigurationController.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// A controller that protect actions require permission control.
    /// </summary>
    public interface IConfigurationController
    {
        void Install();
        void Uninstall();
        void EnableOrDisableApplication(Guid applicationGuid, bool isEnabled);
        void ConfigureApplicationFirewallRules(Guid applicationGuid, FirewallState firewallRule);
        void SetStationMode(StationMode mode);
        void ResetApplicationStatistics(Guid applicationGuid);
        void ResetStationStatistics();
        void EnableOrDisableAccessWithPin(bool isEnabled);
        void ChangePin();
        void RequestStationUpdate();

    }
}
