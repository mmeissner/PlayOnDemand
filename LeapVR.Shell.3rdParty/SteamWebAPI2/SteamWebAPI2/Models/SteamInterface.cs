#region Licence
/****************************************************************
 *  Filename: SteamInterface.cs
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
    internal class SteamInterface
    {
        public string Name { get; set; }
        public IList<SteamMethod> Methods { get; private set; }

        public SteamInterface()
        {
            Methods = new List<SteamMethod>();
        }
    }

    internal class SteamMethod
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public string HttpMethod { get; set; }
        public IList<SteamParameter> Parameters { get; private set; }

        public SteamMethod()
        {
            Parameters = new List<SteamParameter>();
        }
    }

    internal class SteamParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }

        [JsonProperty(PropertyName = "optional")]
        public bool IsOptional { get; set; }

        public string Description { get; set; }
    }
}