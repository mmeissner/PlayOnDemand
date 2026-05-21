#region Licence
/****************************************************************
 *  Filename: ILoginModeViewModel.cs
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

namespace LeapVR.Shell.UI.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Representing a view model loaded with corresponding business mode properly  and awaits to start
    /// </summary>
    public interface ILoginModeViewModel : ILoginPageViewModel
    {
        LoginMode Mode { get; }
    }

    public enum LoginMode
    {
        BusinessMode,
        RestrictedOpenMode,
        OpenMode
    }

}
