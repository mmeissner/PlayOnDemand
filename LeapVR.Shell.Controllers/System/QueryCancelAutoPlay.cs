#region Licence
/****************************************************************
 *  Filename: QueryCancelAutoPlay.cs
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
using System.Runtime.InteropServices;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using LeapVR.Utilities.Windows;
using LeapVR.Utilities.Windows.AutoPlay;
using Microsoft.Win32;
using NLog;

namespace LeapVR.Shell.Controllers.System
{
    [ComVisible(true)]
    [Guid(GuidString)]
    [ProgId("6373b0bd-3159-4d13-8c91-dae97436570c")]
    [ClassInterface(ClassInterfaceType.None)]
    public class QueryCancelAutoPlay : IQueryCancelAutoPlay, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string GuidString = @"c910a0dd-7acb-4ec6-a106-2b4982df2921";

        private RunningObjectTableEntry _rotEntry;
        public void Initialize()
        {
            try
            {
                var key ="SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\AutoplayHandlers\\CancelAutoplay\\CLSID";
                if(!RegistryUtil.KeyExist(RegistryHive.LocalMachine,key) || RegistryUtil.GetValueData(RegistryHive.LocalMachine,key,GuidString)==null)
                {
                    Logger.Info("Autoplay Handler Key does not exist, trying to create handler!");
                    if(!RegistryUtil.SetValue(
                            RegistryHive.LocalMachine,
                            key,
                            GuidString,"",
                            RegistryValueKind.String))
                    {
                        Logger.Error("Could not set AutoPlay Handler Value!");
                    }
                    else
                    {
                        Logger.Info("Autoplay handler successfully set!");
                    }
                }
                
                Logger.Debug("Creating Running Object Table Entry!");
                _rotEntry = new RunningObjectTableEntry(this);
            }
            catch (Exception e)
            {
                Logger.Error(e,"Error during setting Autoplay cancelation registry entry!");
            }
        }

        public int AllowAutoPlay([MarshalAs(UnmanagedType.LPWStr)] string pszPath, [MarshalAs(UnmanagedType.U4)] int dwContentType, [MarshalAs(UnmanagedType.LPWStr)] string pszLabel, [MarshalAs(UnmanagedType.U4)] int dwSerialNumber)
        {
            Logger.Debug("Autoplay Callback invoked, returning 1 to block Autoplay");
            return 1; // 1 = block AutoPlay
        }

        public void Dispose()
        {
            _rotEntry?.Dispose();
        }
    }
}
