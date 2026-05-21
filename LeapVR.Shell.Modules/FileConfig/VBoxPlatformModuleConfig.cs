#region Licence
/****************************************************************
 *  Filename: VBoxPlatformModuleConfig.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-11-27
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Reflection;
using LeapVR.VBox.DataModel.Config;
using LeapVR.VBox.DataModel.Interfaces.Others;

namespace LeapVR.VBox.Modules.FileConfig
{
    [Obfuscation(Exclude = true)]
    public class VBoxPlatformModuleConfig : ConfigObject
    {
        /// <summary>
        /// Indicates whether to terminate application when it is not responding.
        /// </summary>
        public bool IsTerminateNotRespondingGamesEnabled { get; set; } = true;
        /// <summary>
        /// Indicates  how many times should be tried to detect executing before an application gets into its position.
        /// </summary>
        public int MaximumLoopCountBeforeExecutionDetected { get; set; } = 30;
        /// <summary>
        /// Indicates  how long the interval should be for one trial of loop.
        /// </summary>
        public int LoopIntervalBeforeExecutionDetected { get; set; } = 1000;
        /// <summary>
        /// Indicates how long the system will wait for an application resumes from not responding.
        /// </summary>
        public int TimeoutBeforeResumingFromNotResponding { get; set; } = 6000;
    }
}
