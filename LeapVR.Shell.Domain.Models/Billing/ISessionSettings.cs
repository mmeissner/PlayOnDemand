#region Licence
/****************************************************************
 *  Filename: ISessionSettings.cs
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

namespace LeapVR.Shell.Domain.Models.Billing
{
    /// <summary>
    /// Carries information about this Station settings related to Session management.
    /// </summary>
    public interface ISessionSettings : IEquatable<ISessionSettings>
    {
        string ToString();
    }
}
