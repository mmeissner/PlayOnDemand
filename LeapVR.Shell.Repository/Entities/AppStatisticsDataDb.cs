#region Licence
/****************************************************************
 *  Filename: AppStatisticsDataDb.cs
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
using System;
using System.Reflection;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository.Entities
{
    
    class AppStatisticsDataDb : IAppStatisticsData, IEntity
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public Guid ApplicationGuid { get; set; }
        public Guid PlatformGuid { get; set; }
        public DateTime LastTimePlayed { get; set; }
        public int TimesPlayed { get; set; }
        public long LongestSession { get; set; }
        public long TotalRuntime { get; set; }
    }
}
