#region Licence
/****************************************************************
 *  Filename: IVrModule.cs
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

namespace LeapVR.Shell.Modules.Interfaces.Vr
{
    /// <summary>
    /// Interface for VR Driver management
    /// </summary>
    public interface IVrModule : IBaseModule
    {
        string DisplayName { get; }

        /// <summary>
        /// Indicates if this driver is available to use.
        /// </summary>
        bool IsAvailible { get; }

        /// <summary>
        /// Indicates if this driver is running now.
        /// </summary>
        bool IsRunning { get; }

        bool StartVrDriver(bool disableInteraction,TransparencyAreaCallBack transparencyAreaCallback,Action restartVrGui);

        /// <summary>
        /// Request to start this VR Driver.
        /// </summary>
        /// <returns>Boolean indicating success/failure.</returns>
        bool StartOnlyVrDriver();

        /// <summary>
        /// Request to stop this VR Driver.
        /// </summary>
        void StopVrDriver();

        /// <summary>
        /// Determines whether this module supports other IVrModules
        /// </summary>
        /// <param name="submoduleGuid">The submodule unique identifier.</param>
        /// <returns>
        ///   <c>true</c> if [has module support] [the specified submodule unique identifier]; otherwise, <c>false</c>.
        /// </returns>
        bool HasModuleSupport(Guid submoduleGuid);
    }
}
