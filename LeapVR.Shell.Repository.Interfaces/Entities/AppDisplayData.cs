#region Licence
/****************************************************************
 *  Filename: AppDisplayData.cs
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
using LeapVR.Shell.Domain.Models.Disk;

namespace LeapVR.Shell.Repository.Interfaces.Entities
{
    
    public class AppDisplayData : IAppDisplayData
    {
        public Guid ApplicationGuid { get; set; }
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public IDiskEntity MainPicture { get; set; }
    }
}