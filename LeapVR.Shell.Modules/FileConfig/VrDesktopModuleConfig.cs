#region Licence
/****************************************************************
 *  Filename: VrDesktopModuleConfig.cs
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

using System.Reflection;
using LeapVR.Shell.Domain.Models.Customization;

namespace LeapVR.Shell.Modules.FileConfig
{
    
    public class VrDesktopModuleConfig : ConfigObject
    {
        public string VrDesktopExecutableParameters { get; set; } = @"-popupwindow";
        public string VrDesktopExecutableRelativeFilePath { get; set; } = @"vr_desktop\vrlounge_desktop.exe";
        public string VrDesktopProcessName { get; set; } = @"vrlounge_desktop";
    }
}
