#region Licence
/****************************************************************
 *  Filename: UninstallViewModel.cs
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
using System.Linq;
using System.Reactive.Linq;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public class UninstallViewModel : Screen,
                                      IHandle<IUIAppInstalledEvent>,
                                      IHandle<IUIAppUninstalledEvent>,
                                      IDisposable
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IPlatformController _platformController;
        private readonly IDiskController _diskController;
        private readonly IUIMessageBroker _messageBroker;
        private LoadingStates _itemsLoadingState;
        private bool _isFreeSpaceReady;
        private string _freeSpace;
        private UninstallationAppViewModel _selectedItem;
        private BindableCollection<UninstallationAppViewModel> _items = new BindableCollection<UninstallationAppViewModel>();
        public LoadingStates ItemsLoadingState
        {
            get => _itemsLoadingState;
            set
            {
                _itemsLoadingState = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsFreeSpaceReady
        {
            get => _isFreeSpaceReady;
            set
            {
                _isFreeSpaceReady = value;
                NotifyOfPropertyChange();
            }
        }
        public string FreeSpace
        {
            get => _freeSpace;
            set
            {
                _freeSpace = value;
                NotifyOfPropertyChange();
            }
        }
        public BindableCollection<UninstallationAppViewModel> Items
        {
            get { return _items; }
            set
            {
                if(Equals(value, _items)) return;
                _items = value;
                NotifyOfPropertyChange();
            }
        }
        public UninstallationAppViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange(nameof(SelectedItem));
                NotifyOfPropertyChange(nameof(CanUninstall));
            }
        }
        public bool CanUninstall => SelectedItem != null &&
                                    new[] {CanUninstallStatus.ReadyToUninstall, CanUninstallStatus.BrokenCanUninstall}.
                                            Contains(SelectedItem.UninstallableStatus);
        #endregion

        #region Constructors
        public UninstallViewModel(
            IDiskController diskController,
            IPlatformController platformController,
            IUIMessageBroker messageBroker)
        {
            QuickLeap.AssertNotNull(diskController, platformController, messageBroker);

            _diskController = diskController;
            _diskController.WhenDiskUsageChanged.ObserveOnDispatcher().
                            Subscribe(UpdateFreeDiskSpace); // TODo [RM]: keep IDisposable?

            _platformController = platformController;

            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            InitializeItems();
        }
        #endregion

        #region Methods
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            UpdateFreeDiskSpace(_diskController.DiskUsage);
            IsFreeSpaceReady = true;
            UpdateLoadedState();
        }
        public void OnDisplay()
        {
            UpdateFreeDiskSpace(_diskController.DiskUsage);
            CheckForUpdate();
            UpdateLoadedState();
            IsFreeSpaceReady = true;
        }

        public void Uninstall()
        {
            if(SelectedItem == null)
            {
                throw new InvalidOperationException($"{nameof(SelectedItem)} == null");
            }
            _platformController.Uninstall(_selectedItem.ApplicationGuid,_selectedItem.InstallationData.PlatformPluginGuid,false);
        }

        private void UpdateInstalledApplications(UpdateEvent updateEvent)
        {
            switch(updateEvent.Type)
            {
                case UpdateEvent.UpdateType.Installed:
                    var newModel = GetNewModel(updateEvent.ApplicationGuid);
                    if(newModel != null)
                    {
                        Logger.Info($"Adding UnInstallation Model for App with Guid={updateEvent.ApplicationGuid}");
                        Items.Add(newModel);
                    }
                    else
                    {
                        Logger.Warn(
                            $"Could not create UnInstallation Model for App with Guid={updateEvent.ApplicationGuid}");
                    }
                    break;
                case UpdateEvent.UpdateType.Uninstalled:
                    var itemToRemove = Items.FirstOrDefault(x => x.ApplicationGuid.Equals(updateEvent.ApplicationGuid));
                    if(itemToRemove == null)
                    {
                        Logger.Warn(
                            $"Could not find App with Id={updateEvent.ApplicationGuid} to remove in collection");
                    }
                    else
                    {
                        //Check if it might not be broken
                        var uninstallableStatus = _platformController.CanUninstall(updateEvent.ApplicationGuid);
                        if(uninstallableStatus == CanUninstallStatus.NotInstalled)
                        {
                            Items.Remove(itemToRemove);
                        }
                        else
                        {
                            itemToRemove.UpdateUninstallableState(uninstallableStatus);
                        }
                    }
                    break;
            }
        }

        private void UpdateFreeDiskSpace(IWholeDiskUsage diskUsage)
        {
            FreeSpace = QuickLeap.ToDiskSize(
                diskUsage.TotalDiskSpace -
                diskUsage.SystemUsedDiskUsage -
                diskUsage.ContentUsedDiskSpace.Sum(kv => kv.Value));
        }

        public void Handle(IUIAppInstalledEvent appInstalledEvent)
        {
            if(appInstalledEvent.Type == AppInstallationType.Container)
            {
                RefreshUninstallableApplicationsContainer(UpdateEvent.FromInstallEvent(appInstalledEvent));
            }
        }

        public void Handle(IUIAppUninstalledEvent appUninstalledEvent)
        {
            if(appUninstalledEvent.Type == AppInstallationType.Container)
            {
                RefreshUninstallableApplicationsContainer(UpdateEvent.FromUninstallEvent(appUninstalledEvent));
            }
        }

        private void RefreshUninstallableApplicationsContainer(UpdateEvent updateEvent)
        {
            ItemsLoadingState = LoadingStates.Loading;
            UpdateInstalledApplications(updateEvent);
            ItemsLoadingState = Items.Any() ? LoadingStates.Loaded : LoadingStates.Nothing;
            if(Items.Count > 0) SelectedItem = Items.First();
        }

        private void InitializeItems()
        {

            var installedGames = _platformController.GetApplicationInstallationData(AppInstallationType.Container).ToArray();
            var items = new List<UninstallationAppViewModel>();
            if(installedGames.Any())
            {
                var orderedGames =
                        installedGames.OrderByDescending(appInstallationData => appInstallationData.InstallationDate);
                foreach(var installationData in orderedGames)
                {
                    var appPlatformInfo = _platformController.GetInstalledApplication(installationData.ApplicationGuid,installationData.PlatformPluginGuid);
                    var uninstallState = _platformController.CanUninstall(installationData.ApplicationGuid);
                    if (appPlatformInfo == null)
                    {
                        Logger.Warn(
                            $"Could not Load AppDisplayInfo for Installed Game with ApplicationGuid={installationData.ApplicationGuid}");
                        items.Add(
                            new UninstallationAppViewModel(_messageBroker, installationData, uninstallState));
                    }
                    else
                    {
                        items.Add(
                            new UninstallationAppViewModel(
                                _messageBroker,
                                appPlatformInfo,
                                installationData,
                                 uninstallState));
                    }
                }
            }
            //Add New to Observable Collection
            Items.AddRange(items);
        }

        private void CheckForUpdate()
        {
            var dictItems = new Dictionary<Guid, UninstallationAppViewModel>();
            foreach(var item in Items)
            {
                dictItems.Add(item.ApplicationGuid,item);
            }
            var itemsToRemove = new List<UninstallationAppViewModel>(Items);
            var installedGames = _platformController.GetApplicationInstallationData(AppInstallationType.Container).ToArray();
            foreach(IAppInstallationData game in installedGames)
            {
                //Game already exists
                if(dictItems.ContainsKey(game.ApplicationGuid))
                {
                    //Check if Update needed
                    if(dictItems[game.ApplicationGuid].InstallationData.InstallationState != game.InstallationState)
                    {
                        //Remove the Old and Add the New
                        var newModel = GetNewModel(game.ApplicationGuid);
                        Items.Remove(dictItems[game.ApplicationGuid]);
                        Items.Add(newModel);
                    }
                    itemsToRemove.Remove(dictItems[game.ApplicationGuid]);
                }
                //Game is new
                else
                {
                    var newModel = GetNewModel(game.ApplicationGuid);
                    Items.Add(newModel);
                }
            }
            foreach(UninstallationAppViewModel model in itemsToRemove)
            {
                Items.Remove(model);
            }
        }
        private void UpdateLoadedState()
        {
            ItemsLoadingState = Items.Any() ? LoadingStates.Loaded : LoadingStates.Nothing;
            if (Items.Count > 0) SelectedItem = Items.First();
        }
        private UninstallationAppViewModel GetNewModel(Guid id)
        {
            var installationData = _platformController.GetApplicationInstallationData(id);
            var appDisplayInfo = _platformController.GetInstalledApplication(installationData.ApplicationGuid,installationData.PlatformPluginGuid);
            var uninstallState = _platformController.CanUninstall(installationData.ApplicationGuid);
            if (appDisplayInfo == null)
            {
                Logger.Warn(
                    $"Could not Load AppDisplayInfo for Installed Game with ApplicationGuid={installationData.ApplicationGuid}");
                return new UninstallationAppViewModel(_messageBroker, installationData, uninstallState);
            }
            return new UninstallationAppViewModel(
                _messageBroker,
                appDisplayInfo,
                installationData, uninstallState);
        }
        public void Dispose() { _messageBroker.Unsubscribe(this); }
        #endregion
    }
}