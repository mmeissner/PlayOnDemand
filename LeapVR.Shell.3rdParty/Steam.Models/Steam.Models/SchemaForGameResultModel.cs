#region Licence
/****************************************************************
 *  Filename: SchemaForGameResultModel.cs
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
namespace Steam.Models
{
    public class SchemaForGameResultModel
    {
        public string GameName { get; set; }

        public string GameVersion { get; set; }

        public AvailableGameStatsModel AvailableGameStats { get; set; }
    }
}