#region Licence
/****************************************************************
 *  Filename: RemoteNotifications.cs
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
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Controllers.RemoteService.Interfaces
{
    public interface IRemoteServiceSessionMessageHub
    {
        void OnLoginDecisionRequired(ILoginIntention loginIntention);
        void OnLoginDecisionResponse(LoginDecisionResultType response);
        void OnLoginIntentionExpired(ILoginIntention loginIntention);
        void OnSessionStarted(IUISession session);
    }
}
