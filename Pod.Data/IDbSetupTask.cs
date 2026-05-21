#region Licence
/****************************************************************
 *  Filename: IDbSetupTask.cs
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
using System.Text;
using System.Threading.Tasks;

namespace Pod.Data
{
    /// <summary>
    /// Interface for Setup Tasks to populate the Database
    /// Dependencies should be resolved by Ioc
    /// </summary>
    public interface IDbSetupTask
    {
        /// <summary>
        /// The Execution Priority, tasks with a higher a lower value will be executed first
        /// Is important is Setup Tasks need entities that have to be created by other SetupTasks
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// The code to execute to populate the Db
        /// </summary>
        void Execute();
    }
}
