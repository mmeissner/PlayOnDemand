#region Licence
/****************************************************************
 *  Filename: AppPlatformInfo.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.Platform.Account;
using LeapVR.Shell.Controllers.Platform.Installation;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Modules.Interfaces.Platform;
using NLog;

namespace LeapVR.Shell.Controllers.Platform
{
    class AppPlatformInfo : AppDisplayInfo,IAppPlatformInfo
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly InstallationManager _installationManager;
        private readonly IFirewallController _firewallController;
        private readonly AccountManager _accountManager;
        private readonly Platform _platform;
        private volatile bool _displayable;
        private readonly object _updateLock = new object();
        private volatile bool _updating = false;
        private readonly ulong _platformAppId;

        #region Public Properties
        public Guid PlatformGuid => _platform.PlatformGuid;
        public ulong PlatformAppId => _platformAppId;
        public bool IsDisplayable
        {
            get => _displayable;
            private set
            {
                if(value == _displayable) return;
                _displayable = value;
                OnPropertyChanged();
            }
        }
        public bool UpdateInProgress
        {
            get => _updating;
            set
            {
                if(value == _updating) return;
                _updating = value;
                OnPropertyChanged();
            }
        }
        public bool IsLicenseRequired { get; }
        public bool IsEnabled => _platform.IsAvailible && PlatformController.IsApplicationEnabled(ApplicationGuid);

      
        #endregion

        #region Constructor        
        /// <summary>
        /// Initializes a new instance of the <see cref="AppPlatformInfo"/> class.
        /// This Constructor is to be used for NOT installed apps and will be Uninitialized 
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <param name="installationManager">The installation manager.</param>
        /// <param name="accountManager">The account manager.</param>
        /// <param name="firewallController">The Firewall Controller</param>
        /// <param name="platform">The platform module.</param>
        /// <param name="platformController"></param>
        public AppPlatformInfo(
                Guid applicationGuid,
                InstallationManager installationManager,
                AccountManager accountManager,
                IFirewallController firewallController,
                Platform platform,
                PlatformController platformController):base(applicationGuid,platformController)
        {
            _installationManager = installationManager;
            _accountManager = accountManager;
            _platform = platform;
            _firewallController = firewallController;
            PlatformConverter.GetPlatformId(applicationGuid, out _, out _platformAppId);
            IsLicenseRequired = !_platform.SupportedAccountType.HasFlag(AccountType.None);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPlatformInfo"/> class.
        /// This Constructor is to be used for installed apps and will have an Initialized State
        /// </summary>
        /// <param name="appDisplayInfo">The application display information.</param>
        /// <param name="thumbnail"></param>
        /// <param name="installationManager">The installation manager.</param>
        /// <param name="accountManager">The account manager.</param>
        /// <param name="firewallController">The Firewall Controller</param>
        /// <param name="platform">The platform module.</param>
        /// <param name="platformController">The Platform Controler</param>
        /// <param name="displayData"></param>
        /// <param name="categoryProvider"></param>
        public AppPlatformInfo(
                IAppDisplayData displayData,
                ICategoryProvider categoryProvider,
                byte[] thumbnail,
                InstallationManager installationManager,
                AccountManager accountManager,
                IFirewallController firewallController,
                Platform platform, PlatformController platformController):base(displayData,categoryProvider,thumbnail,platformController)
        {
            _installationManager = installationManager;
            _accountManager = accountManager;
            _platform = platform;
            _firewallController = firewallController;
            PlatformConverter.GetPlatformId(ApplicationGuid, out _, out _platformAppId);
            IsLicenseRequired = !_platform.SupportedAccountType.HasFlag(AccountType.None);
            IsDisplayable = true;
        }
        #endregion

        #region Public Methods
        public async Task<bool> GetOrUpdateDisplayDataAsync()
        {
            try
            {
                //Only One Update per Time
                if(_updating)
                {
                    Logger.Warn($"Update already in progress for AppId={ApplicationGuid}, returning false");
                    return false;
                }
                lock(_updateLock)
                {
                    if(_updating)
                    {
                        Logger.Warn($"Update already in progress for AppId={ApplicationGuid}, returning false");
                        return false;
                    }
                    UpdateInProgress = true;
                    Logger.Info($"Aquired Update Log for AppId={ApplicationGuid} starting to update");
                }

                try
                {
                    bool addImage = Thumbnail == null;
                    //TODO: Consider another Interface then DisplayInfo to avoid confusion
                    var displayInfo = await _platform.GetOnlineDisplayInfo(ApplicationGuid, addImage);
                    Logger.Info($"Received Update Log for AppId={ApplicationGuid} ValueIsNull={displayInfo==null}");
                    if(displayInfo != null)
                    {
                        UpdateValues(displayInfo);
                    }
                    return true;
                }
                finally
                {
                    UpdateInProgress = false;
                    Logger.Info($"Update Log for AppId={ApplicationGuid} finished");
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, $"Error during {nameof(GetOrUpdateDisplayDataAsync)}");
                return false;
            }
        }

        public void SetEnabled(bool enabled) { PlatformController.SetApplicationIsEnabled(ApplicationGuid, enabled); }
        
        public PlatformInstallState ClientInstallState() =>_platform.GetPlatformInstallState(ApplicationGuid);

        public InstallationState SystemInstallState() =>_installationManager.GetInstallationState(ApplicationGuid);

        public IAppLicenseInfo LicenseInfo() => _accountManager.GetLicenseInfo(ApplicationGuid);

        public bool CanInstall()
        {
            //App must be initialized
            if(!_displayable) return false;
            var systemInstallState = SystemInstallState();
            var platformInstallState = ClientInstallState();

            //State must be not Installed
            if(systemInstallState != InstallationState.NotInstalled) return false;

            //PlatformState can not be Uninstalled
            if(platformInstallState == PlatformInstallState.Unavailable) return false;

            //If App needs License and their install state is not Local then
            //we need to ensure one license is availible for install
            if(platformInstallState!= PlatformInstallState.Local && IsLicenseRequired && LicenseInfo().CurrentLicenseCount <= 0) return false;

            //If not PlatformState is Local or Online Availible we can install
            if(platformInstallState == PlatformInstallState.Local ||
               platformInstallState == PlatformInstallState.Online) return true;
            return false;
        }

        public bool CanUninstall()
        {
            //App must be initialized
            if(!_displayable) return false;

            //State must be Installed
            if(SystemInstallState() != InstallationState.Installed) return false;

            return true;
        }

        public bool CanPlatformUninstall() { return CanUninstall() && _platform.PlatformUninstallSupported; }

        public void Install()
        {
            PlatformController.Install(this);
        }

        public void Uninstall(bool tryFullUninstall)
        {
            PlatformController.Uninstall(this,tryFullUninstall);
        }

        public bool HasPlatformAccount() => _platform.HasPlatformAccount(ApplicationGuid);

        public bool TryGetInstallationInfo(out IAppInstallationInfo installationInfo)
        {
            installationInfo = PlatformController.GetApplicationInstallationData(ApplicationGuid);
            if(installationInfo != null) return true;
            return false;
        }

        public bool TryGetAccountAccessForApp(out IPlatformAccount account, out IAccountAccess accountAccess)
        {
            return _accountManager.TryGetAccountAccessForApp(ApplicationGuid, out account, out accountAccess);
        }

        public bool AddLicense(IPlatformAccount account)
        {
            var retval = _accountManager.AddApplicationId(account, ApplicationGuid);
            if(retval) OnPlatformAppUpdated(this, PlatformAppUpdate.Licensing);
            return retval;
        }

        public bool RemoveLicense(IPlatformAccount account)
        {
            var retval = _accountManager.RemoveApplicationId(account, ApplicationGuid);
            if(retval)OnPlatformAppUpdated(this, PlatformAppUpdate.Licensing);
            return retval;
        }
        
        public async Task<FirewallState> GetFirewallStateAsync()
        {
            return await _firewallController.GetFirewallStateAsync(ApplicationGuid);
        }

        public void SetFirewallState(FirewallState firewallState)
        {
            _firewallController.SetFirewallState(ApplicationGuid,firewallState);
        }

        public bool TryGetAppExecutableUpdate(out IAppExecutablesUpdate update)
        {
            return PlatformController.TryGetAppExecutableUpdate(ApplicationGuid, out update);
        }

        public event PlatformAppUpdatedEventHandler PlatformAppUpdated;

        internal void OnStateChanged(PlatformAppUpdate type)
        {
            OnPlatformAppUpdated(this,type);
        }
        #endregion

        #region Private Methods
        private void UpdateValues(IAppDisplayInfo appDisplayInfo)
        {
            Name = appDisplayInfo.Name;
            Thumbnail = appDisplayInfo.Thumbnail;
            Category = appDisplayInfo.Category;
            Description = appDisplayInfo.Description;
            IsDisplayable = true;
        }

        private void OnPlatformAppUpdated(IAppPlatformInfo platforminfo, PlatformAppUpdate updatetype)
        {
            PlatformAppUpdated?.Invoke(platforminfo, updatetype);
        }
        #endregion
    }
}