#region Licence
/****************************************************************
 *  Filename: GeneralErrorNotification.cs
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
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Services.Data
{


    public class ServiceError : IServiceErrorInfo
    {
        public ServiceError(string errorMessage)
        {
            ErrorString = errorMessage;
            Type = ServiceErrorType.ServiceError;
        }
        public string ErrorString { get; }
        public ServiceErrorType Type { get; }
    }

    public class LicenseError : ILicenseError
    {
        public LicenseError(LicenseStatus state, string errorString)
        {
            State = state;
            ErrorString = errorString;
            Type = ServiceErrorType.LicenseError;
        }

        public LicenseStatus State { get; }
        public string ErrorString { get; }
        public ServiceErrorType Type { get;}

        public override string ToString() { return $"License Error: State={State}, ErrorString={ErrorString}"; }
    }

    public class VersionError: IServiceErrorInfo
    {
        public VersionError(ShellVersionStatus state, string errorString)
        {
            State = state;
            ErrorString = errorString;
            Type = ServiceErrorType.VersionError;
        }

        public ShellVersionStatus State { get; }
        public string ErrorString { get; }
        public ServiceErrorType Type { get;  }
        public override string ToString() { return $"Version Error: VersionStatus={State}, ErrorString={ErrorString}"; }
    }
}
