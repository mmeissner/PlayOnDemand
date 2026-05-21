#region Licence
/****************************************************************
 *  Filename: IPlatformStateProvider.cs
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
using System.Threading;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Modules.Interfaces.Platform
{
    /// <summary>
    /// Provides State Notifications for a Platform Client 
    /// </summary>
    /// <seealso cref="IUIPlatformNotificationsAvailableEvent" />
    public interface IPlatformStateProvider : IUIPlatformNotificationsAvailableEvent
    {
        /// <summary>
        /// Publishes the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="details">The details.</param>
        void Publish(PlatformState state, string details = null);

        /// <summary>
        /// Publishes the specific state as an cancelable
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="details">The details.</param>
        /// <returns>The Cancellation Token to check if an cancellation is requested</returns>
        CancellationToken PublishCancelable(PlatformState state, string details = null);

        /// <summary>
        /// Signals the end of publishing. This could be the case when a client gets closed
        /// </summary>
        void SignalPublishEnd();
    }

}
