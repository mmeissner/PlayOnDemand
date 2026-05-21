#region Licence
/****************************************************************
 *  Filename: AppDisplayDataDb.cs
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
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository.Entities
{
    
    class AppDisplayDataDb : IAppDisplayData, IEntity
    {
        public Guid Id { get; set; }
        public Guid ApplicationGuid { get; set; }
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public IDiskEntity MainPicture { get; set; }

        public AppDisplayDataDb(){}
        public AppDisplayDataDb(IAppDisplayData displayData)
        {
            ApplicationGuid = displayData.ApplicationGuid;
            Category = displayData.Category;
            Description = displayData.Description;
            Name = displayData.Name;
            Tags = displayData.Tags;
            if(displayData.MainPicture != null) MainPicture = new DiskEntityDb(displayData.MainPicture);
        }
    }
}