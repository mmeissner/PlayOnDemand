#region Licence
/****************************************************************
 *  Filename: SteamPlatformModule.cs
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
using System.Linq;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Modules.Interfaces.Platform.Steam;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using LeapVR.Utilities.Steam.Steam;
using NLog;

namespace LeapVR.Shell.Modules.Platform.Steam
{
    public class SteamPlatformModule : IPlatformModule
    {
        #region Properties & Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        internal static readonly Guid PluginGuid = Guid.Parse("DCF369A1-9969-4472-8485-2A5B566CC9CF");
        private readonly IUIMessageBroker _messageBroker;
        private readonly SteamPlatformProvider _provider;
        private readonly SteamLib _steamLib;
        private readonly ICategoryProvider _categoryProvider;
        private readonly IGenericRepository<CacheableSteamStoreInfo> _storeCache;
        private readonly object _currentExecutionLock = new object();
        private IApplicationExecution _currentExecution;

        public const string GuidPlatformNamePrefix = "STEAM___";
        public Guid ModuleId => PluginGuid;
        public bool IsAvailable => SteamLib.IsAvailable;
        public InstallationType SupportedInstallationTypes => InstallationType.Local | InstallationType.Online;
        public AccountType SupportedAccountType => AccountType.Manually | AccountType.Automatic;
        public bool RequiresAccount => true;
        public bool PlatformUninstallSupported => false;
        public string PlatformNameId { get; } = GuidPlatformNamePrefix;
        public string ModuleName => "Steam Platform Module";
        #endregion Properties & Fields

        #region Constructors
        public SteamPlatformModule(
                SteamLib steamLib,
                ICategoryProvider categoryProvider,
                IGenericCacheProvider genericCacheProvider,
                IUIMessageBroker messageBroker
        )
        {
            QuickLeap.AssertNotNull(messageBroker, steamLib, categoryProvider);
            _messageBroker = messageBroker;
            _categoryProvider = categoryProvider;
            _steamLib = steamLib;
            _provider = new SteamPlatformProvider(this,_steamLib);
            var moduleCache = genericCacheProvider.GetModuleCache(ModuleId, "Steam");
            _storeCache = moduleCache.GetModuleRepository<CacheableSteamStoreInfo>();
        }
        #endregion Constructors

        public bool IsApplicationAvailable(Guid appId) { return _provider.IsApplicationAvailable(appId); }
        public bool HasCache => true;

        public bool IsLocalInstalled(Guid applicationId)
        {
            return _provider.GetLocalApplications().ContainsKey(applicationId);
        }

        /// <summary>
        /// Called to install an Application from an Online Source.
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="accountAccess">The account access providing Username and Password.</param>
        /// <param name="progressReportCallBack">The Call back to report the current installation state.</param>
        /// <param name="installedApp"></param>
        /// <returns>The IAppPlatformData of the Installed application, null in case of failure</returns>
        public bool OnlineInstallation(
                Guid applicationId, IAccountAccess accountAccess, Action<PlatformInstallationPhase> progressReportCallBack,
                out IAppPlatformData installedApp)
        {
            //Install App

            //Refresh Apps and Return IAppPlatformData
            throw new NotImplementedException("This feature is not yet implemented");
        }

        public async Task<IAppDisplayInfo> GetOnlineDisplayInfoAsync(Guid applicationId, bool addImage)
        {
            Logger.Debug($"Trying to get DisplayInfo for AppId={applicationId}, with addImage= {addImage}");
            PlatformConverter.GetPlatformId(applicationId, out var nameId, out var appId);
            if(!nameId.Equals(PlatformNameId)) return null;

            Logger.Debug($"Starting Store Requrest for AppId={applicationId}");

            //Check Cache
            ISteamAppStoreInfo storeInfo =_storeCache.Get(applicationId);
            if(storeInfo == null)
            {
                //Receive from Online Source
                storeInfo = await _steamLib.GetAppDetailsAsync(Convert.ToUInt32(appId),addImage);

                //Cache only if data seems valid
                if(storeInfo != null &&
                   !String.IsNullOrEmpty(storeInfo.Title) &&
                   storeInfo.Image != null &&
                   storeInfo.Image.Any())
                {
                    storeInfo = new CacheableSteamStoreInfo(applicationId, storeInfo);
                    _storeCache.Store(new CacheableSteamStoreInfo(applicationId, storeInfo));
                }
            }
            Logger.Debug($"Store Request for AppId={applicationId} finished!");
            // The fetch can come back null when Steam returns 200 with
            // {"<appid>":{"success":false}} - typical for delisted, region-
            // locked, or family-share-only titles. Convert it into a typed
            // "no display data" outcome instead of NREing inside ToAppDisplayInfo.
            if (storeInfo == null)
            {
                Logger.Info($"No Steam store data available for AppId={applicationId} (delisted / region-locked / unauthenticated query)");
                return null;
            }
            return ToAppDisplayInfo(applicationId, storeInfo, _categoryProvider);
        }

        /// <summary>
        /// Gets all the ApplicationIds of Licensed Apps for a specific account
        /// These can come from an online source.
        /// </summary>
        /// <param name="platformAccount">The platform account.</param>
        /// <returns>ApplicationIds of all licensend Applications by this account</returns>
        public Task<HashSet<Guid>> GetApplicationsFromAccountAsync(IPlatformAccount platformAccount)
        {
            throw new NotImplementedException("This feature is not yet implemented");
        }

        /// <summary>
        /// Gets the all apps that are installed locally on that platform.
        /// </summary>
        /// <returns>
        /// Dictionary with ApplicationId and PlatformData
        /// </returns>
        public Dictionary<Guid, IAppPlatformData> GetLocalInstallations()
        {
            return _provider.GetLocalApplications();
        }

        public IAppPlatformData GetLocalInstallation(Guid applicationId)
        {
            return _provider.GetLocalApplication(applicationId);
        }

        public void ClearCache()
        {
            _storeCache.DeleteAll();
        }

        #region Application Execution Methods
        public IApplicationExecution CreateExecution(IAppPlatformInfo appPlatformInfo,IAppPlatformData appPlatformData, IProcessExecutionLogic executionLogic)
        {
            QuickLeap.AssertNotNull(executionLogic);

            lock(_currentExecutionLock)
            {
                if(_currentExecution != null)
                {
                    Logger.Debug(
                            $"Failed to {nameof(CreateExecution)}. Already other execution in progress. Will return null of type {nameof(IApplicationExecution)}.");
                    return null;
                }

                var applicationExecution = new SteamApplicationExecution(
                        appPlatformInfo,
                        appPlatformData,
                        executionLogic,
                        _steamLib,
                        _messageBroker);
                applicationExecution.WhenExecutionPhaseChange.Subscribe(OnCurrentExecutionPhaseChanged);
                return applicationExecution;
            }
        }
        private void OnCurrentExecutionPhaseChanged(AppExecutionMessage executionMessage)
        {
            switch(executionMessage.Phase)
            {
                case ExecutionPhase.NotStarted:
                case ExecutionPhase.BeforeStart:
                    break;
                case ExecutionPhase.OnPlatformStart:
                    lock(_currentExecutionLock)
                    {
                        _currentExecution = executionMessage.AppExecutionData;
                        _currentExecution.Run();
                    }

                    break;
                case ExecutionPhase.AfterStart:
                case ExecutionPhase.BeforeExit:
                    break;
                case ExecutionPhase.OnPlatformEnd:
                    lock(_currentExecutionLock)
                    {
                        _currentExecution = null;
                    }

                    break;
                case ExecutionPhase.AfterExit:
                case ExecutionPhase.OnFinished:
                    break;
            }
        }
        #endregion Methods

        #region Private Static Methods
        private static IAppDisplayInfo ToAppDisplayInfo(Guid applicationId,
                ISteamAppStoreInfo storeInfo,
                ICategoryProvider categoryProvider)
        {
            Logger.Debug("Starting to Convert StoreInfo to AppDisplayInfo");
            var categories = categoryProvider.GetAllCategories;
            var catIdentifiers = new HashSet<string>();
            bool categoryMatch = false;
            string catMatchIdentifier = "casual";
            foreach(var item in categories)
            {
                catIdentifiers.Add(item.Identifier);
            }

            foreach(var identifier in catIdentifiers)
            {
                foreach(var steamCat in storeInfo.Categories)
                {
                    if(identifier.Contains(steamCat))
                    {
                        catMatchIdentifier = identifier;
                        categoryMatch = true;
                    }

                    if(categoryMatch) break;
                }

                if(categoryMatch) break;
            }

            var category = categoryProvider.GetOrCreateAppCategory(catMatchIdentifier);
            var retval = new SteamAppDisplayInfo(
                    applicationId,
                    storeInfo.Title,
                    category,
                    storeInfo.Description,
                    storeInfo.Image,false,false);
            Logger.Debug($"Convert finished for App with Id={applicationId}");
            return retval;
        }
        #endregion
    }
}