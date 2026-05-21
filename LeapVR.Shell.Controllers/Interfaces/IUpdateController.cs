#region Licence
/****************************************************************
 *  Filename: IUpdateController.cs
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
using LeapVR.Shell.Domain.Models.Controllers;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// Responsible for checking if newer application version is available, downloading newer version and updating application to it.
    /// </summary>
    public interface IUpdateController : IController, IRunLevelMsgReceiver
    {
        /// <summary>
        /// Current version of application.
        /// </summary>
        Version CurrentVersion { get; }

        /// <summary>
        /// Holds single <see cref="IUpdateProcess"/> object that allows to control & monitor update process of the application.
        /// </summary>
        IUpdateProcess UpdateProcess { get; }
    }
}
