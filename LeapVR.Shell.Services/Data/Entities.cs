#region Licence
/****************************************************************
 *  Filename: Entities.cs
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
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using Pod.Grpc.Messages.ConnectHost;
using Pod.Grpc.Messages.Shared;
using Pod.Grpc.Messages.ShellApplications;
using Pod.Grpc.Messages.ShellHost;

namespace LeapVR.Shell.Services.Data
{
    class ShellServer
    {
        ShellServer() { }
        public string HostAddress { get; private set; }
        public uint Port { get; private set; }
        public uint RequiredInterfaceVersion { get; private set; }
        public Guid ConnectionId { get; private set; }

        public static ShellServer FromResponse(ShellServerResponse response)
        {
            return new ShellServer()
                   {
                           HostAddress = response.HostAddress,
                           ConnectionId = response.ConnectionId.ToGuid(),
                           Port = response.Port,
                           RequiredInterfaceVersion = response.RequiredInterfaceVersion
                   };
        }
    }

    class ServerSettings
    {
        ServerSettings() { LocalTimeUtcNow = DateTime.UtcNow; }
        public static ServerSettings FromResponse(ServerSettingsResponse response)
        {
            return new ServerSettings()
                   {
                           HeartbeatInterval = response.HeartbeatInterval.ToTimeSpanNullable().Value,
                           HeartbeatTimeout = response.HeartbeatTimeout.ToTimeSpanNullable().Value,
                           ServerTimeUtcNow = response.ServerTimeUtcNow.ToDateTimeUtcNullable().Value
                   };
        }
        public TimeSpan HeartbeatInterval { get; private set; }
        public TimeSpan HeartbeatTimeout { get; private set; }
        public DateTime LocalTimeUtcNow { get; }
        public DateTime ServerTimeUtcNow { get; private set; }
    }

    class ConnectResponse
    {
        ConnectResponse(){}
        public static ConnectResponse FromResponse(Pod.Grpc.Messages.ShellHost.ConnectResponse response)
        {
            return new ConnectResponse();
        }
    }

    class ClientSettings
    {
        ClientSettings(){}
        public static ClientSettings FromResponse(ClientSettingsResponse response)
        {
            return new ClientSettings()
                   {
                           DisplayName = response.DisplayName,
                           QrCode = response.QrCode,
                           Mode =response.Mode
                   };
        }
        public string DisplayName { get;private set; }
        public string QrCode { get;private set; }
        public ControlMode Mode { get;private set; }
    }

    class LoginRequest
    {
        LoginRequest() { }

        public static LoginRequest FromResponse(TimeSpan timeSkew, LoginRequestResponse response)
        {
            var retval = new LoginRequest
                         {
                                 SessionDetails = SessionDetails.FromDetails(timeSkew,response.SessionDetails)
                         };
            return retval;
        }
        public SessionDetails SessionDetails { get; set; }
        public DateTime ServerDeadlineUtc { get; private set; }
    }

    class LoginIntentionReply 
    {
        LoginIntentionReply(){}
        public static LoginIntentionReply FromResponse(TimeSpan timeSkew,LoginIntentionReplyResponse response)
        {
          
            if(response.Session == null)return new LoginIntentionReply();
            return new LoginIntentionReply()
                   {
                           State = SessionDetails.FromDetails(timeSkew,response.Session)
                   };
        }
        public SessionDetails State { get;private set; }
    }

    class RequestedLogin
    {
        RequestedLogin(){}

        public static RequestedLogin FromResponse(TimeSpan timeSkew,RequestedLoginResponse response)
        {
            return new RequestedLogin()
                         {
                                 SessionDetails = SessionDetails.FromDetails(timeSkew,response.SessionDetails),
                         };
        }
        public SessionDetails SessionDetails { get; set; }
    }

    class SessionState
    {
        SessionState(){}
        public static SessionState FromResponse(TimeSpan timeSkew,SessionStateResponse response)
        {
            if(response.Session == null)return new SessionState();
            return new SessionState
                   {
                                 State = SessionDetails.FromDetails(timeSkew,response.Session)
                         };
        }
        public SessionDetails State { get;private set; }
    }

    class SessionDetails
    {
        private TimeSpan? _effectiveDuration;
        SessionDetails(){}
        public Guid SessionId { get;private set; }
        public Pod.Grpc.Messages.Shared.SessionState Stage { get;private set; }
        public DateTime RequestedOnUtc { get; private set; }
        public DateTime? DeadlineUtcForPickup { get; private set; }
        public DateTime? DeadlineUtcForConfirmation { get;private  set; }
        public TimeSpan? MaxTimeForConfirmationDecision { get; private set; }
        public DateTime? StartTimeUtc { get; private set; }
        public TimeSpan? EffectiveDuration => GetEffectiveDuration();
        public SessionConditions Conditions { get; private set; }
        public static SessionDetails FromDetails(TimeSpan timeSkew,Pod.Grpc.Messages.ShellHost.SessionDetails details)
        {
            return new SessionDetails()
                         {
                                 SessionId = details.SessionId.ToGuid(),
                                 RequestedOnUtc = details.RequestedOnUtc.ToDateTimeUtc(timeSkew),
                                 Stage = details.SessionState,
                                 DeadlineUtcForPickup = details.DeadlineUtcForPickUp.ToDateTimeUtcNullable(timeSkew),
                                 DeadlineUtcForConfirmation = details.DeadlineUtcForConfirmation.ToDateTimeUtcNullable(timeSkew),
                                 MaxTimeForConfirmationDecision = details.MaxTimeForConfirmationDecision.ToTimeSpanNullable(),
                                 StartTimeUtc = details.StartTimeUtc.ToDateTimeUtcNullable(timeSkew),
                                 _effectiveDuration = details.EffectiveDuration.ToTimeSpanNullable(),
                                 Conditions = SessionConditions.FromSessionConditions(details.Conditions)
                         };
        }
        private TimeSpan? GetEffectiveDuration()
        {
            if(Stage == Pod.Grpc.Messages.Shared.SessionState.Running)
            {
                return _effectiveDuration;
            }
            //Not running Sessions have just a Startup Condition
            return Conditions?.InitialDurationOnSessionStart;
        }
    }

    class SessionConditions
    {
        SessionConditions(Pod.Grpc.Messages.ShellHost.SessionConditions conditions)
        {
            InitialDurationOnSessionStart = conditions.InitialDurationOnSessionStart.ToTimeSpanNullable();
            AutostartAppIdOnSessionStart = conditions.AutostartAppIdOnSessionStart.ToGuidNullable();
            AutoLogoutOnAppExit = conditions.AutoLogoutOnAppExit;
            AllowedAppIds = new HashSet<Guid>();
            if (conditions.AllowedApps != null && conditions.AllowedApps.Any())
            {
                foreach(GuidAsBytes app in conditions.AllowedApps)
                {
                    AllowedAppIds.Add(app.ToGuid());
                }
            }
        }
        public TimeSpan? InitialDurationOnSessionStart { get; }
        public Guid? AutostartAppIdOnSessionStart { get; }
        public bool AutoLogoutOnAppExit { get; }
        public HashSet<Guid> AllowedAppIds { get; }

        public static SessionConditions FromSessionConditions(Pod.Grpc.Messages.ShellHost.SessionConditions conditions)
        {
            if(conditions == null) return null;
            return new SessionConditions(conditions);
        }
    }

    class LogoutResponse
    {
        LogoutResponse(){}

        public static LogoutResponse FromResponse(Pod.Grpc.Messages.ShellHost.LogoutResponse response)
        {
            return new LogoutResponse();
        }
    }

    class HeartbeatResponse
    {
        HeartbeatResponse(){}

        public static HeartbeatResponse FromResponse(Pod.Grpc.Messages.ShellHost.HeartbeatResponse response)
        {
            return new HeartbeatResponse();
        }
    }

    class DisconnectResponse
    {
        DisconnectResponse(){}

        public static DisconnectResponse FromResponse(Pod.Grpc.Messages.ShellHost.DisconnectResponse response)
        {
            return new DisconnectResponse();
        }
    }

    class LoginDecisionResponse
    {
        LoginDecisionResponse() { }
        LoginDecisionResponse(SessionDetails session)
        {
            Session = session;
        }
        public SessionDetails Session { get; }

        public static LoginDecisionResponse FromResponse(TimeSpan timeSkew, LoginIntentionReplyResponse response)
        {
            if(response.Session != null &&
               response.Session.SessionState != Pod.Grpc.Messages.Shared.SessionState.NoSession)
            {
                return new LoginDecisionResponse(SessionDetails.FromDetails(timeSkew,response.Session));
            }
            return new LoginDecisionResponse();
        }
    }

    class SyncResponse : ISyncResponse
    {
        SyncResponse(){}

        public static ISyncResponse FromResponse(TimeSpan timeSkew,Pod.Grpc.Messages.ShellApplications.SyncResponse response)
        {
            var retval = new SyncResponse()
                         {
                                 AppStates = new HashSet<IAppState>()
                         };
            var lastSyncTimestampUtc = response.LastSyncTimestamp.ToDateTimeUtcNullable();
            if(lastSyncTimestampUtc.HasValue)
            {
                retval.LastSyncTimestampUtc = lastSyncTimestampUtc.Value.Add(timeSkew);
            }
            foreach(AppDataState appState in response.AppStates)
            {
                retval.AppStates.Add(AppState.FromAppState(appState));
            }

            return retval;
        }
        public DateTime LastSyncTimestampUtc { get;private set; }
        public HashSet<IAppState> AppStates { get;private set; }
    }

    class SyncAppsResponse : ISyncAppsResponse
    {
        SyncAppsResponse(){}
        public static ISyncAppsResponse FromResponse(TimeSpan timeSkew, Pod.Grpc.Messages.ShellApplications.SyncAppsResponse response)
        {
            return new SyncAppsResponse()
                   {
                           NewSyncTimestamp = response.NewSyncTimestamp.ToDateTimeUtcNullable().Value.Add(timeSkew),
                   };
        }
        public DateTime NewSyncTimestamp { get;private set; }
    }

    class AppState : IAppState
    {
        AppState(){}
        public static IAppState FromAppState(AppDataState appState)
        {
            return new AppState()
                   {
                           ApplicationId = appState.ApplicationId.ToGuid(),
                           InstanceVersion = appState.InstanceVersion
                   };
        }
        public Guid ApplicationId { get;private set; }
        public uint InstanceVersion { get;private set; }
    }

    class AppUpdateResponse : IAppUpdateResponse
    {
        AppUpdateResponse(){}
        public static IAppUpdateResponse FromResponse(UpdateResponse response)
        {
            return new AppUpdateResponse();
        }
    }

    static class AppInfoConverter
    {
        public static Pod.Grpc.Messages.ShellApplications.AppUpdateInfo ToAppUpdateInfo(IAppUpdateInfo updateInfo)
        {
            return new Pod.Grpc.Messages.ShellApplications.AppUpdateInfo()
                   {
                           ApplicationId = updateInfo.ApplicationId.ToGuidAsBytes(),
                           DisplayName = updateInfo.DisplayName,
                           InstanceVersion = updateInfo.InstanceVersion,
                           IsEnabled = updateInfo.IsEnabled
                   };
        }
        public static Pod.Grpc.Messages.ShellApplications.AppInstallInfo ToAppInstallInfo(IAppInstallInfo installInfo)
        {
            return new AppInstallInfo()
                   {
                           ApplicationId = installInfo.ApplicationId.ToGuidAsBytes(),
                           PlatformId = installInfo.PlatformId.ToGuidAsBytes(),
                           DisplayName = installInfo.DisplayName,
                           InstanceVersion = installInfo.InstanceVersion,
                           IsEnabled = installInfo.IsEnabled
                   };
        }

        public static AppUninstallInfo ToAppUninstallInfo(TimeSpan timeSkew,IAppUninstallInfo uninstallInfo)
        {
            return new AppUninstallInfo()
                   {
                           ApplicationId = uninstallInfo.ApplicationId.ToGuidAsBytes(),
                           UninstalledOnUtc = uninstallInfo.UninstalledOnUtc.Add(timeSkew).ToDateTimeUtcAsLong()
                   };
        }
    }


    static class ConvertEnum
    {
        public static ServerMessage FromClientNotificationEvent(ClientNotificationEvent notificationEvent)
        {
            switch(notificationEvent)
            {
                case ClientNotificationEvent.CheckLoginRequest:
                    return ServerMessage.GetLoginRequest;
                case ClientNotificationEvent.UpdateClientSettings:
                    return ServerMessage.UpdateClientSettings;
                case ClientNotificationEvent.UpdateSessionState:
                    return ServerMessage.UpdateSession;
                case ClientNotificationEvent.UpdateServerSettings:
                    return ServerMessage.UpdateServerSettings;
                case ClientNotificationEvent.SendHeartbeat:
                    return ServerMessage.SendHeartbeat;
                default:
                    return ServerMessage.Unknown;
            }
        }

        public static LogoutReason FromSessionStopReason(SessionStopReason reason)
        {
            switch(reason)
            {
                case SessionStopReason.StationLogout:
                    return LogoutReason.UserLogout;
                case SessionStopReason.StationInactivity:
                    return LogoutReason.Inactivity;
                case SessionStopReason.AbandonedSession:
                    return LogoutReason.Inactivity;
                case SessionStopReason.UserBlocked:
                    return LogoutReason.LimitReached;
                case SessionStopReason.StationShutdown:
                    return LogoutReason.Shutdown;
                case SessionStopReason.SessionLimitReached:
                    return LogoutReason.LimitReached;
                default:
                    return LogoutReason.Unset;
            }
        }
    }
}