#region Licence
/****************************************************************
 *  Filename: DtoConverter.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using LeapVR.Shell.Controllers.Disk;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Repository.Interfaces.Entities;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    /// <summary>
    /// Converter for VBOX Package DataModel to Application DataModel
    /// </summary>
    public static class DtoConverter
    {
        /// <summary>
        /// Converts the specified data dto to an <see cref="IAppDisplayData"/>.
        /// </summary>
        /// <param name="dataDto">The data dto.</param>
        /// <param name="platformId">The platform identifier.</param>
        /// <returns></returns>
        public static IAppDisplayData Convert(IAppDisplayDataDto dataDto,Guid platformId)
        {
            var retval = new AppDisplayData();
            retval.ApplicationGuid = dataDto.ApplicationGuid;
            retval.Description = dataDto.Description;
            retval.Name = dataDto.Name;
            retval.Category = dataDto.Category;
            retval.Tags = dataDto.Tags;
            if(dataDto.MainPicture != null)
            {
                retval.MainPicture = Convert(dataDto.MainPicture,platformId);
            }
            return retval;
        }

        /// <summary>
        /// Converts the specified data dto to an <see cref="IAppPlatformDataDto"/>
        /// </summary>
        /// <param name="dataDto">The data dto.</param>
        /// <param name="applicationName">The original Application Name that does not change</param>
        /// <returns></returns>
        public static IAppPlatformData Convert(IAppPlatformDataDto dataDto, string applicationName)
        {
            var retval = new AppPlatformData
                         {
                                 ApplicationGuid = dataDto.ApplicationGuid,
                                 ApplicationName = applicationName,
                                 PlatformPluginId = dataDto.PlatformPluginId
                         };
            if(dataDto.ExecutionLogicInstructions != null)
            {
                var executionLogicInstructions = new List<IProcessExecutionLogic>();
                foreach(var instructionDto in dataDto.ExecutionLogicInstructions)
                {
                    executionLogicInstructions.Add(Convert(instructionDto));
                }
                retval.ExecutionLogicInstructions = executionLogicInstructions.ToArray();
            }
            else
            {
                retval.ExecutionLogicInstructions = new List<IProcessExecutionLogic>();
            }
            return retval;
        }

        /// <summary>
        /// Converts the specified data dto to an <see cref="IProcessExecutionLogic"/>
        /// </summary>
        /// <param name="dataDto">The data dto.</param>
        /// <returns></returns>
        private static IProcessExecutionLogic Convert(IProcessExecutionLogicDto dataDto)
        {
            return new ProcessExecutionLogic(dataDto, DiskEntityType.Relative);
        }

        /// <summary>
        /// Converts the specified VBox Container Data dto to an <see cref="IDiskEntity"/>
        /// </summary>
        /// <param name="dataDto">The data dto.</param>
        /// <param name="platformId">The platform identifier.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static IDiskEntity Convert(IDiskEntityDto dataDto,Guid platformId, DiskEntityType type =DiskEntityType.Relative)
        {
            return new DiskEntity(dataDto,platformId,type);
        }
    }
}
