#region Licence
/****************************************************************
 *  Filename: IRemoteServiceController.cs
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
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Station;
using Pod.Data.Infrastructure;

namespace LeapVR.Shell.Domain.Models.Controllers
{
    public interface IRemoteServiceController : IController
    {
        /// <summary>
        /// Gets the latest shell client information
        /// </summary>
        /// <value>
        /// The latest shell client information.
        /// </value>
        IShellClientInfo LatestShellClientInfo { get; }

        string StationId { get; }
        bool HasStationIdSet { get; }
        bool HasPasswordSet { get; }
        bool AutoLogin { get; }
        ConnectState ConnectState { get; }
        Task<IResult<bool>> ConnectAsync();
        Task<IResult<bool>> DisconnectAsync();
        bool SetStationId(string stationId);
        bool SetPassword(string password);
        void SetAutoLogin(bool autoLoginEnabled);
        Task<IntendAnonymousSessionResult> SendAnonymousSessionLoginIntentionAsync();
    }



    public enum ConnectState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
    }
}
