#region Licence
/****************************************************************
 *  Filename: IRemoteServiceSet.cs
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
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shell.Controllers.Station;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using Pod.Data.Infrastructure;

namespace LeapVR.Shell.Controllers.RemoteService.Interfaces
{
    public interface IRemoteServiceSet : IDisposable
    {
        IObservable<IShellClientInfo> WhenShellClientDisplayInfoChanged { get; }
        IObservable<ISessionSettings> WhenSessionSettingsChanged { get; }
        IObservable<ConnectionState> WhenConnectionStateChanged { get; }
        IObservable<IServiceErrorInfo> WhenServiceErrorOccured { get; }
        IObservable<ILoginIntention> WhenLoginDecisionRequired { get; }
        IObservable<LoginDecisionResultType> WhenLoginDecisionResponseArrived { get; }
        IObservable<ILoginIntention> WhenLoginIntentionExpired { get; }
        IObservable<IUISession> WhenSessionStarted { get; }

        Task<IResult<bool>> ConnectAsync(
                string stationId, string password,
                CancellationTokenSource serviceSetMainCts);
        Task<IResult<bool>> DisconnectAsync();
        Task<IResult<IntendAnonymousSessionResult>> SendLoginRequestAsync();
    }

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
    }
}