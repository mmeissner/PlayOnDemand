#region Licence
/****************************************************************
 *  Filename: IAdministrationViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-11-16
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
    public interface IAdministrationViewModel : IScreen
    {
        void UnlockSystem();
    }
}
