#region Licence
/****************************************************************
 *  Filename: RarityResultContainer.cs
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
using System.Collections.Generic;

namespace SteamWebAPI2.Models.DOTA2
{
    internal class Rarity
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Order { get; set; }
        public string Color { get; set; }
        public string LocalizedName { get; set; }
    }

    internal class RarityResult
    {
        public int Count { get; set; }
        public int Status { get; set; }
        public IList<Rarity> Rarities { get; set; }
    }

    internal class RarityResultContainer
    {
        public RarityResult Result { get; set; }
    }
}