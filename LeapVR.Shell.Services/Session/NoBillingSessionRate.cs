#region Licence
/****************************************************************
 *  Filename: NoBillingSessionRate.cs
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
using LeapVR.Shell.Domain.Models.Billing;

namespace LeapVR.Shell.Services.Session
{
    class NoBillingSessionRate :BaseSessionRate, INoBillingSessionRate
    {
        public NoBillingSessionRate() { Type = SessionType.Unlimited; }
        public override string ToString() { return "NoBillingSessionRate has no Billing Data"; }
        public override ISessionRate Clone()
        {
            return new NoBillingSessionRate()
                   {
                           Type = SessionType.Unlimited,
                   };
        }
    }
}