#region Licence
/****************************************************************
 *  Filename: SteamMethodModel.cs
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

namespace Steam.Models
{
    public class SteamMethodModel
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public string HttpMethod { get; set; }
        public IReadOnlyCollection<SteamParameterModel> Parameters { get; private set; }
    }
}