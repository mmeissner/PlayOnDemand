#region Licence
/****************************************************************
 *  Filename: ProcessExtentions.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-1-12
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Diagnostics;

namespace LeapVR.Utilities.Windows.Processes
{
    public static class ProcessExtentions
    {
        /// <summary>
        /// Gets the parent process of a process.
        /// </summary>
        /// <param name="process">on which process instance this method gets called</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(this Process process)
        {
            return ProcessUtilities.GetParentProcess(process.Id);
        }

    }

}
