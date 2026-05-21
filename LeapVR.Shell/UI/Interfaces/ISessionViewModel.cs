#region Licence
/****************************************************************
 *  Filename: ISessionViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-9-1
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
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.UI.Interfaces
{
    /// <summary>
    /// Representing a view model that holds the information of a <see cref="IUISession"/>.
    /// </summary>
    public interface ISessionViewModel
    {
        TimeSpan Played { get; }
    }
}
