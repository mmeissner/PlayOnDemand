#region Licence
/****************************************************************
 *  Filename: OpenVrModuleConfig.cs
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

using System.Collections.Generic;
using System.Reflection;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Shell.Modules.Vr;

namespace LeapVR.Shell.Modules.FileConfig
{
    
    public class OpenVrModuleConfig : ConfigObject
    {
        public bool IsOpenVrConfigReplacingEnabled { get; set; } = true;
        public bool IsOpenVrConfigReplaced { get; set; } = false;
        public string OriginalOpenVrSettingsName { get; set; } = "_original";
        public string DefaultOpenVrSettingsName { get; set; } = "_default";
        public string VrMonitorProcessName { get; set; } = "vrmonitor";
        public string VrServerProcessName { get; set; } = "vrserver";
        public string VrCompositorProcessName { get; set; } = "vrcompositor";
        public string VrMonitorStartCommand { get; set; } = "vrmonitor://compositor";
        public string VrMonitorStopCommand { get; set; } = "vrmonitor://quit";
        public string VrMonitorConfigFilePath { get; set; } = @"%LOCALAPPDATA%\openvr\openvrpaths.vrpath";
        public string VrMonitorLogFileName { get; set; } = "vrmonitor.txt";

        public string[] VrMonitorProcessNamesToExit { get; set; } =
        {
            "vrmonitor",
            "vrcompositor",
            "vrserver",
            "vivelink"
        };
        public OpenVrSettingEntityDetails[] ConfigFilesDetails { get; set; } =
        {
            new OpenVrSettingEntityDetails(OpenVrConfigLocation.OpenVrConfigDirectory, @"steamvr.vrsettings", OpenVrConfigFileType.Json, OpenVrConfigApplyBehavior.Override),
            new OpenVrSettingEntityDetails(OpenVrConfigLocation.OpenVrDirectory, @"content\panorama\layout\dashboard.xml", OpenVrConfigFileType.Other),
            new OpenVrSettingEntityDetails(OpenVrConfigLocation.OpenVrDirectory, @"content\panorama\layout\powermenu.xml", OpenVrConfigFileType.Other),
            new OpenVrSettingEntityDetails(OpenVrConfigLocation.OpenVrDirectory, @"content\panorama\styles\vrsettings.css", OpenVrConfigFileType.Other),
            new OpenVrSettingEntityDetails(OpenVrConfigLocation.OpenVrDirectory, @"content\panorama\styles\dashboard.css", OpenVrConfigFileType.Other),
        };

        public Dictionary<OpenVrConfigFileType, OpenVrConfigApplyBehavior> DefaultApplyBehaviors { get; set; } = new Dictionary<OpenVrConfigFileType, OpenVrConfigApplyBehavior>
        {
            {OpenVrConfigFileType.Other, OpenVrConfigApplyBehavior.ReplaceFile },
            {OpenVrConfigFileType.Json, OpenVrConfigApplyBehavior.ReplaceFile },
        };
    }
}
