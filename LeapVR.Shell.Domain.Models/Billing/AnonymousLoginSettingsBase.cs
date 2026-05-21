#region Licence
/****************************************************************
 *  Filename: AnonymousLoginSettingsBase.cs
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

namespace LeapVR.Shell.Domain.Models.Billing
{
    /// <summary>
    /// Represents settings for Station in Anonymous Session mode.
    /// </summary>
    public abstract class AnonymousLoginSettingsBase : ISessionSettings
    {
        public bool Equals(ISessionSettings other)
        {
            return other is AnonymousLoginSettingsBase;
        }

        public override string ToString()
        {
            return $"AnonymousLoginSettingsBase";
        }
    }
}
