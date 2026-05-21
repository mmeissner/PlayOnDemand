#region Licence
/****************************************************************
 *  Filename: OpenVrModule.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces.Repositories;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Utilities.Steam.Steam;
using NLog;

// ReSharper disable ExplicitCallerInfoArgument

namespace LeapVR.Shell.Modules.Vr
{
    public class OpenVrModule : IOpenVrModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _stateLock = new object();

        private readonly IConfigFileRepository<OpenVrModuleConfig> _openVrModuleConfigRepository;
        private readonly IOpenVrSettingsSetRepository _openVrSettingsSetRepository;
        private readonly SteamLib _steamLib;
        private static OpenVrProcessHandler _openVrProcessHandler;

        #region Properties & Fields
        public Guid ModuleId => VrConstants.OpenVrModuleId;
        public string ModuleName => VrConstants.OpenVrModuleName;
        public bool IsAvailible { get; }
        public bool IsRunning
        {
            get
            {
                var isRunning = _openVrProcessHandler?.IsRunning;
                return isRunning.HasValue && isRunning.Value;
            }
        }
        #endregion Properties & Fields

        #region Constructors
        public OpenVrModule(
                IConfigFileRepository<OpenVrModuleConfig> openVrModuleConfigRepository,
                IOpenVrSettingsSetRepository openVrSettingsSetRepository,
                SteamLib steamLib)
        {
            QuickLeap.AssertNotNull(openVrModuleConfigRepository, steamLib, openVrSettingsSetRepository);
            IsAvailible = OpenVR.Wrapper.OpenVR.IsRuntimeInstalled();
            Logger.Info($"Open VR Availibility={IsAvailible}");
            if(IsAvailible)
            {
                _openVrModuleConfigRepository = openVrModuleConfigRepository;
                _openVrSettingsSetRepository = openVrSettingsSetRepository;
                _steamLib = steamLib;
            }
        }
        #endregion Constructors

        #region Public Methods
        public string DisplayName => "OpenVR";
        public bool StartOnlyVrDriver() { return StartVrDriver(false,null, null); }
        public bool StartVrDriver(bool disableInteraction,TransparencyAreaCallBack transparencyAreaCallback, Action restartVrGui)
        {
            if(!IsAvailible) return false;
            lock(_stateLock)
            {
                Logger.Info($"Start VR Driver Called with TransparencyAreaCallBack={transparencyAreaCallback}");
                var processHandler = GetOrCreateProcessHandler(disableInteraction,transparencyAreaCallback,restartVrGui);
                var retval = processHandler.StartVrDriver();
                Logger.Info($"Start VR Driver Finished with result ={retval}");
                return retval;
            }
        }
        public void StopVrDriver()
        {
            if(!IsAvailible) return;
            lock(_stateLock)
            {
                Logger.Info("Stop VR Driver Called");

                GetOrCreateProcessHandler().StopVrDriver();
                Logger.Info("Stop VR Driver Finished");
            }
        }
        public bool HasModuleSupport(Guid submoduleGuid)
        {
            if(submoduleGuid != ModuleId) return false;
            return true;
        }

        public bool GetWatchdog(out IHmdActivityWatchdog watchdog)
        {
            lock(_stateLock)
            {
                return GetOrCreateProcessHandler().GetWatchdog(out watchdog);
            }
        }
        #endregion

        #region Private Methods
        private OpenVrProcessHandler GetOrCreateProcessHandler(bool disableInteraction =false,TransparencyAreaCallBack transparencyAreaCallback = null,Action restartVrGui = null)
        {
            Logger.Debug("Called GetOrCreateProcessHandler");
            if(_openVrProcessHandler == null || _openVrProcessHandler.HasError || !_openVrProcessHandler.IsRunning)
            {                
                Logger.Debug("Creating new OpenVR ProcessHandler");
                _openVrProcessHandler = new OpenVrProcessHandler(
                        _openVrModuleConfigRepository,
                        _openVrSettingsSetRepository,
                        _steamLib,
                        transparencyAreaCallback,
                        disableInteraction,
                        restartVrGui);
                return _openVrProcessHandler;
            }
            Logger.Debug("Returning existing OpenVR ProcessHandler");
            return _openVrProcessHandler;
        }
        #endregion
    }
}