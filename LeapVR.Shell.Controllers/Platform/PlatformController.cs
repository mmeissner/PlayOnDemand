#region Licence
/****************************************************************
 *  Filename: PlatformController.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.Platform.Account;
using LeapVR.Shell.Controllers.Platform.Installation;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;
using Pod.Data.Infrastructure;

namespace LeapVR.Shell.Controllers.Platform
{
    /// <inheritdoc />
    public class PlatformController : IPlatformController
    {
        #region Properties & Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _installationLock = new object();
        private readonly HashSet<Guid> _installLockedApps = new HashSet<Guid>();
        private readonly Dictionary<Guid,Platform> _platformDic;
        private readonly IUIMessageBroker _uiMessageBroker;
        private readonly InstallationManager _installationManager;
        private readonly IDiskController _diskController;
        private readonly IVirtualRealityController _virtualRealityController;
        private readonly IFirewallController _firewallController;
        private readonly IAppInstallationRepository _appInstallationRepository;
        private readonly IAppDisplayRepository _appDisplayRepository;
        private readonly IAppPlatformRepository _appPlatformRepository;
        private readonly IAppInfoProcessor _appPreExecutionProcessor;
        private readonly ICategoryProvider _categoryProvider;
        private readonly AccountManager _accountManager;
        #endregion Properties & Fields

        /// <summary>
        /// Provides ability to subscribe objects to get informed about DisplayUpdates.
        /// Allows objects to raise "event" on Update
        /// </summary>
        /// <value>
        /// The when application display update.
        /// </value>
        internal Subject<AppDisplayUpdate> WhenAppDisplayUpdate { get; } = new Subject<AppDisplayUpdate>();
        internal Subject<AppExecutablesUpdate> WhenAppExecutablesUpdate { get; } = new Subject<AppExecutablesUpdate>();

        #region Constructors
        public PlatformController(
            IEnumerable<IPlatformModule> platformModules,
            IAppInstallationRepository appInstallationRepository,
            IAppDisplayRepository appDisplayRepository,
            IAppPlatformRepository appPlatformRepository,
            IAppPlatformAccountRepository appPlatformAccountRepository,
            IAppInfoProcessor appPreExecutionProcessor,
            IDiskController diskController,
            IFirewallController firewallController,
            IVirtualRealityController virtualRealityController,
            ICategoryProvider categoryProvider,
            ILocalMachine localMachine,
            IUIMessageBroker uiMessageBroker)
        {
            QuickLeap.AssertNotNull(
                platformModules,
                appInstallationRepository,
                appDisplayRepository,
                appPlatformRepository,
                appPlatformAccountRepository,
                diskController,
                virtualRealityController,
                categoryProvider,
                localMachine,
                uiMessageBroker);
            _uiMessageBroker = uiMessageBroker;
            _platformDic = new Dictionary<Guid, Platform>();
            _installationManager = new InstallationManager(
                    appInstallationRepository,
                    diskController);
            _virtualRealityController = virtualRealityController;
            _accountManager = new AccountManager(appPlatformAccountRepository, localMachine);
            _appInstallationRepository = appInstallationRepository;
            _appDisplayRepository = appDisplayRepository;
            _appPlatformRepository = appPlatformRepository;
            _appPreExecutionProcessor = appPreExecutionProcessor;
            _diskController = diskController;
            _firewallController = firewallController;
            _categoryProvider = categoryProvider;

            foreach(var module in platformModules)
            {
                var platform = new Platform(module,_diskController, this,_firewallController,_installationManager,uiMessageBroker,_appDisplayRepository, _accountManager,_categoryProvider);
                _platformDic.Add(module.ModuleId,platform);
            }

            WhenAppDisplayUpdate.Subscribe(OnAppDisplayUpdated);
            WhenAppExecutablesUpdate.Subscribe(OnAppExecutableUpdated);
        }
        #endregion Constructors


        #region Application Get and Set Methods        
        public IEnumerable<IAppPlatformInfo> GetAvailableApplications()
        {
            try
            {
                List<IAppPlatformInfo> retval = new List<IAppPlatformInfo>();
                foreach(IAppPlatformData app in _appPlatformRepository.GetAll().Where(x => x.IsEnabled))
                {
                    if(TryGetAppFromPlatform(_platformDic, app, out var platformInfo) && IsApplicationAvailable(platformInfo))
                    {
                        retval.Add(platformInfo);
                    }
                    else
                    {
                        Logger.Warn($"App with Guid={app.ApplicationGuid} is enabled and has PlatformData, but could not receive Display Data!");
                    }
                }
                return retval;
            }
            catch(RepositorySerializationException exception)
            {
                Logger.Error(exception, $"{nameof(GetAvailableApplications)} Error occured!");
                //TODO: Publish an Error Dialog Message
                //_uiMessageBroker.Publish();
                return new List<IAppPlatformInfo>();
            }
        }

        public bool IsAvailible(Guid applicationGuid) { return TryGetAvailableApplication(applicationGuid, out _); }

        public bool TryGetAvailableApplication(Guid applicationGuid, out IAppPlatformInfo platformApp)
        {
            try
            {
                if(_appPlatformRepository.TryGetEnabledApp(applicationGuid, out var platformAppData))
                {
                    if(TryGetAppFromPlatform(_platformDic, platformAppData, out platformApp) && IsApplicationAvailable(platformApp))
                    {
                        return true;
                    }
                }
                platformApp = null;
                return false;
            }
            catch(RepositorySerializationException exception)
            {
                Logger.Error(exception, $"{nameof(GetAvailableApplications)} Error occured!");
                //TODO: Publish an Error Dialog Message
                //_uiMessageBroker.Publish();
                platformApp = null;
                return false;
            }
        }

        public IEnumerable<IAppPlatformInfo> GetInstalledApplications()
        {
            var applicationsToDisplay = GetApplicationInstallationData().
                    Where(x => x.InstallationState == InstallationState.Installed);
            return from installationData in applicationsToDisplay
                   select GetInstalledApplication(installationData.ApplicationGuid,installationData.PlatformPluginGuid) 
                   into appPlatformInfo where appPlatformInfo != null
                   select appPlatformInfo;
        }

        //TODO: Refactor Code and remove GetInstalledApplication from Platform Controller as public method
        public IAppPlatformInfo GetInstalledApplication(Guid applicationGuid, Guid platformGuid)
        {
            //Platform with App Provider
            if(_platformDic.TryGetValue(platformGuid, out var platform))
            {
                return platform.GetInstalledPlatformApp(applicationGuid);
            }
            return null;
        }

        public IEnumerable<IAppInstallationData> GetApplicationInstallationData(AppInstallationType type)
        {
            try
            {
                return _appInstallationRepository.GetAll().Where(x=> x.Type == type);
            }
            catch(RepositorySerializationException exception)
            {
                Logger.Error(exception, $"{nameof(GetApplicationInstallationData)} Error occured!");
                //TODO: Publish an Error Dialog Message
                //_uiMessageBroker.Publish();
                return new List<IAppInstallationData>();
            }
        }

        public IAppInstallationData GetApplicationInstallationData(Guid applicationGuid)
        {
            try
            {
                return _appInstallationRepository.Get(applicationGuid);
            }
            catch(Exception exception)
            {
                if(!(exception is RepositoryGetDbException) && !(exception is RepositorySerializationException)) throw;
                Logger.Error(exception, $"{nameof(GetApplicationInstallationData)} Error occured!");
                //TODO: Publish an Error Dialog Message
                //_uiMessageBroker.Publish();
                return null;
            }
        }

        public HashSet<Guid> GetLockedApplications()
        {
            lock(_installationLock) { return new HashSet<Guid>(_installLockedApps); }
        }
        #endregion

        #region Application Execution and Termination Methods
        public IAppExecutionInfo GetAppExecutionInfo(Guid applicationGuid, bool needsFullfilledRequirements)
        {
            try
            {
                var platformData = _appPlatformRepository.Get(applicationGuid);

                var displays =
                        _appPreExecutionProcessor.GetExecutionInfoResult(platformData.ExecutionLogicInstructions,needsFullfilledRequirements);
                var appExecutionInfo = new AppExecutionInfo()
                                           {
                                                   ExecutionCandidates = displays.ToArray()
                                           };
                return appExecutionInfo;
            }
            catch(Exception exception)
            {
                if(!(exception is RepositoryGetDbException) && !(exception is RepositorySerializationException)) throw;
                Logger.Error(exception, $"{nameof(GetAppExecutionInfo)} Error occured!");
                //TODO: Publish an Error Dialog Message
                //_uiMessageBroker.Publish();
                return null;
            }
        }

        public IApplicationExecution RequestExecutionObject(IExecuteable executable)
        {
            var internalDisplayInfo = executable as Executeable;
            var executionLogic = internalDisplayInfo?.ExecutionLogic;
            if(executionLogic == null)
            {
                Logger.Warn(
                    $"Could not find {nameof(IProcessExecutionLogic)} for {nameof(Executeable)} '{executable.DisplayName}'");
                return null;
            }
            try
            {
                var platformData = _appPlatformRepository.Get(executionLogic.ApplicationGuid);
                if(platformData == null)
                {
                    Logger.Error($"Could not receive PlatformData for Execution of App with Guid={executionLogic.ApplicationGuid}");
                    return null;
                }
                var platformInfo = GetInstalledApplication(platformData.ApplicationGuid, platformData.PlatformPluginId);
                if(platformInfo == null)
                {
                    Logger.Error($"Could not receive PlatformInfo for Execution of App with Guid={executionLogic.ApplicationGuid}");
                    return null;
                }
                return CreateApplicationExecution(platformInfo,platformData,executionLogic);
            }
            catch(Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
        #endregion

        #region Install & Uninstall Platform Apps
        internal void Install(AppPlatformInfo platformInfo)
        {
            if(!platformInfo.CanInstall())return;
            try
            {
                lock(_installationLock)
                {
                    if(platformInfo.ApplicationGuid == Guid.Empty ||
                       _installLockedApps.Contains(platformInfo.ApplicationGuid))
                    {
                        Logger.Error("Error during attempt to install platform app, Guid is empty or app is currently locked");
                        return;
                    }
                    if(_platformDic.TryGetValue(platformInfo.PlatformGuid, out var platform))
                    {
                        var platformInstallationProcess = platform.Install(platformInfo, FinalizeInstall);
                        if(platformInstallationProcess == null)
                        {
                            Logger.Error($"Error during attempt to install platform app, platform returned null for {nameof(platformInstallationProcess)}");
                            return;
                        }
                        _installLockedApps.Add(platformInstallationProcess.ApplicationGuid);
                        _uiMessageBroker.Publish(new UIPlatformAppInstallationStartedEvent(platformInstallationProcess));
                        platformInstallationProcess.BeginInstall();
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Error during attempt to install platform app");
                //TODO IUIBroker Publish UI Error Message Dialog that Uninstallation Attemped failed
            }
        }

        public void Uninstall(IAppPlatformInfo platformInfo, bool tryFullUninstall)
        {
            if(!platformInfo.CanUninstall()) return;
            try
            {
                lock(_installationLock)
                {
                    if(_installLockedApps.Contains(platformInfo.ApplicationGuid) || platformInfo.ApplicationGuid == Guid.Empty)
                    {
                        Logger.Warn($"Application Uninstallation denied as app is under lock. applicationGuid={platformInfo.ApplicationGuid}");
                        return;
                    }
                    _installLockedApps.Add(platformInfo.ApplicationGuid);
                    platformInfo.SetEnabled(false);
                }

                AppUninstallationType installationType = tryFullUninstall ? AppUninstallationType.PlatformUninstall :AppUninstallationType.PlatformRemove;
                if(_installationManager.CreateUninstallationProcess(
                        platformInfo,
                        installationType,
                        out var uninstallationProcess))
                {
                    Logger.Debug($"Created Successfully Uninstallation Process for App with Guid={platformInfo.ApplicationGuid}");
                    //IUIBroker Publish Message Uninstall Process Started
                    _uiMessageBroker.Publish(new UIAppUninstallationStartedEvent(uninstallationProcess));

                    _firewallController.RemoveAllRules(platformInfo.ApplicationGuid);
                    _installationManager.StartUninstallationProcess(uninstallationProcess, FinalizeUninstall);
                }
                else
                {
                    //TODO IUIBroker Publish Message Error
                    Logger.Warn("Application can not be removed, CreateUninstallationProcess returned false");
                    lock (_installationLock)
                    {
                        _installLockedApps.Remove(platformInfo.ApplicationGuid);
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Error during attempt to install container");
                //TODO IUIBroker Publish UI Error Message Dialog that Uninstallation Attemped failed
            }
        }
        #endregion

        #region Install & Uninstall Local Content / VBox Container Apps
        public CanInstallStatus CanInstall(IAppInstallationContainer<IContainerPackage> container)
        {
            return _installationManager.CanInstall(container);
        }

        public CanInstallStatus CanInstall(Guid applicationGuid)
        {
            return _installationManager.CanInstall(applicationGuid);
        }

        public CanUninstallStatus CanUninstall(Guid applicationGuid)
        {
            return _installationManager.CanUninstall(applicationGuid);
        }

        public void Install(IAppInstallationContainer<IContainerPackage> container)
        {
            Logger.Info( $"Application Installation requested. DisplayName={container.DisplayName}");
            try
            {
                lock(_installationLock)
                {
                    QuickLeap.AssertNotNull(container);
                    if(_installLockedApps.Contains(container.ApplicationGuid) || container.ApplicationGuid == Guid.Empty)
                    {
                        Logger.Warn($"Application Installation denied as app is under lock. ApplicationGuid={container.ApplicationGuid}");
                        return;
                    }
                    if(_installationManager.CreateInstallationProcess(container, out var installationProcess))
                    {
                        if(installationProcess.ApplicationGuid == Guid.Empty ||
                           _installLockedApps.Contains(installationProcess.ApplicationGuid))
                        {
                            return;
                        }
                        try
                        {
                            _installLockedApps.Add(installationProcess.ApplicationGuid);
                            //IUIBroker Publish Message with InstallationProcess to UI so that he can monitor and display progress
                            _uiMessageBroker.Publish(new UIAppInstallationStartedEvent(installationProcess));

                            //Start Installation Process after everyone interessted has received the Message with the process
                            _installationManager.StartInstallationProcess(installationProcess, FinalizeInstall);
                        }
                        catch(Exception exception)
                        {
                            Logger.Error(exception, "Exception occured during attempt to Install");

                            //If anything goes wrong before the action starts we need to remove the application lock
                            _installLockedApps.Remove(installationProcess.ApplicationGuid);
                            throw;
                        }
                    }
                    //TODO IUIBroker Publish UI Info Dialog Message that informs user that Uninstallation is not possible
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Error during attempt to install container");
                //TODO IUIBroker Publish UI Error Message Dialog that Uninstallation Attemped failed
            }
        }

        public void Uninstall(Guid applicationGuid,Guid platformGuid, bool useMinimalDisplayInfo = false)
        {
            try
            {
                Logger.Info( $"Application Uninstallation requested. applicationGuid={applicationGuid}");
                lock(_installationLock)
                {
                    if(_installLockedApps.Contains(applicationGuid) || applicationGuid == Guid.Empty)
                    {
                        Logger.Warn($"Application Uninstallation denied as app is under lock. applicationGuid={applicationGuid}");
                        return;
                    }
                    _installLockedApps.Add(applicationGuid);
                    SetApplicationIsEnabled(applicationGuid, false);
                }

                IAppDisplayInfo displayInfo = null;
                if(!useMinimalDisplayInfo)displayInfo = GetInstalledApplication(applicationGuid,platformGuid);

                //Might be a broken installation
                if(displayInfo == null &&
                   _installationManager.CreateUninstallationProcess(applicationGuid, AppUninstallationType.PlatformUninstall,out var uninstallationProcess) ||
                   _installationManager.CreateUninstallationProcess(displayInfo, AppUninstallationType.PlatformUninstall,out uninstallationProcess))
                {
                    Logger.Debug($"Created Successfully Uninstallation Process with DisplayInfo={displayInfo}");
                    //IUIBroker Publish Message Uninstall Process Started
                    _uiMessageBroker.Publish(new UIAppUninstallationStartedEvent(uninstallationProcess));

                    _firewallController.RemoveAllRules(applicationGuid);
                    _installationManager.StartUninstallationProcess(uninstallationProcess, FinalizeUninstall);
                }
                else
                {
                    //TODO IUIBroker Publish Message Error
                    Logger.Warn("Application can not be removed due to inconsistent state");
                    lock (_installationLock)
                    {
                        _installLockedApps.Remove(applicationGuid);
                    }
                }
                Logger.Debug($"Application Uninstallation left for ApplicationGuid={applicationGuid}");
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Exception occured during attempt to Uninstall");
                //If anything goes wrong before the action starts we need to remove the application lock
                lock (_installationLock)
                {
                    _installLockedApps.Remove(applicationGuid);
                }
                throw;
            }
        }
        #endregion

        #region Plaform Functionalities
        public IEnumerable<IPlatform> GetPlatforms() { return _platformDic.Values; }
        #endregion

        #region Station Message Interface
        public void OnStationMessage(StationMessage messages)
        {
            switch(messages)
            {
                case StationMessage.InitStart:
                    _installationManager.CleanUpInstallations();
                    break;
            }
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Determine if app does support VR Mode.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns></returns>
        internal bool ShowVRSupport(Guid applicationGuid)
        {
            var platformData = _appPlatformRepository.Get(applicationGuid);
            if (platformData != null)
            {
                return _appPreExecutionProcessor.GetExecutionInfoResult(platformData.ExecutionLogicInstructions,true).Any(executeablese => executeablese.IsVirtualRealityRequired);
            }
            return false;
        }

        /// <summary>
        /// Determine if app does support Screen Mode.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns></returns>
        internal bool ShowScreenSupport(Guid applicationGuid)
        {
            var platformData = _appPlatformRepository.Get(applicationGuid);
            if (platformData != null)
            {
                return _appPreExecutionProcessor.GetExecutionInfoResult(platformData.ExecutionLogicInstructions,true).Any(executeablese => executeablese.IsScreenModeSupported);
            }
            return false;
        }

        /// <summary>
        /// Publishes an Message that the applications Display Info changed.
        /// </summary>
        /// <param name="displayInfo">The display information.</param>
        /// <param name="update">The update.</param>
        internal void PublishAppInfoChanged(AppDisplayInfo displayInfo, AppDisplayUpdate update)
        {
            _uiMessageBroker.Publish(new UIAppDisplayInfoChanged(displayInfo,update));
        }

        /// <summary>
        /// Determines whether an Application with the specified application identifier is enabled.
        /// Does not check for other required conditions as Platform availability
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is application enabled] [the specified application identifier]; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsApplicationEnabled(Guid applicationId)
        {
            return _appPlatformRepository.Get(applicationId).IsEnabled;
        }

        /// <summary>
        /// Changes specified application's <see cref="isEnabled"/> status.
        /// </summary>
        /// <param name="applicationGuid">Guid of application</param>
        /// <param name="isEnabled">value to set</param>
        internal bool SetApplicationIsEnabled(Guid applicationGuid, bool isEnabled)
        {
            try
            {
                Logger.Debug($"Setting Application with Guid={applicationGuid} Enabled={isEnabled}");
                if(_appPlatformRepository.SetAppEnabled(applicationGuid, isEnabled, out var platformId))
                {
                    Logger.Debug($"Publishing Async Application with Guid={applicationGuid} enabled={isEnabled}");
                    _uiMessageBroker.Publish(new UIAppEnabledStateChangedEvent(applicationGuid,platformId, isEnabled));
                    return true;
                }

                return false;
            }
            catch(RepositoryStoreDbException exception)
            {
                Logger.Error(exception, $"{nameof(SetApplicationIsEnabled)} Error occured!");
                //TODO: Publish an Error Dialog Message
                //_uiMessageBroker.Publish();
                return false;
            }
        }

        internal bool TryGetAppExecutableUpdate(Guid applicationGuid, out IAppExecutablesUpdate executableUpdate)
        {
            executableUpdate = null;
            var appPlatformData =_appPlatformRepository.Get(applicationGuid);
            if(appPlatformData == null) return false;
            executableUpdate = new AppExecutablesUpdate(this,_virtualRealityController.GetSelectableVrModules(appPlatformData),appPlatformData);
            return true;
        }
        #endregion

        #region Private Methods    
        private IEnumerable<IAppInstallationData> GetApplicationInstallationData()
        {
            try
            {
                return _appInstallationRepository.GetAll();
            }
            catch(RepositorySerializationException exception)
            {
                Logger.Error(exception, $"{nameof(GetApplicationInstallationData)} Error occured!");
                return new List<IAppInstallationData>();
            }
        }

        private bool IsApplicationAvailable(IAppPlatformInfo platformInfo)
        {
            if(!platformInfo.IsEnabled) return false;
            if(!platformInfo.IsLicenseRequired || platformInfo.IsLicenseRequired && platformInfo.HasPlatformAccount())return true;
            return false;
        }
        private bool TryGetAppFromPlatform(Dictionary<Guid,Platform> platformDic,IAppPlatformData platformData, out IAppPlatformInfo platformInfo)
        {
            //Platform with App Provider
            if(platformDic.TryGetValue(platformData.PlatformPluginId, out var platform))
            {
                platformInfo = platform.GetInstalledPlatformApp(platformData.ApplicationGuid);
                if(platformInfo != null)
                {
                    return true;
                }
                return false;
            }
            platformInfo = null;
            return false;
        }

        /// <summary>
        /// Called when ApplicationDisplayInfo changed and persists changes.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnAppDisplayUpdated(AppDisplayUpdate obj)
        {
            var displayData = _appDisplayRepository.Get(obj.ApplicationId);
            if(obj.CategoryChanged) displayData.Category = obj.Category.Identifier;
            if(obj.DescriptionChanged) displayData.Description = obj.Description;
            if(obj.NameChanged) displayData.Name = obj.Name;
            _appDisplayRepository.Store(displayData);
        }

        /// <summary>
        /// Called when ApplicationDisplayInfo changed and persists changes.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void OnAppExecutableUpdated(AppExecutablesUpdate obj)
        {
            var newExecutionInstructions = new List<IProcessExecutionLogic>();
            foreach(var executionLogic in obj.Execution)
            {
                newExecutionInstructions.Add(executionLogic.Convert());
            }
            _appPlatformRepository.Update(obj.ApplicationId, newExecutionInstructions);
        }
    
        private IApplicationExecution CreateApplicationExecution(
            IAppPlatformInfo platformInfo,
            IAppPlatformData platformData,
            IProcessExecutionLogic executionLogic)
        {
            var appPlatformData = platformData;
            if(appPlatformData == null)
            {
                Logger.Debug( "Failed to execute logic. PlatformData is null");
                return null;
            }
            if(!_platformDic.TryGetValue(appPlatformData.PlatformPluginId, out var platform))
            {
                Logger.Warn(
                    $"Failed to execute logic. Can not find {nameof(IPlatform)} for {nameof(IAppPlatformData.PlatformPluginId)} of {appPlatformData.PlatformPluginId}. Return null of type {nameof(IApplicationExecution)}.");
                return null;
            }

            Logger.Debug(
                $"Try to perform {nameof(IPlatformModule.CreateExecution)} with {appPlatformData.ToJson()}");
            var appExecutionObj = platform.CreateExecution(platformInfo,platformData, executionLogic);
            if(appExecutionObj == null)
            {
                Logger.Debug("Could not create execution object!");
                return null;
            }

            Logger.Debug(
                $"Starting to subscribe at new Execution with {nameof(appExecutionObj.LogicToExecute.ApplicationGuid)} = {appExecutionObj.LogicToExecute.ApplicationGuid}!");
            appExecutionObj.WhenExecutionPhaseChange.Subscribe(OnExecutionMessage);
            Logger.Trace(
                $"Leaving ExecuteLogic Block of {nameof(appExecutionObj.LogicToExecute.ApplicationGuid)} = {appExecutionObj.LogicToExecute.ApplicationGuid}!");
            return appExecutionObj;
        }
        private void FinalizeUninstall(UninstallProcessData uninstallProcessData)
        {
            try
            {
                Logger.Debug("Finalize Uninstall Action called");
                //We delete the Applications data as the app is in any way not executable anymore
                _appDisplayRepository.Delete(uninstallProcessData.ApplicationGuid);
                _appPlatformRepository.Delete(uninstallProcessData.ApplicationGuid);
                if(_platformDic.TryGetValue(uninstallProcessData.PlatformGuid,out var platform))
                {
                    platform.CleanUpUninstalled(uninstallProcessData);
                }
                else
                {
                    Logger.Warn($"Could not find Platform with Guid={uninstallProcessData.PlatformGuid} for Uninstalled App with Guid={uninstallProcessData.ApplicationGuid}");
                }
                Logger.Debug("Delete Data From Repositories finished, start to remove lock");
            }
            finally
            {
                Logger.Debug("Aquire _installLockedApps lock");
                lock(_installationLock)
                {
                    _installLockedApps.Remove(uninstallProcessData.ApplicationGuid);
                    Logger.Debug($"Lock for Application with Guid={uninstallProcessData.ApplicationGuid} removed");
                }
                //Publish that a new Game is Uninstalled.
                Logger.Debug("Publishing new AppUninstalled Event!");
                _uiMessageBroker.Publish(new UIAppUninstalledEvent(uninstallProcessData));
                
            }
            if(uninstallProcessData.Exception != null)
            {
                //TODO IUIBroker Publish Error Message

                Logger.Error(uninstallProcessData.Exception,"Detected an Exception set for uninstallProcessData");
            }
        }
        private void FinalizeInstall(InstallProcessData installProcessData)
        {
            try
            {
                Logger.Debug(
                    $"Finalize Install Action called for App with Guid={installProcessData.InstallationData.ApplicationGuid}", installProcessData);
                if(installProcessData.Exception == null)
                {
                    //Persist Data when installation was successful
                    _appDisplayRepository.Store(installProcessData.DisplayData);
                    _appPlatformRepository.Store(installProcessData.PlatformData);
                    Logger.Debug($"Installation Data Persistance finished for App with Guid={installProcessData.InstallationData.ApplicationGuid}");

                    //Set Firewall Rules if its Container Installation
                    if(installProcessData.InstallationData.Type == AppInstallationType.Container)
                    {
                        _firewallController.SetFirewallState(installProcessData.PlatformData.ApplicationGuid, FirewallState.NoTrafficAllowed);
                    }
                    //Will set and publish that a new Game is available to UI
                    SetApplicationIsEnabled(installProcessData.PlatformData.ApplicationGuid, true);
                }
                else
                {
                    //TODO IUIBroker Publish Error Message
                }
            }
            finally
            {
                //Publish that a new Game is installed even it might be broken, but UI needs to update the state
                var newInstalledApp = new UIAppInstalledEvent(installProcessData);
                Logger.Debug($"Publishing AppInstalledEvent: {newInstalledApp}");
                _uiMessageBroker.Publish(newInstalledApp);
                lock(_installationLock)
                {
                    _installLockedApps.Remove(installProcessData.InstallationData.ApplicationGuid);
                }
            }
        }
        private void OnExecutionMessage(AppExecutionMessage message) { }
        #endregion
    }
}