#region Licence
/****************************************************************
 *  Filename: DotaTeamType.cs
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
namespace Steam.Models.DOTA2
{
    public sealed class DotaTeamType : DotaEnumType
    {
        public static readonly DotaTeamType UNKNOWN = new DotaTeamType("Unknown", "Unknown", "This team is unknown.");
        public static readonly DotaTeamType GOOD = new DotaTeamType("Good", "Radiant", "The 'good' team.");
        public static readonly DotaTeamType BAD = new DotaTeamType("Bad", "Dire", "The 'bad' team.");

        public DotaTeamType(string key, string displayName, string description)
            : base(key, displayName, description)
        { }
    }
}