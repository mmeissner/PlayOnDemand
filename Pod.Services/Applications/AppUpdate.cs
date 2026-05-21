#region Licence
/****************************************************************
 *  Filename: AppUpdate.cs
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
using System;
using System.Collections.Generic;
using System.Text;
using Pod.Data.Infrastructure;
using Pod.Data.Models;
using Pod.Data.Models.Interfaces;
using Pod.Grpc.Messages.ShellApplications;

namespace Pod.Services.Applications
{

    class AppUpdate : IAppUpdate
    {
        public Guid ApplicationId { get; private set; }
        public uint InstanceVersion { get; private set;}
        public string DisplayName { get; private set;}
        public bool IsEnabled { get;private set; }

        public static IResult<AppUpdate> FromAppUpdateInfo(AppUpdateInfo updateInfo)
        {
            var result = new Result<AppUpdate>();
            if(!result.ArgTrue(
                    updateInfo.ApplicationId.ToGuidNullable().HasValue,
                    "updateInfo.ApplicationId.ToGuid().HasValue"))
            {
                return result;
            }

            return result.Add(
                    new AppUpdate()
                    {
                            ApplicationId = updateInfo.ApplicationId.ToGuid(),
                            DisplayName = updateInfo.DisplayName,
                            IsEnabled = updateInfo.IsEnabled,
                            InstanceVersion = updateInfo.InstanceVersion
                    });
        }
        public static IResult<AppUpdate> FromAppInstalledInfo(AppInstallInfo installedInfo)
        {
            var result = new Result<AppUpdate>();
            if(!result.ArgTrue(
                    installedInfo.ApplicationId.ToGuidNullable().HasValue,
                    "installedInfo.ApplicationId.ToGuid().HasValue"))
            {
                return result;
            }

            return result.Add(
                    new AppUpdate()
                    {
                            ApplicationId = installedInfo.ApplicationId.ToGuid(),
                            DisplayName = installedInfo.DisplayName,
                            IsEnabled = installedInfo.IsEnabled,
                            InstanceVersion = installedInfo.InstanceVersion
                    });
        }
    }
}
