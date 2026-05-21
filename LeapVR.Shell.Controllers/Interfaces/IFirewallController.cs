#region Licence
/****************************************************************
 *  Filename: IFirewallController.cs
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
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Controllers;

namespace LeapVR.Shell.Controllers.Interfaces
{
    public interface IFirewallController : IController
    {
        /// <summary>
        /// Gets current <see cref="FirewallState"/> for specific installed application with given <see cref="applicationGuid"/>.
        /// </summary>
        /// <param name="applicationGuid">Guid of installed application that firewall state will be get</param>
        /// <returns><see cref="FirewallState"/></returns>
        Task<FirewallState> GetFirewallStateAsync(Guid applicationGuid);
        /// <summary>
        /// Sets <see cref="FirewallState"/> for selected installed application with given <see cref="applicationGuid"/>.
        /// </summary>
        /// <param name="applicationGuid">Guid of installed application that firewall state will be get</param>
        /// <param name="newState">New <see cref="FirewallState"/> to set for application</param>
        void SetFirewallState(Guid applicationGuid, FirewallState newState);

        void RemoveAllRules(Guid applicationGuid);
    }
}