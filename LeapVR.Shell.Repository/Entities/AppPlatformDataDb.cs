#region Licence
/****************************************************************
 *  Filename: AppPlatformDataDb.cs
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
using System.Collections.Generic;
using System.Reflection;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository.Entities
{
    
    class AppPlatformDataDb: IAppPlatformData, IEntity
    {
        public Guid Id { get; set; }
        public Guid ApplicationGuid { get; set; }
        public Guid PlatformPluginId { get; set; }
        public string ApplicationName { get; set; }
        public IEnumerable<IProcessExecutionLogic> ExecutionLogicInstructions { get; set; }
        public bool IsEnabled { get; set; }

        public AppPlatformDataDb(){}

        public AppPlatformDataDb(IAppPlatformData platformData)
        {
            ApplicationGuid = platformData.ApplicationGuid;
            ApplicationName = platformData.ApplicationName;
            PlatformPluginId = platformData.PlatformPluginId;

            IsEnabled = platformData.IsEnabled;
            if(platformData.ExecutionLogicInstructions != null)
            {
                ExecutionLogicInstructions = EntityConverter.Convert(platformData.ExecutionLogicInstructions);
            }
        }
    }
}