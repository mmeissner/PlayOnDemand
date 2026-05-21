#region Licence
/****************************************************************
 *  Filename: IShellClientInfo.cs
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

using System.Collections;
using System.Collections.Generic;

namespace LeapVR.Shell.Domain.Models.Station
{
    /// <summary>
    /// Contains details about Station (name, SN) and Location and Platform it belongs to.
    /// </summary>
    public interface IShellClientInfo :IEqualityComparer<IShellClientInfo>
    {
        string StationDisplayName { get; }
        string SerialNumber { get; }
        ShellVersionStatus VersionStatus { get; }
        LicenseStatus LicenseStatus { get; }
    }
}
