#region Licence
/****************************************************************
 *  Filename: MatchPickBanModel.cs
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
    public class MatchPickBanModel
    {
        public bool IsPick { get; set; }

        public int HeroId { get; set; }

        public int Team { get; set; }
        public int Order { get; set; }
    }
}