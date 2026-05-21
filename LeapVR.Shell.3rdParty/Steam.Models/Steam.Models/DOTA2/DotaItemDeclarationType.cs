#region Licence
/****************************************************************
 *  Filename: DotaItemDeclarationType.cs
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
    public sealed class DotaItemDeclarationType : DotaEnumType
    {
        public static readonly DotaItemDeclarationType UNKNOWN = new DotaItemDeclarationType("DECLARE_PURCHASES_UNKNOWN", "Unknown", "The declaration of this item is unknown.");
        public static readonly DotaItemDeclarationType PURCHASES_TO_TEAMMATES = new DotaItemDeclarationType("DECLARE_PURCHASES_TO_TEAMMATES", "To Teammates", "Declare this item to teammates when purchased.");
        public static readonly DotaItemDeclarationType PURCHASES_IN_SPEECH = new DotaItemDeclarationType("DECLARE_PURCHASES_IN_SPEECH", "In Speech", "Declare this item in speech when purchased.");
        public static readonly DotaItemDeclarationType PURCHASES_TO_SPECTATORS = new DotaItemDeclarationType("DECLARE_PURCHASES_TO_SPECTATORS", "To Spectators", "Declare this item to spectators when purchased.");

        public DotaItemDeclarationType(string key, string displayName, string description)
            : base(key, displayName, description)
        { }
    }
}