#region Licence
/****************************************************************
 *  Filename: ISessionService.cs
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
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Services.Data;
using Pod.Data.Infrastructure;

namespace LeapVR.Shell.Services.Session
{
    interface ISessionService
    {
        Task<IResult<LoginDecisionResponse>> SendLoginIntentionResponseAsync(bool isAccepted);
        IResult<LogoutResponse> SendLogoutRequest(SessionStopReason reason);
        void RemoveSession(RemoteSession session);
    }
}
