#region Licence
/****************************************************************
 *  Filename: IServiceErrorInfo.cs
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
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Controllers.RemoteService.Interfaces
{
    public interface IServiceErrorInfo
    {
        string ErrorString { get; }
        ServiceErrorType Type { get; }
    }

    public enum ServiceErrorType
    {
        VersionError,
        LicenseError,
        ServiceError,
    }

    public interface ILicenseError : IServiceErrorInfo
    {
        LicenseStatus State { get; }
        string ToString();
    }

    public interface IVersionError : IServiceErrorInfo
    {
        ShellVersionStatus State { get; }
        string ToString();
    }
}