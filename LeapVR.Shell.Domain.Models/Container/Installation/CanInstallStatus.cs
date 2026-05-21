#region Licence
/****************************************************************
 *  Filename: CanInstallStatus.cs
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
    /// Specifies ability to install application to LeapVR system.
    /// </summary>
    public enum CanInstallStatus
    {
        Unknown = 0,

        ReadyToInstall = 1,

        AlreadyInstalled = 10,
        NotEnoughSpace = 11,

        ContainerBroken = 20,
    }

}
