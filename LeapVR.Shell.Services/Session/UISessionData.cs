#region Licence
/****************************************************************
 *  Filename: UISessionData.cs
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
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Services.Session
{
    class UISessionData : IUISessionData
    {
        internal UISessionData(RemoteSession sessionData)
        {
            SessionId = sessionData.SessionId;
            Type = sessionData.Type;
            Started = sessionData.Started;
            Stopped = sessionData.Stopped;
            StopReason = sessionData.StopReason;
            SessionRate = sessionData.SessionRate.Clone();
        }
        private UISessionData(IUISessionData sessionData)
        {
            SessionId = sessionData.SessionId;
            Type = sessionData.Type;
            Started = sessionData.Started;
            Stopped = sessionData.Stopped;
            StopReason = sessionData.StopReason;
            SessionRate = sessionData.SessionRate.Clone();
        }
        public Guid SessionId { get; }
        public SessionType Type { get; }
        public DateTime Started { get; }
        public DateTime? Stopped { get; }
        public SessionStopReason? StopReason { get; }
        public ISessionRate SessionRate { get; }
        public IUISessionData Clone() { return new UISessionData(this); }
    }

    class UISession : UISessionData, IUISession
    {
        private readonly Action<SessionStopReason> _stopAction;
        private UISession(Action<SessionStopReason> stopAction, RemoteSession sessionData, out BehaviorSubject<IUISession> behaviorSubject) : base(sessionData)
        {
            _stopAction = stopAction;
            behaviorSubject = new BehaviorSubject<IUISession>(this);
            WhenSessionUpdated = behaviorSubject;
        }
        public UISession(Action<SessionStopReason> stopAction,IObservable<IUISession> whenSessionUpdated,RemoteSession sessionData) : base(sessionData)
        {
            WhenSessionUpdated = whenSessionUpdated;
            _stopAction = stopAction;
        }
        public IObservable<IUISession> WhenSessionUpdated { get; }
        public void RequestStopSession(SessionStopReason reason) => _stopAction(reason);

        public static BehaviorSubject<IUISession> BehaviorSubjectFromRemoteSession(
                Action<SessionStopReason> stopAction, RemoteSession sessionData)
        {
            new UISession(stopAction, sessionData, out var retval);
            return retval;
        }
    }
}