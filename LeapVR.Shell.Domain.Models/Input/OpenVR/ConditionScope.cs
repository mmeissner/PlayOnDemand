#region Licence
/****************************************************************
 *  Filename: ConditionScope.cs
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

namespace LeapVR.Shell.Domain.Models.Input.OpenVR
{
    /// <summary>
    /// Flags representing in which conditions some action can be performed;
    /// Value of for example <see cref="NoSession"/> | <see cref="SessionNoGame"/> means that action can be performed either if there is no session, or there is session with no game running.
    /// </summary>
    [Flags]
    public enum ConditionScope
    {
        Unknown = 0,

        NoSession = 1 << 0,
        SessionNoGame = 1 << 1,
        SessionGame = 1 << 2,

        All = NoSession | SessionNoGame | SessionGame,
    }
}
