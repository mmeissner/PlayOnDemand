#region Licence
/****************************************************************
 *  Filename: IUIPlatformNotificationsAvailableEvent.cs
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

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages {

    /// <summary>
    /// Event broadcasted to inform about available notifications for a Platform
    /// Mainly provided in case a Platform Application is starting
    /// </summary>
    public interface IUIPlatformNotificationsAvailableEvent
    {
        /// <summary>
        /// Subscribes the specified change notification.
        /// </summary>
        /// <param name="changeNotification">The change notification callback.</param>
        /// <param name="notificationsEnded">The notifications ended callback.</param>
        /// <param name="context">The context.</param>
        void Subscribe(Action<IPlatformStateDetails> changeNotification, Action notificationsEnded,SynchronizationContext context);
    }

    /// <summary>
    /// Platform Notification Information
    /// </summary>
    public interface IPlatformStateDetails {
        Guid ApplicationId {get; }
        Guid PlatformId { get; }
        PlatformState State { get; }
        string Details { get; }
        bool IsCancelable { get; }
        bool IsCanceled { get; }
        void Cancel();
    }

    /// <summary>
    /// States a Platform Client / Application can have
    /// </summary>
    public enum PlatformState
    {
        Unavailable,
        StartingClient,
        StartingApplication,
        UpdateingApplication,
        UpdateingClient,
        StoppingClient,
        LoggingIn,
        LoggingOut,
        ApplicationRunning,
        ApplicationUpdateRequired,
        StartingClientError,
        StartingApplicationError,
    }
}