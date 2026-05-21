#region Licence
/****************************************************************
 *  Filename: BaseSessionRate.cs
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
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Services.Data;
using SessionState = Pod.Grpc.Messages.Shared.SessionState;

namespace LeapVR.Shell.Services.Session
{
    abstract class BaseSessionRate : ISessionRate
    {
        /// <summary>
        /// Provides the Type and separates between limited and unlimited session 
        /// </summary>
        public SessionType Type { get; protected set; }

        /// <summary>
        /// Provides the StopTime if the Session is running and time limited, otherwise it has NoValue
        /// </summary>
        public DateTime? CalculatedStopTimeUtc { get; protected set; }
        public abstract ISessionRate Clone();
        internal static BaseSessionRate Create(SessionDetails details)
        {
            TimeSpan? durationToEvaluate;
            //Is not running
            if (details.Stage == SessionState.LoginRequested || details.Stage == SessionState.AwaitingConfirmation)
            {
                //If its running we must take priority of the effective duration
                //Otherwise the InitialDurationOnStart counts
                durationToEvaluate = details.Conditions?.InitialDurationOnSessionStart;
                if(durationToEvaluate.HasValue)
                {
                    return new PrepaidSessionRate(durationToEvaluate.Value,null);
                }
                return new NoBillingSessionRate();
            }

            //Must be running
            durationToEvaluate = details.EffectiveDuration;
            if(durationToEvaluate.HasValue)
            {
                //Must have an calculable StopTime
                DateTime calculatedStopTime = details.StartTimeUtc.Value.Add(durationToEvaluate.Value).AddMilliseconds(-500);
                return new PrepaidSessionRate(durationToEvaluate.Value, calculatedStopTime);
            }
            return new NoBillingSessionRate();
        }
    }
}