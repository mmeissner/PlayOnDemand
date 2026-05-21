#region Licence
/****************************************************************
 *  Filename: Platform.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.Platform.Account;
using LeapVR.Shell.Controllers.Platform.Installation;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Controllers.Platform
{
    //<inheritdoc />
    class Platform : IPlatform
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<Guid, IAppPlatformInfo> _appPlatformInfoCache =
                new ConcurrentDictionary<Guid, IAppPlatformInfo>();

        private readonly IAppDisplayRepository _appDisplayRepository;
        private readonly IDiskController _diskController;
        private readonly PlatformController _platformController;
        private readonly IFirewallController _firewallController;
        private readonly IPlatformModule _platformModule;
        private readonly AccountManager _accountManager;
        private readonly InstallationManager _installationManager;
        private readonly IUIMessageBroker _messageBroker;
        private readonly ICategoryProvider _categoryProvider;


        #region Public Properties
        public Guid PlatformGuid => _platformModule.ModuleId;
        public AccountType SupportedAccountType => _platformModule.SupportedAccountType;
        public InstallationType SupportedInstallationTypes => _platformModule.SupportedInstallationTypes;
        public bool PlatformUninstallSupported => _platformModule.PlatformUninstallSupported;
        public bool IsAvailible => _platformModule.IsAvailable;
        #endregion


        #region Internal Properties
        internal Platform(
                IPlatformModule platformModule,
                IDiskController diskController,
                PlatformController platformController,
                IFirewallController firewallController,
                InstallationManager installationManager,
                IUIMessageBroker uiMessageBroker,
                IAppDisplayRepository appDisplayRepository,
                AccountManager accountManager,
                ICategoryProvider categoryProvider)
        {
            _platformController = platformController;
            _diskController = diskController;
            _firewallController = firewallController;
            _messageBroker = uiMessageBroker;
            _appDisplayRepository = appDisplayRepository;
            _installationManager = installationManager;
            _platformModule = platformModule;
            _accountManager = accountManager;
            _categoryProvider = categoryProvider;
            _accountManager.AddPlatform(this);
            _accountManager.WhenAccountChangeOccures.
                            Where(x => x.PlatformId.Equals(PlatformGuid)).
                            Subscribe(OnAccountChange);
        }
        #endregion


        #region Public App Methods
        public IAppPlatformInfo GetInstalledPlatformApp(Guid applicationId)
        {
            var installationState = _installationManager.GetInstallationState(applicationId);
            if(installationState == InstallationState.Installed)
            {
                return GetFromCacheOrRepo(applicationId);
            }

            return null;
        }

        public IAppPlatformInfo GetPlatformApp(Guid applicationId)
        {
            //Installed Applications
            if(_installationManager.GetInstallationState(applicationId) == InstallationState.Installed)
            {
                //Must be in cache or repo and will be an initialized object
                return GetFromCacheOrRepo(applicationId);
            }

            //Not Installed Applications will on new be returned uninitialized
            if(!_appPlatformInfoCache.TryGetValue(applicationId, out var appPlatformDisplayInfo))
            {
                //Not in Cache then try a GetOrAdd
                appPlatformDisplayInfo = new AppPlatformInfo(
                        applicationId,
                        _installationManager,
                        _accountManager,
                        _firewallController,
                        this,
                        _platformController);
                appPlatformDisplayInfo = _appPlatformInfoCache.GetOrAdd(
                        applicationId,
                        appPlatformDisplayInfo);
            }

            return appPlatformDisplayInfo;
        }

        public void GetPlatformApps(Action<IAppPlatformInfo> callback, Action completed, SynchronizationContext context)
        {
            var whenPlatformDisplayInfoArrived = new Subject<IAppPlatformInfo>();
            var subscription = whenPlatformDisplayInfoArrived.Subscribe(callback, completed);

            //Check if the Platform Module supports Platform Apps or just Container
            if(!_platformModule.SupportedInstallationTypes.HasFlag(InstallationType.Local) &&
               !_platformModule.SupportedInstallationTypes.HasFlag(InstallationType.Online))
            {
                whenPlatformDisplayInfoArrived.OnCompleted();
                subscription.Dispose();
                return;
            }

            Task.Run(() => { CollectPlatformApps(whenPlatformDisplayInfoArrived, subscription); });
        }

        public List<IPlatformAccount> GetPlatformAccounts()
        {
            return _accountManager.GetPlatformAccounts(_platformModule.ModuleId);
        }

        public bool CreatePlatformAccount(
                string username, string password, Guid platformId,
                out IPlatformAccount platformAccount)
        {
            return _accountManager.CreateAccount(platformId, username, password, out platformAccount);
        }

        public bool DeletePlatformAccount(IPlatformAccount platformAccount)
        {
            return _accountManager.DeleteAccount(platformAccount);
        }
        #endregion


        #region Internal License Methods
        /// <summary>
        /// Updates the available licenses for that Platform Account from an online source.
        /// </summary>
        /// <param name="platformAccount">The platform account to update.</param>
        /// <returns></returns>
        internal Task<bool> GetLicensesFromOnlineSource(IPlatformAccount platformAccount)
        {
            return _accountManager.UpdateLicensesOnline(platformAccount);
        }


        /// <summary>
        /// Updates the availible licenses for these Platform Accounts from an online source.
        /// </summary>
        /// <param name="platformAccounts">The platform accounts to update.</param>
        /// <returns></returns>
        internal Task<bool> GetLicensesFromOnlineSource(IEnumerable<IPlatformAccount> platformAccounts)
        {
            return _accountManager.UpdateLicensesOnline(platformAccounts);
        }
        #endregion


        #region Internal Methods        
        /// <summary>
        /// Determines whether Platform Account with an License exists for an Application.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns>
        ///   <c>true</c> if [has platform account] [the specified application identifier]; otherwise, <c>false</c>.
        /// </returns>
        internal bool HasPlatformAccount(Guid applicationId) { return _accountManager.HasAccount(applicationId); }

        /// <summary>
        /// Gets the state of the Platform Installation state for an App.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns></returns>
        internal PlatformInstallState GetPlatformInstallState(Guid applicationGuid)
        {
            Logger.Debug($"Trying to receive Installation State for ApplicationId={applicationGuid}");
            PlatformInstallState retval = PlatformInstallState.Unavailable;
            try
            {
                if(_installationManager.GetInstallationState(applicationGuid) == InstallationState.InstallationInProgress)
                {
                    retval = PlatformInstallState.Installing;
                }
                else if(_platformModule.IsLocalInstalled(applicationGuid))
                {
                    retval = PlatformInstallState.Local;
                }
                else if(_platformModule.SupportedInstallationTypes.HasFlag(AppInstallationType.Platform))
                {
                    if(_accountManager.HasAccount(applicationGuid))
                    {
                        retval = PlatformInstallState.Online;
                    }
                }

                return retval;
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
            finally
            {
                Logger.Debug($"Returning Installation State = {retval} for ApplicationId={applicationGuid}");
            }
        }

        /// <summary>
        /// Gets the AppDisplayInfo from an Online Source for an ApplicationId.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="addImage">if set to <c>true</c> [add image].</param>
        /// <returns></returns>
        internal async Task<IAppDisplayInfo> GetOnlineDisplayInfo(Guid applicationId, bool addImage)
        {
            return await _platformModule.GetOnlineDisplayInfoAsync(applicationId, addImage);
        }

        internal PlatformInstallationProcess Install(
                AppPlatformInfo appPlatformDisplayinfo, Action<InstallProcessData> finalizeAction)
        {
            if(!appPlatformDisplayinfo.CanInstall())
            {
                Logger.Error(
                        $"Cant install an Application Application with Id={appPlatformDisplayinfo.ApplicationGuid}");
                return null;
            }
            Logger.Debug($"Creating new Platform Installation Process for AppId={appPlatformDisplayinfo.PlatformAppId}");
            return new PlatformInstallationProcess(appPlatformDisplayinfo, this, _installationManager, finalizeAction);
        }

        internal bool OnlineInstallation(
                PlatformInstallationProcess platformInstallationProcess, out IAppPlatformData appPlatformData)
        {
            //Get an Account
            IAccountAccess accountAccess = null;
            appPlatformData = null;
            try
            {
                if(!_accountManager.GetAccountAccess(platformInstallationProcess.ApplicationGuid, out accountAccess))
                {
                    Logger.Error(
                            $"Could not get Account for Online Installation of Application={platformInstallationProcess.ApplicationGuid}");
                    return false;
                }

                return _platformModule.OnlineInstallation(
                        platformInstallationProcess.ApplicationGuid,
                        accountAccess,
                        platformInstallationProcess.ReportPlatformInstallation,
                        out appPlatformData);
            }
            finally
            {
                accountAccess?.Release();
            }
        }

        internal bool LocalInstallation(
                PlatformInstallationProcess platformInstallationProcess, out IAppPlatformData appPlatformData)
        {
            appPlatformData = _platformModule.GetLocalInstallation(platformInstallationProcess.ApplicationGuid);
            return appPlatformData != null;
        }

        /// <summary>
        /// Removes an Application from cache.
        /// Needs to be called 
        /// </summary>
        /// <param name="uninstallProcessData"></param>
        /// <returns></returns>
        internal void CleanUpUninstalled(UninstallProcessData uninstallProcessData)
        {
            _appPlatformInfoCache.TryRemove(uninstallProcessData.ApplicationGuid, out _);
        }

        /// <summary>
        /// Creates an execution object to run an application.
        /// </summary>
        /// <param name="appPlatformInfo">The application platform information.</param>
        /// <param name="appPlatformData"></param>
        /// <param name="executionLogic">The Logic to execute.</param>
        /// <returns></returns>
        internal IApplicationExecution CreateExecution(
                IAppPlatformInfo appPlatformInfo,
                IAppPlatformData appPlatformData,
                IProcessExecutionLogic executionLogic)
        {
            //Add an Account to Execution Logic 
            return _platformModule.CreateExecution(appPlatformInfo,appPlatformData, executionLogic);
        }
        #endregion


        #region Private Methods
        private void CollectPlatformApps(
                Subject<IAppPlatformInfo> whenPlatformDisplayInfoArrived, IDisposable subscription)
        {
            try
            {
                //All Applications that have been already handled
                HashSet<Guid> handledApplications = new HashSet<Guid>();

                //Installed Applications that are locally availible
                foreach(IAppInstallationData installationData in _installationManager.GetInstalledByPlatform(
                        PlatformGuid))
                {
                    if(installationData.InstallationState == InstallationState.Installed)
                    {
                        var appPlatformDisplayInfo = GetFromCacheOrRepo(installationData.ApplicationGuid);
                        if(appPlatformDisplayInfo == null)
                        {
                            handledApplications.Add(installationData.ApplicationGuid);
                            Logger.Error(
                                    $"Could not receive AppDisplayInfo from CacheOrRepo for App with Guid={installationData.ApplicationGuid}, desipte its InstallationState");
                            continue;
                        }

                        handledApplications.Add(installationData.ApplicationGuid);
                        whenPlatformDisplayInfoArrived.OnNext(appPlatformDisplayInfo);
                    }
                }

                //Get as second all Applications that are currently known from local installations at the Platform
                foreach(KeyValuePair<Guid, IAppPlatformData> platformData in _platformModule.GetLocalInstallations())
                {
                    if(handledApplications.Contains(platformData.Key)) continue;
                    //Try to get from cache
                    if(!_appPlatformInfoCache.TryGetValue(platformData.Key, out var appPlatformDisplayInfo))
                    {
                        //Not in Cache then try a GetOrAdd
                        appPlatformDisplayInfo = new AppPlatformInfo(
                                platformData.Key,
                                _installationManager,
                                _accountManager,
                                _firewallController,
                                this,
                                _platformController);
                        appPlatformDisplayInfo = _appPlatformInfoCache.GetOrAdd(
                                platformData.Key,
                                appPlatformDisplayInfo);
                    }

                    handledApplications.Add(platformData.Key);
                    whenPlatformDisplayInfoArrived.OnNext(appPlatformDisplayInfo);
                }

                //Online @License/Platform Account
                foreach(Guid guid in _accountManager.GetAppLicensesByPlatform(_platformModule.ModuleId))
                {
                    if(handledApplications.Contains(guid)) continue;
                    if(!_appPlatformInfoCache.TryGetValue(guid, out var appPlatformDisplayInfo))
                    {
                        appPlatformDisplayInfo = new AppPlatformInfo(
                                guid,
                                _installationManager,
                                _accountManager,
                                _firewallController,
                                this,
                                _platformController);
                        appPlatformDisplayInfo = _appPlatformInfoCache.GetOrAdd(
                                guid,
                                appPlatformDisplayInfo);
                    }

                    handledApplications.Add(guid);
                    whenPlatformDisplayInfoArrived.OnNext(appPlatformDisplayInfo);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, $"Exception during Feedback of {nameof(GetPlatformApps)}");
            }
            finally
            {
                whenPlatformDisplayInfoArrived.OnCompleted();
                subscription.Dispose();
            }
        }

        /// <summary>
        /// Gets an AppPlatformInfo for an Installed Application from cache or repo.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns>The IAppPlatformInfo or Null if not found</returns>
        private IAppPlatformInfo GetFromCacheOrRepo(Guid applicationGuid)
        {
            // In Cache
            if(_appPlatformInfoCache.TryGetValue(applicationGuid, out var platformInfo))
            {
                return platformInfo;
            }

            // Not In Cache
            var newPlatformInfo = GetAppPlatformInfoFromRepository(applicationGuid);
            if(newPlatformInfo == null)
            {
                Logger.Warn($"Could not find DisplayInfo with Guid={applicationGuid} for PlatformInfo!");
                return null;
            }
            var requestedApp = _appPlatformInfoCache.GetOrAdd(applicationGuid, newPlatformInfo);
            return requestedApp;
        }

        /// <summary>
        /// Gets the IAppAppPlatformInfo for an Installed App from repository.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns>The displayInfo or Null if not found</returns>
        private AppPlatformInfo GetAppPlatformInfoFromRepository(Guid applicationGuid)
        {
            try
            {
                //var appDisplayInfo = new AppDisplayInfo(applicationGuid, _platformController);
                var appDisplayData = _appDisplayRepository.Get(applicationGuid);
                if(appDisplayData == null)
                {
                    return null;
                }

                byte[] thumbnail = null;
                var pictureFilePath = _diskController.GetFilePath(appDisplayData.MainPicture);
                if(File.Exists(pictureFilePath))
                {
                    thumbnail = File.ReadAllBytes(pictureFilePath);
                }

                return new AppPlatformInfo(
                        appDisplayData,
                        _categoryProvider,
                        thumbnail,
                        _installationManager,
                        _accountManager,
                        _firewallController,
                        this,
                        _platformController);
            }
            catch(Exception exception)
            {
                if(!(exception is RepositoryGetDbException) && !(exception is RepositorySerializationException))
                {
                    Logger.Error(exception, $"{nameof(GetAppPlatformInfoFromRepository)} Error occured!");
                    throw;
                }

                Logger.Error(exception, $"{nameof(GetAppPlatformInfoFromRepository)} Error occured!");
                //TODO: Publish an Error Dialog Message
                //_uiMessageBroker.Publish();
                return null;
            }
        }

        /// <summary>
        /// Called when an Account changes that relates to this platform
        /// </summary>
        /// <param name="accountChangeInfo">The account change information.</param>
        private void OnAccountChange(AccountChangeInfo accountChangeInfo)
        {
            //Check if Remove or Add of Apps affects Availibility of Games and pubish an Event 
            switch(accountChangeInfo.EventType)
            {
                case AccountEventType.AddApps:
                    foreach(AppLicenseInfo info in accountChangeInfo.LicenseInfo)
                    {
                        _messageBroker.Publish(
                                new UIPlatformAccountChanged(
                                        PlatformGuid,
                                        info.ApplicationId,
                                        accountChangeInfo.AccountId,
                                        AccountEventType.AddApps));
                    }

                    break;
                case AccountEventType.RemoveApps:
                    foreach(AppLicenseInfo info in accountChangeInfo.LicenseInfo)
                    {
                        _messageBroker.Publish(
                                new UIPlatformAccountChanged(
                                        PlatformGuid,
                                        info.ApplicationId,
                                        accountChangeInfo.AccountId,
                                        AccountEventType.RemoveApps));
                    }

                    break;
                case AccountEventType.AddAccount:
                    _messageBroker.Publish(
                            new UIPlatformAccountChanged(
                                    PlatformGuid,
                                    null,
                                    accountChangeInfo.AccountId,
                                    AccountEventType.AddAccount));
                    break;
                case AccountEventType.RemoveAccount:
                    _messageBroker.Publish(
                            new UIPlatformAccountChanged(
                                    PlatformGuid,
                                    null,
                                    accountChangeInfo.AccountId,
                                    AccountEventType.RemoveAccount));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion
    }
}