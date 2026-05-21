#region Licence
/****************************************************************
 *  Filename: ISessionController.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using LeapVR.Shell.Domain.Models.Controllers;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <summary>
    /// Controller managing billing session starting, keeping and stoping.
    /// </summary>
    
    [Obsolete]
    public interface ISessionController : IController
    {
        ///// <summary>
        ///// Currently active <see cref="ISessionSettings"/> of current station.
        ///// </summary>
        //ISessionSettings CurrentSessionSettings { get; }

        ///// <summary>
        ///// Notifies the server that there is Anonymous Session intent on this Station.
        ///// </summary>
        ///// <returns>Server reply indicating success or failure of intent.</returns>
        //IntendAnonymousSessionResult IntendAnonymousSession();

        ///// <summary>
        ///// Holds currently running <see cref="ISession"/>.
        ///// Null if no session is running.
        ///// </summary>
        //ISession CurrentSession { get; }

        ///// <summary>
        ///// Fired when new session is started.
        ///// Hot observable, notifies subscriber only real-time, no memory (like Subject).
        ///// </summary>
        //IObservable<ISession> WhenSessionStarted { get; }

        ///// <summary>
        ///// Holds <see cref="ILoginIntention"/> that is active at the moment.
        ///// </summary>
        //ILoginIntention CurrentLoginIntention { get; }

        ///// <summary>
        ///// Fired when new <see cref="ILoginIntention"/> arrives.
        ///// Hot observable, notifies subscriber only real-time, no memory (like Subject).
        ///// </summary>
        //IObservable<ILoginIntention> WhenLoginIntended { get; }

        ///// <summary>
        ///// Performs <see cref="ILoginDecision"/> in response to <see cref="ILoginIntention"/>.
        ///// </summary>
        ///// <param name="decision">Decision made</param>
        ///// <returns><see cref="ILoginDecisionResult"/></returns>
        //void SendLoginDecision(ILoginDecision decision);
    }
}
