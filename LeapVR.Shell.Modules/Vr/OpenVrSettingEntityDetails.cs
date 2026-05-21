#region Licence
/****************************************************************
 *  Filename: OpenVrSettingEntityDetails.cs
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
using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.Modules.Vr
{
    
    public class OpenVrSettingEntityDetails : IOpenVrSettingEntityDetails
    {
        #region Fields & Properties

        public OpenVrConfigLocation BaseLocation { get; set; }
        public string RelativePath { get; set; }
        public OpenVrConfigFileType FileType { get; set; }
        public OpenVrConfigApplyBehavior? BehaviorOverride { get; set; }

        #endregion Fields & Properties

        #region Contructors
        public OpenVrSettingEntityDetails(){}
        public OpenVrSettingEntityDetails(OpenVrConfigLocation baseLocation, string relativePath, OpenVrConfigFileType fileType, OpenVrConfigApplyBehavior? behaviorOverride = null)
        {
            BaseLocation = baseLocation;
            RelativePath = relativePath;
            FileType = fileType;
            BehaviorOverride = behaviorOverride;
        }

        #endregion Contructors

        #region Methods

        //

        #endregion Methods
    }
}
