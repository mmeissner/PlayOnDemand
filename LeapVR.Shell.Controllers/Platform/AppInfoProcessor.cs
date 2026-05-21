#region Licence
/****************************************************************
 *  Filename: AppInfoProcessor.cs
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
using System.Linq;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Controllers.Platform
{
    public class AppInfoProcessor : IAppInfoProcessor
    {
        #region Fields & Properties
        private readonly IVirtualRealityController _virtualRealityController;
        #endregion

        #region Constructors

        public AppInfoProcessor(IVirtualRealityController virtualRealityController)
        {
            _virtualRealityController = virtualRealityController;
        }
        #endregion

        #region Public Methods       
        public IEnumerable<IExecuteable> GetExecutionInfoResult(IEnumerable<IProcessExecutionLogic> executionLogicInstructions, bool needsFullfilledRequirements = true)
        {
            foreach (var processExecutionLogic in executionLogicInstructions)
            {
                var executionInfoResult = GetExecutionInfoResultLogic(processExecutionLogic);
                if(needsFullfilledRequirements)
                {
                    if(!executionInfoResult.HasAllRequirementsFullfiled)continue;
                }
                yield return executionInfoResult;
            }
        }
        #endregion

        #region Private Methods
        private Executeable GetExecutionInfoResultLogic(IProcessExecutionLogic executionLogic)
        {
            var isVrRequired = !(string.IsNullOrEmpty(executionLogic.ReguiredVrModuleGuid) ||
                                 executionLogic.ReguiredVrModuleGuid == Guid.Empty.ToString());

            var isScreenModeSupported = !isVrRequired;

            bool isVrRequirementFulfilled = false;
            if (isVrRequired)
            {
                if(_virtualRealityController.AvailableVrModules.Any(
                    x => x.HasModuleSupport(
                        Guid.Parse(executionLogic.ReguiredVrModuleGuid))))
                {
                    isVrRequirementFulfilled = true;
                }
            }
            if (executionLogic.RequiredModuleGuids != null && executionLogic.RequiredModuleGuids.Any())
                throw new NotImplementedException("Required Module Guid Check is not yet implemented");

            var executionInfoResult = new Executeable(executionLogic, isVrRequired, isVrRequirementFulfilled, isScreenModeSupported);
            return executionInfoResult;
        }
        #endregion
    }
}
