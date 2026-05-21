#region Licence
/****************************************************************
 *  Filename: UserStatus.cs
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
namespace Steam.Models.SteamCommunity
{
    /// <summary>
    /// Indicates the current status of the user on the Steam network
    /// </summary>
    public enum UserStatus
    {
        Offline = 0,
        Online = 1,
        Busy = 2,
        Away = 3,
        Snooze = 4,
        Unknown = 5,
        InGame = 6
    }
}