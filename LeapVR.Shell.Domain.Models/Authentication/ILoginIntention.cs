#region Licence
/****************************************************************
 *  Filename: ILoginIntention.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Threading.Tasks;
using LeapVR.Shared.Lib;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Domain.Models.Authentication
{
    /// <summary>
    /// Represent single user's intention to start new billing session on station.
    /// </summary>
    public interface ILoginIntention
    {
        IObservable<IUISession> WhenSessionStarted { get; }

        /// <summary>
        /// ID of intention, needed for communication with server.
        /// </summary>
        Guid IntentionId { get; }

        /// <summary>
        /// (UTC) Expiration of login intention. After this time session cannot be started using this <see cref="ILoginIntention"/>.
        /// </summary>
        DateTime IntentionConfirmationExpiresOnUtc { get; }

        /// <summary>
        /// Billing rate of session to be started.
        /// </summary>
        ISessionRate SessionRate { get; }

        //Sends the Login Decision for this LoginIntention
        Task SendLoginDecisionAsync(LoginDecisionType decision);
    }
}
