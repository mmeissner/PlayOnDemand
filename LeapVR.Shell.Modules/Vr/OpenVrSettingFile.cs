#region Licence
/****************************************************************
 *  Filename: OpenVrSettingFile.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.Modules.Vr
{
    
    public class OpenVrSettingFile : IOpenVrSettingFile
    {
        #region Fields & Properties
        public IOpenVrSettingEntityDetails EntityDetails { get; }

        public string FileContent { get; }
        #endregion Fields & Properties

        #region Contructors
        internal OpenVrSettingFile(IOpenVrSettingEntityDetails entityDetails, string fileContent)
        {
            QuickLeap.AssertNotNull(entityDetails, fileContent);

            EntityDetails = entityDetails;
            FileContent = fileContent;
        }
        #endregion Contructors
    }
}