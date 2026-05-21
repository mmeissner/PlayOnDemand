#region Licence
/****************************************************************
 *  Filename: SchemaPrefabCapabilityModel.cs
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
    public class SchemaPrefabCapabilityModel
    {
        public bool Nameable { get; set; }

        public bool CanHaveSockets { get; set; }

        public bool GemsCanBeExtracted { get; set; }

        public bool CanGiftWrap { get; set; }

        public bool UsableGC { get; set; }

        public bool UsableOutOfGame { get; set; }

        public bool Decodable { get; set; }

        public bool Usable { get; set; }

        public bool IsGem { get; set; }
    }
}