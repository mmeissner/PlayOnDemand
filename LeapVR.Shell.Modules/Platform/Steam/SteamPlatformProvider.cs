#region Licence
/****************************************************************
 *  Filename: SteamPlatformProvider.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Modules.Vr;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Utilities.Steam.Steam;
using NLog;

namespace LeapVR.Shell.Modules.Platform.Steam
{
    internal class SteamPlatformProvider
    {
        private readonly TimeSpan _appInitializationTimeout = new TimeSpan(0,0,5,0); 
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly SteamLib _steamLib;
        private readonly object _dataAccessLock = new object();
        private readonly ManualResetEventSlim _waitForScan = new ManualResetEventSlim(false);
        private readonly IPlatformModule _platformModule;
        private readonly Dictionary<Guid, IAppPlatformData> _platformApps = new Dictionary<Guid, IAppPlatformData>();

        #region Constructor
        public SteamPlatformProvider(IPlatformModule platformModule,SteamLib steamLib)
        {
            _steamLib = steamLib;
            _platformModule = platformModule;
            Task.Run(
                    () =>
                    {
                        lock(_dataAccessLock)
                        {
                            var steamGames = _steamLib.GetSteamAppInfo();
                            foreach(var appInfo in steamGames)
                            {
                                var platformData = ToPlatformData(_platformModule.PlatformNameId,appInfo);
                                _platformApps.Add(platformData.ApplicationGuid, platformData);
                            }
                            //Set to let through GetPlatformApps Requests
                            _waitForScan.Set();
                        }
                    });
        }
        #endregion

        #region Public Methods
        internal Dictionary<Guid, IAppPlatformData> GetLocalApplications()
        {
            var cts = new CancellationTokenSource(_appInitializationTimeout);
            return InternalGetLocalApplications(cts.Token);
        }

        internal IAppPlatformData GetLocalApplication(Guid applicationId)
        {
            var cts = new CancellationTokenSource(_appInitializationTimeout);
            return InternalGetLocalApplication(applicationId,cts.Token);
        }

        internal Dictionary<Guid, IAppPlatformData> RefreshLocalApplications()
        {
            lock(_dataAccessLock)
            {
                var steamGames = _steamLib.GetSteamAppInfo();
                var currentAppIds = new HashSet<Guid>(_platformApps.Keys);
                foreach(var appInfo in steamGames)
                {
                    var platformData = ToPlatformData(_platformModule.PlatformNameId,appInfo);
                    if(_platformApps.ContainsKey(platformData.ApplicationGuid))
                    {
                        _platformApps[platformData.ApplicationGuid] = platformData;
                        currentAppIds.Remove(platformData.ApplicationGuid);
                    }
                    else
                    {
                        _platformApps.Add(platformData.ApplicationGuid, platformData);
                    }
                }

                foreach(Guid appId in currentAppIds)
                {
                    _platformApps.Remove(appId);
                }

                return GetLocalApplications();
            }
        }

        internal bool IsApplicationAvailable(Guid appId)
        {
            lock(_dataAccessLock)
            {
                if(_platformApps.TryGetValue(appId, out var platformData) && platformData.IsEnabled) return true;
                return false;
            }
        }
        #endregion

        #region Private Methods
        private Dictionary<Guid, IAppPlatformData> InternalGetLocalApplications(CancellationToken ct)
        {
            try
            {
                //Wait for beeing able to get Platforms as they need first to be scanned
                //and scanning happens on an seperate Task during construction
                _waitForScan.Wait(ct);
                lock(_dataAccessLock)
                {
                    return new Dictionary<Guid, IAppPlatformData>(_platformApps);
                }
            }
            catch(Exception exception)
            {
                if(exception is TaskCanceledException taskCanceled)
                {
                    Logger.Warn($"{nameof(InternalGetLocalApplications)} request was canceled");
                    return null;
                }
                throw;
            }
        }

        private IAppPlatformData InternalGetLocalApplication(Guid applicationId,CancellationToken ct)
        {
            try
            {
                //Wait for beeing able to get Platforms as they need first to be scanned
                //and scanning happens on an separate Task during construction
                _waitForScan.Wait(ct);
                lock(_dataAccessLock)
                {
                    if(_platformApps.ContainsKey(applicationId)) return _platformApps[applicationId];
                    return null;
                }
            }
            catch(Exception exception)
            {
                if(exception is TaskCanceledException taskCanceled)
                {
                    Logger.Warn($"{nameof(InternalGetLocalApplications)} request was canceled");
                    return null;
                }
                throw;
            }
        }

        /// <summary>
        /// Creates an IAppPlatformData instance from the SteamAppInfo provided
        /// </summary>
        /// <param name="platformNameId">The platform name identifier.</param>
        /// <param name="steamApp">The steam application.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static IAppPlatformData ToPlatformData(string platformNameId,SteamAppInfo steamApp)
        {
            var applicationGuid = PlatformConverter.CreateGuid(platformNameId,steamApp.AppId);
            var platformData =
                    new AppPlatformData
                    {
                            ApplicationGuid = applicationGuid,
                            ApplicationName = steamApp.DisplayName,
                            PlatformPluginId = SteamPlatformModule.PluginGuid,
                    };
            var listExecutionLogic = new List<IProcessExecutionLogic>();

            foreach(var steamAppLaunchInfo in steamApp.LaunchInfo)
            {
                var exeInstruction = ProcessMonitorOption.IsMainExecutable;
                exeInstruction |= ProcessMonitorOption.KillOnExit;
                if(!File.Exists(steamAppLaunchInfo.ExecutableFilePath)) continue;
                var executionLogic = new SteamProcessExecutionLogic
                                     {
                                             ApplicationGuid = applicationGuid,
                                             DisplayName = steamAppLaunchInfo.Description,
                                             ExecutionGuid = Guid.NewGuid(),
                                             ExecutionFile =
                                                     new DiskEntity
                                                     {
                                                             ApplicationGuid = applicationGuid,
                                                             PlatformGuid = SteamPlatformModule.PluginGuid,
                                                             PackageGuid = Guid.Empty,
                                                             Path = steamAppLaunchInfo.ExecutableFilePath,
                                                             Type = DiskEntityType.Absolute
                                                     },
                                             PlatformPluginId = SteamPlatformModule.PluginGuid,
                                             ExecutionParameters = steamAppLaunchInfo.Arguments,
                                             MonitorInstructions =
                                                     new List<IProcessMonitorInstruction>()
                                                     {
                                                             new
                                                             SteamProcessMonitorInstruction()
                                                             {
                                                                     ExecutableRelativePathFileName
                                                                             =
                                                                             steamAppLaunchInfo.ExecutableFilePath.
                                                                                                Substring(
                                                                                                        steamApp.
                                                                                                                AppDirectory.
                                                                                                                Length +
                                                                                                        1),
                                                                     Instruction
                                                                             = exeInstruction
                                                             }
                                                     }.ToArray(),
                                     };
                switch(steamAppLaunchInfo.Type)
                {
                    case LaunchType.Screen:
                        break;
                    case LaunchType.OpenVr:
                        executionLogic.ReguiredVrModuleGuid = VrConstants.OpenVrModuleId.ToString();
                        break;
                    case LaunchType.Oculus:
                        executionLogic.ReguiredVrModuleGuid = VrConstants.OculusVrModuleId.ToString();
                        break;
                    case LaunchType.OtherVr:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                listExecutionLogic.Add(executionLogic);
            }

            platformData.ExecutionLogicInstructions = listExecutionLogic;
            return platformData;
        }
        #endregion
    }
}