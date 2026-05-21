#region Licence
/****************************************************************
 *  Filename: USBSticksBarViewModel.cs
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
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using LeapVR.Shell.Modules.Container;
using LeapVR.Shell.UI.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public class UsbSticksBarViewModel : Screen
    {
        #region Fields & Properties
        private LoadingStates _loadingState;
        public LoadingStates LoadingState
        {
            get => _loadingState;
            set
            {
                _loadingState = value;
                NotifyOfPropertyChange();
            }
        }

        private BindableCollection<UsbStickItemViewModel> _items;
        public BindableCollection<UsbStickItemViewModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                NotifyOfPropertyChange();
            }
        }

        private UsbStickItemViewModel _selectedItem;
        public UsbStickItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        private Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        public UsbSticksBarViewModel(IUsbDevicesManager usbDeviceManager)
        {
            QuickLeap.AssertNotNull(usbDeviceManager);

            LoadingState = LoadingStates.Nothing;
            _items = new BindableCollection<UsbStickItemViewModel>();
            InitializeDrives(usbDeviceManager.UsbDrives);
            usbDeviceManager.UsbDrives.CollectionChanged += UsbDrives_CollectionChanged;
        }

        #endregion

        #region Methods

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            NotifyOfPropertyChange(() => LoadingState);
            NotifyOfPropertyChange(() => SelectedItem);
        }

        private void InitializeDrives(IEnumerable<IUsbStorage> initDrives)
        {
            foreach (var usbDrive in initDrives)
            {
                try
                {
                    AddUsbStorage(usbDrive);
                }
                catch (Exception e)
                {
                    _logger.Error($"In InitializeDrives while adding usbDrive (usbDrive == null: `{usbDrive == null}` usbDrive?.DriveInfo == null: `{usbDrive?.DriveInfo == null}`, DriveInfo.Name = `{usbDrive?.DriveInfo?.Name}`, IsReady = `{usbDrive?.DriveInfo?.IsReady}`, TotalSize = `{usbDrive?.DriveInfo?.TotalSize}`) Exception occured: `{e}`.");
                }
            }
            if (Items.Any() && LoadingState != LoadingStates.Loaded)
            {
                LoadingState = LoadingStates.Loaded;
            }
            else if (LoadingState != LoadingStates.Nothing)
            {
                LoadingState = LoadingStates.Nothing;
            }
        }

        private void UsbDrives_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadingState = LoadingStates.Loading;

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    AddUsbStorage((IUsbStorage)newItem);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    RemoveUsbStorages((IUsbStorage)oldItem);
                }
            }

            if (Items.Any() && LoadingState != LoadingStates.Loaded)
            {
                LoadingState = LoadingStates.Loaded;
            }
            else if (LoadingState != LoadingStates.Nothing)
            {
                LoadingState = LoadingStates.Nothing;
            }
        }

        private void AddUsbStorage(IUsbStorage usbStorage)
        {
            var newAccess = usbStorage.GetStorageAccess<AppInstallationFile>();
            Items.Add(new UsbStickItemViewModel(newAccess));

            SelectFirstIfNull();
        }

        private void RemoveUsbStorages(IUsbStorage usbStorage)
        {
            var drivesToRemove = Items.Where(q => q.UsbStorageAccess.DriveInfo.Name == usbStorage.DriveInfo.Name).ToList();
            Items.RemoveRange(drivesToRemove);

            SelectFirstIfNull();
        }

        private void SelectFirstIfNull()
        {
            if (Items.Any() && SelectedItem == null)
            {
                SelectedItem = Items.First();
            }
        }

        #endregion
    }
}
