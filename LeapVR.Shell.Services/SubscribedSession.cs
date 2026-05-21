#region Licence
/****************************************************************
 *  Filename: SubscribedSession.cs
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
using LeapVR.Shell.Services.Session;

namespace LeapVR.Shell.Services
{
    partial class RemoteServicesSet
    {
        class SubscribedSession : IDisposable
        {
            readonly HashSet<IDisposable> _hashSetDisposables = new HashSet<IDisposable>();
            public SubscribedSession(RemoteSession session, RemoteServicesSet remoteServicesSet)
            {
                Session = session;
                //Publishes all the Events from the Session also on the RemoteServiceSet
                AddSubscription(session.WhenLoginDecisionRequired.Subscribe(remoteServicesSet._whenLoginDecisionRequired.OnNext));
                AddSubscription(session.WhenLoginDecisionResponseArrived.Subscribe(remoteServicesSet._whenLoginDecisionResponseArrived.OnNext));
                AddSubscription(session.WhenLoginIntentionExpired.Subscribe(remoteServicesSet._whenLoginIntentionExpired.OnNext));
                AddSubscription(session.WhenSessionStarted.Subscribe(remoteServicesSet._whenSessionStarted.OnNext));
            }
            public RemoteSession Session { get; }
            private void AddSubscription(IDisposable subscription) { _hashSetDisposables.Add(subscription); }

            public void Dispose()
            {
                foreach (IDisposable disposable in _hashSetDisposables)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}