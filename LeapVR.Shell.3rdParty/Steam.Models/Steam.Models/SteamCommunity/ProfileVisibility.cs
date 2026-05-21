#region Licence
/****************************************************************
 *  Filename: ProfileVisibility.cs
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
    /// Indicates the selected privacy/visibility level of the player's Steam Community profile
    /// </summary>
    public enum ProfileVisibility
    {
        Unknown = 0,
        Private = 1,
        Public = 3,
        FriendsOnly = 8,
    }
}