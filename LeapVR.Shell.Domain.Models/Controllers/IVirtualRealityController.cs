#region Licence
/****************************************************************
 *  Filename: IVirtualRealityController.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Domain.Models.Controllers
{
    public interface IVirtualRealityController : IExecutionMessageReceiver, IStationMessageReceiver
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
        /// Gets all availible VR Modules.
        /// </summary>
        /// <value>
        /// Availible VR modules.
        /// </value>
        IEnumerable<IVrModule> AvailibleVrModules { get; }

        /// <summary>
        /// Observable that signals when the VR Module state changed.
        /// </summary>
        /// <value>
        /// The Observable for the VR Module State.
        /// </value>
        IObservable<VrModuleState> WhenVrModuleStateChanged { get; }

        /// <summary>
        /// Observable that signals when the VR GUI state changed.
        /// </summary>
        /// <value>
        /// The Observable for the VR GUI State.
        /// </value>
        IObservable<VrGuiState> WhenVrGuiStateChanged { get; }

        Task ChangeModeAsync(VrMode requestedMode);
        Task SetActiveVRModuleAsync(IVrModule module);

    }
}
