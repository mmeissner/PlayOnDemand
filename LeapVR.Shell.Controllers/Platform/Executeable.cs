#region Licence
/****************************************************************
 *  Filename: Executeable.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Controllers.Platform
{
    internal class Executeable : IExecuteable
    {
        internal Executeable(IProcessExecutionLogic logic, bool isVirtualRealityRequired, bool isVrRequirementsFulfiled, bool isScreenModeSupported)
        {
            ExecutionLogic = logic;
            IsVirtualRealityRequired = isVirtualRealityRequired;
            IsVrRequirementFullfiled = isVrRequirementsFulfiled;
            IsScreenModeSupported = isScreenModeSupported;
        }
        public string DisplayName => ExecutionLogic.DisplayName;
        public bool IsVirtualRealityRequired { get; }
        public bool IsScreenModeSupported { get; }
        public bool IsVrRequirementFullfiled { get; }
        public bool HasAllRequirementsFullfiled => !IsVirtualRealityRequired || IsVirtualRealityRequired && IsVrRequirementFullfiled;
        public IProcessExecutionLogic ExecutionLogic { get; }
    }


}
