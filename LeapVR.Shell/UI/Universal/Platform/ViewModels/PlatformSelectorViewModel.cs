#region Licence
/****************************************************************
 *  Filename: PlatformSelectorViewModel.cs
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
using System.Linq;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Platform.Account;
using NLog;

namespace LeapVR.Shell.UI.Universal.Platform.ViewModels {
    public class PlatformSelectorViewModel : Screen
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private PlatformViewModel _selectedPlatform;
        private bool _hasPlatforms;
        private bool _allowSelect;

        public bool HasPlatforms
        {
            get => _hasPlatforms;
            set
            {
                if(value == _hasPlatforms) return;
                _hasPlatforms = value;
                NotifyOfPropertyChange();
            }
        }
        public BindableCollection<PlatformViewModel> Platforms { get; }
        public PlatformViewModel SelectedPlatform
        {
            get => _selectedPlatform;
            set
            {
                if(Equals(value, _selectedPlatform)) return;
                _selectedPlatform = value;
                NotifyOfPropertyChange();
            }
        }
        public bool EnableSelect
        {
            get => _allowSelect;
            set
            {
                if(value == _allowSelect) return;
                _allowSelect = value;
                NotifyOfPropertyChange();
            }
        }

        public PlatformSelectorViewModel(Core.PlatformProvider platformProvider)
        {
            Platforms = new BindableCollection<PlatformViewModel>(platformProvider.GetPlatformViewModels().Where(x => x.Platform.SupportedInstallationTypes.HasFlag(InstallationType.Online) || x.Platform.SupportedInstallationTypes.HasFlag(InstallationType.Local)));
            SelectedPlatform = Platforms.FirstOrDefault();
            if(SelectedPlatform != null){HasPlatforms = true;}
        }
    }
}