#region Licence
/****************************************************************
 *  Filename: ShellClientInfo.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Services.Data
{
    public class ShellClientInfo : IShellClientInfo
    {
        public string StationDisplayName { get; set; }
        public string SerialNumber { get; set; }
        public ShellVersionStatus VersionStatus { get; set; }
        public LicenseStatus LicenseStatus { get; set; }

        public static ShellClientInfo CloneFrom(IShellClientInfo clientInfo)
        {
            return new ShellClientInfo()
            {
                StationDisplayName = clientInfo.StationDisplayName,
                SerialNumber = clientInfo.SerialNumber,
                VersionStatus = clientInfo.VersionStatus,
                LicenseStatus = clientInfo.LicenseStatus
            };
        }
        public bool Equals(IShellClientInfo x, IShellClientInfo y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.StationDisplayName, y.StationDisplayName) && string.Equals(x.SerialNumber, y.SerialNumber) && x.VersionStatus == y.VersionStatus && x.LicenseStatus == y.LicenseStatus;
        }

        public int GetHashCode(IShellClientInfo obj)
        {
            unchecked
            {
                var hashCode = (obj.StationDisplayName != null ? obj.StationDisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.SerialNumber != null ? obj.SerialNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)obj.VersionStatus;
                hashCode = (hashCode * 397) ^ (int)obj.LicenseStatus;
                return hashCode;
            }
        }
    }

    public class RemoteMode : QrCodeLoginSettingsBase
    {}

    public class LocalMode : AnonymousLoginSettingsBase
    {}

    

}
