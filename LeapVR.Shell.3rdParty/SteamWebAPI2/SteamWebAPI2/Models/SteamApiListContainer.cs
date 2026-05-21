#region Licence
/****************************************************************
 *  Filename: SteamApiListContainer.cs
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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamWebAPI2.Models
{
    internal class SteamApiListResult
    {
        public IList<SteamInterface> Interfaces { get; set; }
    }

    internal class SteamApiListContainer
    {
        [JsonProperty("apilist")]
        public SteamApiListResult Result { get; set; }
    }
}