#region Licence
/****************************************************************
 *  Filename: IVirtualRealityController.cs
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
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.Controllers.Interfaces
{
    public interface IVirtualRealityController : IExecutionMessageReceiver, IRunLevelMsgReceiver
    {
        VrMode Mode { get; }

        /// <summary>
        /// Gets or sets the active vr module.
        /// </summary>
        /// <value>
        /// The active vr module.
        /// </value>
        IVrModule ActiveVrModule { get; }

        /// <summary>
        /// Gets the state of the vr module.
        /// </summary>
        /// <value>
        /// The state of the vr module.
        /// </value>
        VrModuleState VrModuleState { get; }

        /// <summary>
        /// Gets the state of the VR GUI.
        /// </summary>
        /// <value>
        /// The state of the VR GUI.
        /// </value>
        VrGuiState VrGuiState { get; }

        /// <summary>
        /// Gets all available VR Modules.
        /// </summary>
        /// <value>
        /// Available VR modules.
        /// </value>
        IEnumerable<IVrModule> AvailableVrModules { get; }

        /// <summary>
        /// Observable that signals when the VR Module state changed.
        /// </summary>
        /// <value>
        /// The Observable for the VR Module State.
        /// </value>
        IObservable<VrModuleState> WhenVrModuleStateChanged { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to restart the vr driver after each usage
        /// This is useful if some applications mess up the driver after they quit
        /// In this case the driver would reset from errors by restart
        /// </summary>
        /// <value>
        ///   <c>true</c> if [force driver restart]; otherwise, <c>false</c>.
        /// </value>
        bool ForceDriverRestart { get;set; }

        /// <summary>
        /// Gets or sets a value indicating whether to disable GUI Interaction with the VR Driver.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable driver interaction]; otherwise, <c>false</c>.
        /// </value>
        bool DisableDriverInteraction { get; set; }
        /// <summary>
        /// Observable that signals when the VR GUI state changed.
        /// </summary>
        /// <value>
        /// The Observable for the VR GUI State.
        /// </value>
        IObservable<VrGuiState> WhenVrGuiStateChanged { get; }

        /// <summary>
        /// Sets the Default Mode for VR to determine behavior.
        /// </summary>
        /// <param name="requestedMode">The requested VR Mode.</param>
        void ChangeMode(VrMode requestedMode);

        /// <summary>
        /// Sets the Default Mode for VR to determine behavior.
        /// </summary>
        /// <param name="requestedMode">The requested VR Mode.</param>
        Task ChangeModeAsync(VrMode requestedMode);

        /// <summary>
        /// Changes the VRModule to use
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns></returns>
        Task SetActiveVRModuleAsync(IVrModule module);

        /// <summary>
        /// Sets the required values to let the VR Module interact with the UI.
        /// Allows the app to create a transparent Area through that a VR Driver can be seen
        /// In this case the state of the VR Devices can still be seen
        /// </summary>
        /// <param name="transparencyAreaCallback">The transparency area callback.</param>
        void SetUiInteractivity(TransparencyAreaCallBack transparencyAreaCallback);

        /// <summary>
        /// Gets the selectable vr modules for that AppExecutables.
        /// This does not perform any check if the app is really compatible to these VR Types
        /// </summary>
        /// <param name="executablesUpdate">The executables update.</param>
        /// <returns></returns>
        IEnumerable<ISelectableVrType> GetSelectableVrModules(IAppPlatformData executablesUpdate);
    }
}
