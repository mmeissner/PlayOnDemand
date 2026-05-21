#region Licence
/****************************************************************
 *  Filename: ILoginPageViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using Caliburn.Micro;

namespace LeapVR.Shell.UI.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Representing a view model that locates in a specific stage of awaiting state. e.g <see cref="T:LeapVR.Shell.UI.Interfaces.IReadyToStartViewModel" /> and <see cref="T:LeapVR.Shell.UI.Interfaces.ILoginIntentionViewModel" />
    /// </summary>
    public interface ILoginPageViewModel : IScreen
    {

    }
}
