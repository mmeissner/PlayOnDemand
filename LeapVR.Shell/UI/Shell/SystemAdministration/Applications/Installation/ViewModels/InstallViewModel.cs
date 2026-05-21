#region Licence
/****************************************************************
 *  Filename: InstallViewModel.cs
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using LeapVR.Shell.Modules.Container;
using LeapVR.Shell.UI.Interfaces;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public class InstallViewModel : Screen
        , IHandle<IUIAppInstalledEvent>
        , IHandle<IUIAppUninstalledEvent>
        , IDisposable
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IUIMessageBroker _messageBroker;
        private readonly IContainerModule _containerModule;
        private readonly IPlatformController _platformController;

        private UsbSticksBarViewModel _usbSticksBar;
        private LoadingStates _loadingState;
        private BindableCollection<Screen> _items;
        private Screen _selectedItem;


        public UsbSticksBarViewModel UsbSticksBar
        {
            get => _usbSticksBar;
            set
            {
                _usbSticksBar = value;
                NotifyOfPropertyChange();
            }
        }

        public LoadingStates LoadingState
        {
            get => _loadingState;
            set
            {
                _loadingState = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<Screen> Items
        {
            get => _items;
            set
            {
                _items = value;
                NotifyOfPropertyChange();
            }
        }

        public Screen SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange(nameof(SelectedItem));
                NotifyOfPropertyChange(nameof(CanInstall));

                if (_selectedItem is InstallationFolderViewModel selectedFolder)
                {
                    RefreshInstallableApplicationsContainer(selectedFolder.Folder);
                }
            }
        }

        public bool CanInstall => SelectedItem is InstallationAppViewModel appViewModel && _platformController.CanInstall(appViewModel.File.AppinstallationContainer) == CanInstallStatus.ReadyToInstall;
        #endregion

        #region Constructors

        public InstallViewModel(
            IContainerModule containerModule,
            IPlatformController platformController,
            IUIMessageBroker messageBroker,
            UsbSticksBarViewModel usbSticksBarViewModel
            )
        {
            QuickLeap.AssertNotNull(containerModule, platformController, messageBroker);
            _containerModule = containerModule;
            _platformController = platformController;

            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);

            _items = new BindableCollection<Screen>();
            UsbSticksBar = usbSticksBarViewModel;
            UsbSticksBar.PropertyChanged += UsbSticksBar_PropertyChanged;

        }

        #endregion

        #region Methods
        public void OnDisplay()
        {
            NotifyOfPropertyChange(() => LoadingState);
        }

        public async void Install()
        {
            if (!(SelectedItem is InstallationAppViewModel selectedApp))
            {
                throw new InvalidOperationException($"{nameof(selectedApp)} == null");
            }
            //TODO: Verify that it does not create Problems, then remove comment
            await Task.Run(()=>selectedApp.Install());
        }

        public void BrowseToInstall()

        {            
            
            var ofd = new OpenFileDialog
                               {
                                       InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                       Filter = $"{Resources.System_Installation_SelectVBoxFile} (*.vbox)|*.vbox"
                               };
            var retval = ofd.ShowDialog();
            if(retval.HasValue && retval.Value)
            {
                //Filters out Shortcuts to URL's and other stuff
                if((new FileInfo(ofd.FileName)).Extension != ".vbox")
                {
                    MessageBox.Show($"{Resources.System_Installation_Install_OnlyVBoxFiles}",Resources.System_Installation_Install, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var selectedFile = ofd.FileName;
                try
                {
                    var container = _containerModule.GetAppInstallationContainer(selectedFile);
                    container.Initialize();
                    var installStatus = _platformController.CanInstall(container);
                    if(installStatus == CanInstallStatus.ReadyToInstall)
                    {
                        _platformController.Install(container);
                    }
                    else
                    {
                        MessageBox.Show($"{Resources.System_Installation_Install_Failed} {installStatus}",Resources.System_Installation_Install, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch(Exception e)
                {
                    Logger.Error(e,$"Failed to install from File= {selectedFile}");
                    MessageBox.Show($"{Resources.System_Installation_Install_Failed} {e.InnerException}",Resources.System_Installation_Install, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Handle(IUIAppInstalledEvent appInstalledEvent)
        {
            NotifyOfPropertyChange(() => CanInstall);
        }
        public void Handle(IUIAppUninstalledEvent appUninstalledEvent)
        {
            NotifyOfPropertyChange(() => CanInstall);
        }

        private void UsbSticksBar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(UsbSticksBarViewModel.SelectedItem):

                    var newSelected = UsbSticksBar.SelectedItem;
                    RefreshInstallableApplicationsContainer(newSelected?.UsbStorageAccess?.RootFolder);
                    break;
            }
        }
        private void RefreshInstallableApplicationsContainer(IFolder<AppInstallationFile> newFolder)
        {
            LoadingState = LoadingStates.Loading;

            Items.Clear();

            if (newFolder == null)
            {
                LoadingState = LoadingStates.Nothing;
                return;
            }

            newFolder.LoadFolder();

            if (newFolder.IsLoaded)
            {
                if (newFolder.Parent != null)
                {
                    Items.Add(new InstallationFolderViewModel(UsbSticksBar.SelectedItem.UsbStorageAccess.RootFolder, FolderType.RootFolder)); // root folder
                    Items.Add(new InstallationFolderViewModel(newFolder.Parent, FolderType.ParentFolder)); // parent folder
                }

                foreach (var folder in newFolder.Folders)
                {
                    Items.Add(new InstallationFolderViewModel(folder));
                }

                foreach (var file in newFolder.Files.OrderBy(item => item.FileName))
                {
                    file.ContainerModule = _containerModule;
                    var appViewModel = new InstallationAppViewModel(_messageBroker,file, _platformController);
                    Items.Add(appViewModel);
                }
            }

            LoadingState = Items.Any() ? LoadingStates.Loaded : LoadingStates.Nothing;
        }


        public void Dispose()
        {
            _messageBroker.Unsubscribe(this);
        }
        #endregion

    }
}
