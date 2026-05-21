#region Licence
/****************************************************************
 *  Filename: IDto.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapVR.Shell.Controllers.RemoteService.Interfaces
{
    #region ShellService
    public enum ServerMessage
    {
        Unknown,
        UpdateClientSettings,
        UpdateServerSettings,
        GetLoginRequest,
        UpdateSession,
        SendHeartbeat
    }
    #endregion

    #region ApplicationService

    public interface ISyncResponse
    {
        DateTime LastSyncTimestampUtc { get; }
        HashSet<IAppState> AppStates { get; } 
    }

    public interface ISyncAppsResponse
    {
        DateTime NewSyncTimestamp {get;}
    }
    public interface IAppState
    {
        Guid ApplicationId { get; }
        uint InstanceVersion { get; }
    }

    public interface IAppUpdateInfo
    {
        Guid ApplicationId { get; }
        string DisplayName {get;}
        uint InstanceVersion { get; }
        bool IsEnabled { get; }
    }

    public interface IAppInstallInfo
    {
        Guid ApplicationId { get; }
        Guid PlatformId { get; }
        string DisplayName {get;}
        uint InstanceVersion { get; }
        bool IsEnabled { get; }
    }

    public interface IAppUninstallInfo
    {
        Guid ApplicationId { get; }
        DateTime UninstalledOnUtc { get; }
    }

    public interface IAppUpdateResponse{}
    #endregion
}
