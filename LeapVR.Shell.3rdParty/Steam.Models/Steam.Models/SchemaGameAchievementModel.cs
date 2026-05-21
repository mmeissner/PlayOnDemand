#region Licence
/****************************************************************
 *  Filename: SchemaGameAchievementModel.cs
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
    public class SchemaGameAchievementModel
    {
        public string Name { get; set; }

        public long DefaultValue { get; set; }

        public string DisplayName { get; set; }

        public int Hidden { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }

        public string Icongray { get; set; }
    }
}