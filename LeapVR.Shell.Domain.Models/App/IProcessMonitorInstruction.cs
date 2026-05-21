#region Licence
/****************************************************************
 *  Filename: IProcessMonitorInstruction.cs
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Domain.Models.App
{    
    
    public interface IProcessMonitorInstruction
    {
        /// <summary>
        /// The Executables FullFilePathName relative to the base Application Directory
        /// </summary>
        string ExecutableRelativePathFileName { get; }

        /// <summary>
        /// Options that are applied to the process.
        /// </summary>
        ProcessMonitorOption Instruction { get; }
    }
}
