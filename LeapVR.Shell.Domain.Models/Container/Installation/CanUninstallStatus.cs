#region Licence
/****************************************************************
 *  Filename: CanUninstallStatus.cs
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

namespace LeapVR.Shell.Domain.Models.Container.Installation
{
    /// <summary>
    /// Specifies ability to uninstall application from LeapVR system.
    /// </summary>
    public enum CanUninstallStatus
    {
        Unknown = 0,

        ReadyToUninstall = 1,
        BrokenCanUninstall = 2,

        NotInstalled = 10,
        BrokenCannotUninstall = 11,
    }

}
