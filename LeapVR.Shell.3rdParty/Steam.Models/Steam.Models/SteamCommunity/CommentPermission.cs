#region Licence
/****************************************************************
 *  Filename: CommentPermission.cs
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
    /// Indicates the selected privacy/visibility level of the player's comments section on their Steam Community profile
    /// </summary>
    public enum CommentPermission
    {
        Unknown = 0,
        FriendsOnly = 1,
        Private = 2,
        Public = 3
    }
}