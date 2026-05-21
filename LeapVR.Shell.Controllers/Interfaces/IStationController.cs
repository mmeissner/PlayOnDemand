#region Licence
/****************************************************************
 *  Filename: IStationController.cs
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
using System.Globalization;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.System;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// Controller responsible for monitoring and managing connection between Station and Server.
    /// </summary>
    public interface IStationController : IController, IDisposable
    {
        #region Initialization
        void Initialize(Action<StationMessage> msgToSystemCallback,
            Action<TerminationSignal> msgToSystemTermSignalCallback);
        #endregion

        #region StationControl and Execution Methods        
        /// <summary>
        /// Gets the Startion Mode <see cref="StationMode"/>
        /// </summary>
        /// <value>
        /// The mode the station is currently in.
        /// </value>
        StationMode Mode { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to restart the vr driver after each usage
        /// </summary>
        /// <value>
        ///   <c>true</c> if [force driver restart]; otherwise, <c>false</c>.
        /// </value>
        bool ForceVrDriverRestart { get; }

        /// <summary>
        /// Gets a value indicating whether it is allowed to interact with the VR Driver GUI.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable vr driver interaction]; otherwise, <c>false</c>.
        /// </value>
        bool DisableVrDriverInteraction { get; }

        /// <summary>
        /// Gets the software version.
        /// </summary>
        /// <value>
        /// The software version.
        /// </value>
        Version SoftwareVersion { get; }

        /// <summary>
        /// Gets a value indicating whether an app is currently in execution.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [currently executing]; otherwise, <c>false</c>.
        /// </value>
        bool CurrentlyExecuting { get; }

        /// <summary>
        /// Requests the execution of an application with no specific account information
        /// </summary>
        /// <param name="executeables">The executeables.</param>
        /// <returns></returns>
        bool RequestExecution(IExecuteable executeables);

        /// <summary>
        /// Requests the termination of all currently running applications.
        /// </summary>
        void TerminateApps();

        /// <summary>
        /// Quits all running Applications and
        /// resets the state of the station state in case of a failure of any used Drivers/Devices/Modules
        /// </summary>
        void ResetStationState();

        /// <summary>
        /// Requests the restart of the application.
        /// </summary>
        void RequestRestart();

        /// <summary>
        /// Requests the shutdown of the application.
        /// </summary>
        void RequestShutdown();

        /// <summary>
        /// Requests the shutdown of the application.
        /// </summary>
        void RequestPowerOff();

        /// <summary>
        /// Requests access to the admin area.
        /// </summary>
        void RequestAdminAccess();

        void OpenConnectDialog();

        List<StationMode> GetAvailableModes();

        Task SetStationModeAsync(StationMode mode);

        /// <summary>
        /// Sets if the VR-Driver should be restarted after each usage/app
        /// This is useful if some applications mess up the driver after they quit
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        void SetRestartVrDriver(bool value);


        /// <summary>
        /// Sets the value for <see cref="DisableVrDriverInteraction"/>
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        void SetDisableVrDriverInteraction(bool value);
        #endregion

    }
}
