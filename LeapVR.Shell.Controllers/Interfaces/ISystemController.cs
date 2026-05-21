#region Licence
/****************************************************************
 *  Filename: ISystemController.cs
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
using System.Globalization;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Manages global configuration.
    /// </summary>
    public interface ISystemController : IController
    {
        /// <summary>
        /// Details about local machine wrapped.
        /// </summary>
        ILocalMachine LocationMachine { get; }

        void Initialize();
    }
}
