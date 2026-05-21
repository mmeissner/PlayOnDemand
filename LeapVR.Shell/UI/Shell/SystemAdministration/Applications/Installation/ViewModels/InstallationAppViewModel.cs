#region Licence
/****************************************************************
 *  Filename: InstallationAppViewModel.cs
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
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using LeapVR.Shell.Modules.Container;
using LeapVR.Shell.UI.Universal.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public class InstallationAppViewModel : Screen
        , IHandle<IUIAppUninstalledEvent>
        , IHandle<IUIAppInstalledEvent>
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IPlatformController _platformController;
        private readonly IDisposable _whenFileLoadFinishedSubscription;
        private readonly IUIMessageBroker _messageBroker;

        private string _title;
        private string _desiredSpaceSize;
        private volatile bool _isLoaded;

        private ApplicationThumbnailViewModel _thumbnail;
        private CanInstallStatus _installableStatus;
        private ImageSource _installableIcon;



        public Guid ApplicationGuid { get; private set; }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public string DesiredSpaceSize
        {
            get => _desiredSpaceSize;
            set
            {
                _desiredSpaceSize = value;
                NotifyOfPropertyChange();
            }
        }

        public ApplicationThumbnailViewModel Thumbnail
        {
            get => _thumbnail;
            set
            {
                _thumbnail = value;
                NotifyOfPropertyChange(() => Thumbnail);
            }
        }

        //private ImageSource _thumbnail;
        //public ImageSource Thumbnail
        //{
        //    get => _thumbnail;
        //    set
        //    {
        //        _thumbnail = value;
        //        NotifyOfPropertyChange();
        //    }
        //}

        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                _isLoaded = value;
                NotifyOfPropertyChange();
            }
        }

        public CanInstallStatus InstallableStatus
        {
            get => _installableStatus;
            private set
            {
                _installableStatus = value;
                var resourceKey = $"IconApp{_installableStatus}";
                InstallableIcon = Application.Current.Resources[resourceKey] as ImageSource;
            }
        }

        public ImageSource InstallableIcon
        {
            get => _installableIcon;
            private set
            {
                _installableIcon = value;
                NotifyOfPropertyChange();
            }
        }

        public AppInstallationFile File { get; }
        #endregion

        #region Constructors
        public InstallationAppViewModel(IUIMessageBroker messageBroker,AppInstallationFile file, IPlatformController platformController)
        {
            QuickLeap.AssertNotNull(file, platformController, messageBroker);
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _platformController = platformController;
            File = file;
            _whenFileLoadFinishedSubscription = File.WhenLoadFinished.ObserveOnDispatcher().Subscribe(OnFileLoadFinished);
            Task.Run(() =>
            {
                File.LoadFile();
            });
        }
        #endregion

        #region Methods
        public void Install()
        {
            if (File == null)
            {
                throw new InvalidOperationException($"{nameof(InstallationAppViewModel)}.{nameof(File)} = null");
            }

            if (File.AppinstallationContainer == null)
            {
                throw new InvalidOperationException($"{nameof(InstallationAppViewModel)}.{nameof(File)}.{nameof(File.AppinstallationContainer)} = null");
            }

            if (!_isLoaded)
            {
                throw new InvalidOperationException($"!{File.AppinstallationContainer.DisplayName}.IsLoaded = {IsLoaded}");
            }

            _platformController.Install(File.AppinstallationContainer);
        }

        #region Message Handlers
        public void Handle(IUIAppUninstalledEvent appUninstallEvent)
        {
            if (_isLoaded && appUninstallEvent.ApplicationGuid == ApplicationGuid)
            {
                InstallableStatus = _platformController.CanInstall(ApplicationGuid);
            }
        }

        public void Handle(IUIAppInstalledEvent appInstalledEvent)
        {
            if (_isLoaded && appInstalledEvent.ApplicationGuid == ApplicationGuid)
            {
                InstallableStatus = _platformController.CanInstall(ApplicationGuid);
            }
        }
        #endregion

        private void OnFileLoadFinished(LoadedState state)
        {
            switch (state)
            {
                case LoadedState.Failure:
                    Title = File.FileName;
                    Thumbnail = null;
                    InstallableStatus = CanInstallStatus.ContainerBroken;
                    IsLoaded = true;
                    break;
                case LoadedState.Success:
                    Title = File.AppinstallationContainer.DisplayName;
                    // TODO [RM] the file should provide Image property instead of bytes.
                    // TODO [RM]: Passing empty array (new string[]{}) to VM to ignore tags
                    Thumbnail = new ApplicationThumbnailViewModel(UIHelper.BytesToImageSource(File.AppinstallationContainer.ThumbnailAsBytes), false, false);
                    DesiredSpaceSize = QuickLeap.ToDiskSize(File.AppinstallationContainer.TotalFilesSize);
                    // TODO [FH] file.AppinstallationContainer is always null before the file is loaded.
                    ApplicationGuid = File.AppinstallationContainer.ApplicationGuid;
                    InstallableStatus = _platformController.CanInstall(File.AppinstallationContainer);
                    IsLoaded = true;
                    break;
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _whenFileLoadFinishedSubscription?.Dispose();
        }
        #endregion
    }
}
