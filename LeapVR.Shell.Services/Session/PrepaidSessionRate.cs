#region Licence
/****************************************************************
 *  Filename: PrepaidSessionRate.cs
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
    class PrepaidSessionRate :BaseSessionRate, IPrepaidSessionRate
    {
        public PrepaidSessionRate(TimeSpan effectiveDuration, DateTime? calculatedStopTime)
        {
            Type = SessionType.Limited;
            EffectiveDuration = effectiveDuration;
            CalculatedStopTimeUtc = calculatedStopTime;
        }
        public TimeSpan EffectiveDuration { get; }
        public override string ToString()
        {
            return $"EffectiveDuration={EffectiveDuration}";
        }

        public override ISessionRate Clone()
        {
            //We just use the existing SessionDetails object as it should never mutate
            return new PrepaidSessionRate(EffectiveDuration,CalculatedStopTimeUtc);
        }
    }
}