#region Licence
/****************************************************************
 *  Filename: IAppInfoProcessor.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Domain.Models.Platform
{
    /// <summary>
    /// A component that takes in <see cref="IProcessExecutionLogic"/> and output <see cref="IExecuteable"/>.
    /// </summary>
    public interface IAppInfoProcessor
    {
        IEnumerable<IExecuteable> GetExecutionInfoResult(IEnumerable<IProcessExecutionLogic> processExecutionInstructions, bool needsFullfilledRequirements);
    }
}
