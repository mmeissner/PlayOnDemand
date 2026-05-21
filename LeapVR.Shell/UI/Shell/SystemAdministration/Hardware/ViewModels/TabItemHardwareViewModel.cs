#region Licence
/****************************************************************
 *  Filename: TabItemHardwareViewModel.cs
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
using System.Linq;
using System.Reactive.Linq;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;
using LeapVR.Shell.UI.Universal.StationDetails.ViewModels;
using LeapVR.Shell.UI.Universal.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Hardware.ViewModels
{
    public sealed class TabItemHardwareViewModel : TabItemSystemScreen
    {
        #region Fields & Properties
        private readonly IDisposable _whenDiskUsageChangedSubscription;
        private StationDetailsViewModel _stationDetailsViewModel;
        private string _cpuDescription;
        private string _ramDescription;
        private string _vgaDescription;
        private string _hardwareId;
        private BindableCollection<NotificationKeyValuePair<string, long>> _items;
        private SpaceSeriesViewModel _freeSpaceInfo;
        private SpaceSeriesViewModel _applicationSpaceInfo;
        private SpaceSeriesViewModel _systemSpaceInfo;

        public override string DisplayName
        {
            get { return Resources.System_Hardware; }
            set { /* ignore */ }
        }

        public StationDetailsViewModel StationDetailsViewModel
        {
            get => _stationDetailsViewModel;
            set
            {
                _stationDetailsViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public string CpuDescription
        {
            get => _cpuDescription;
            set
            {
                _cpuDescription = value;
                NotifyOfPropertyChange();
            }
        }

        public string RamDescription
        {
            get => _ramDescription;
            set
            {
                _ramDescription = value;
                NotifyOfPropertyChange();
            }
        }

        public string VgaDescription
        {
            get => _vgaDescription;
            set
            {
                _vgaDescription = value;
                NotifyOfPropertyChange();
            }
        }

        public string HardwareId
        {
            get => _hardwareId;
            set
            {
                _hardwareId = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<NotificationKeyValuePair<string, long>> Items
        {
            get => _items;
            set
            {
                _items = value;
                NotifyOfPropertyChange();
            }
        }

        public SpaceSeriesViewModel FreeSpaceInfo
        {
            get => _freeSpaceInfo;
            set
            {
                _freeSpaceInfo = value;
                NotifyOfPropertyChange();
            }
        }

        public SpaceSeriesViewModel ApplicationSpaceInfo
        {
            get => _applicationSpaceInfo;
            set
            {
                _applicationSpaceInfo = value;
                NotifyOfPropertyChange();
            }
        }

        public SpaceSeriesViewModel SystemSpaceInfo
        {
            get => _systemSpaceInfo;
            set
            {
                _systemSpaceInfo = value;
                NotifyOfPropertyChange();
            }
        }
        public long TotalSpace
        {
            get
            {
                var result = SystemSpaceInfo?.Value + FreeSpaceInfo?.Value + ApplicationSpaceInfo?.Value;
                if (result != null)
                    return (long)result;
                return long.MinValue;
            }
        }

        public override int DisplayOrder => 9;
        #endregion

        #region Constructors

        public TabItemHardwareViewModel(
            IUIMessageBroker messageBroker,
            IDiskController diskController,
            ILocalMachine localMachine,
            StationDetailsViewModel stationDetailsViewModel) : base(messageBroker,"IconHardware")
        {
            QuickLeap.AssertNotNull(diskController, localMachine, stationDetailsViewModel);           
            CpuDescription = localMachine.CpuDetails;
            RamDescription = localMachine.RamDetails;
            VgaDescription = localMachine.VgaDetails;
            HardwareId = localMachine.VBoxFingerprint;

            StationDetailsViewModel = stationDetailsViewModel;
            SystemSpaceInfo = new SpaceSeriesViewModel(Resources.System_Hardware_SystemFiles);
            FreeSpaceInfo = new SpaceSeriesViewModel(Resources.System_Hardware_FreeSpace);
            ApplicationSpaceInfo = new SpaceSeriesViewModel(Resources.System_Hardware_Applications);
            Items = new BindableCollection<NotificationKeyValuePair<string, long>>();
            _whenDiskUsageChangedSubscription = diskController.WhenDiskUsageChanged.ObserveOnDispatcher().Subscribe(OnDiskUsageChanged); // TODO [RM]: keep disposable?
            OnDiskUsageChanged(diskController.DiskUsage);
            UpdateChartNames();
        }
        #endregion
        
        #region Methods
        private void OnDiskUsageChanged(IWholeDiskUsage diskUsage)
        {
            if (new[] { SystemSpaceInfo, FreeSpaceInfo, ApplicationSpaceInfo }.Any(x => x == null)) return;

            var totalSpace = diskUsage.TotalDiskSpace;
            var appUsedSpace = diskUsage.ContentUsedDiskSpace.Sum(q => q.Value);
            var systemUsedSpace = diskUsage.SystemUsedDiskUsage;
            var freeSpace = totalSpace - appUsedSpace - systemUsedSpace;

            SystemSpaceInfo.Value = systemUsedSpace;
            FreeSpaceInfo.Value = freeSpace;
            ApplicationSpaceInfo.Value = appUsedSpace;
            UpdateChartNames();
            NotifyOfPropertyChange(nameof(TotalSpace));
        }
        #endregion

        private void UpdateChartNames()
        {
            SystemSpaceInfo.Title = $"{Resources.System_Hardware_SystemFiles} {QuickLeap.ToDiskSize((ulong)SystemSpaceInfo.Value)}";
            FreeSpaceInfo.Title = $"{Resources.System_Hardware_FreeSpace} {QuickLeap.ToDiskSize((ulong)FreeSpaceInfo.Value)}";
            ApplicationSpaceInfo.Title = $"{Resources.System_Hardware_Applications} {QuickLeap.ToDiskSize((ulong)ApplicationSpaceInfo.Value)}";
        }
        protected override void HandleLanguageChange(IUILanguageChangedEvent message)
        {
            if (new[] { SystemSpaceInfo, FreeSpaceInfo, ApplicationSpaceInfo }.Any(x => x == null))
            {
                return;
            }
            UpdateChartNames();
        }
    }
}
