#region Licence
/****************************************************************
 *  Filename: ShellConfigurator.cs
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

using System;
using System.IO;
using System.Linq;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.Properties;
using NLog;

namespace LeapVR.Shell.Modules.ShellConfigurator
{
    public class ShellConfigurator
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DiskConfig _diskConfig;

        public bool HasValidDiskConfig => IsDiskConfigValid();
        public ShellConfigurator(
            IConfigFileRepository<DiskConfig> diskConfigFileRepository)
        {
            QuickLeap.AssertNotNull(diskConfigFileRepository);
            _diskConfig = diskConfigFileRepository.Get();
        }

        private bool IsDiskConfigValid()
        {
            if(_diskConfig?.SystemDrives == null || !_diskConfig.SystemDrives.Any() || String.IsNullOrEmpty(_diskConfig.StorageBaseDir))
            {
                return false;
            }
            //Check if we the Drive and Directory is still existing
            try 
            {  
                DriveInfo info = new DriveInfo(_diskConfig.StorageBaseDir);
                if(info.TotalSize > 0)
                {
                    var dirInfo= new DirectoryInfo(_diskConfig.StorageBaseDir);
                    if(!dirInfo.Exists) dirInfo.Create();
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                Logger.Warn(e,$"Drive Info for Path ={_diskConfig.StorageBaseDir}could not be received!");
                return false;
            }
        }
    }
}