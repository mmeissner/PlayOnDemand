#region Licence
/****************************************************************
 *  Filename: SystemConfig.cs
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

using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Customization
{
    
    public class SystemConfig : ConfigObject
    {
        public string LeapVRTaskName { get; set; } = "Start LeapPlay";
        public string DefaultLanguage { get; set; } = "en-US";
        public string[] SupportedLanguageCultureNames { get; set; } = { "zh-CN", "en-US" };
    }
}