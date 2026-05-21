#region Licence
/****************************************************************
 *  Filename: LicenseStatus.cs
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

namespace LeapVR.Shell.Domain.Models.Station
{
    public enum LicenseStatus
    {
        Unknown = 0,
        LicenseValid = 1,
        LicenseRevoked = 2,
        LicenseSuspended = 3,
        LicenseNotDeployed = 4,
        LicenseNotLinked = 5,
        LicenseNotFound = 6,
        StationRevoked = 7,
        StationSuspended = 8,
        LicenseInUse = 9,
        InvalidUsernamePassword = 10,
    }
}
