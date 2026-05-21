#region Licence
/****************************************************************
 *  Filename: TabItemInstallationViewModel.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public sealed class TabItemInstallationViewModel : TabItemAppManagementScreen
    {

        #region Fields & Properties
        bool disposed = false;
        private UninstallViewModel _uninstallView;
        public UninstallViewModel UninstallView
        {
            get { return _uninstallView; }
            set
            {
                _uninstallView = value;
                NotifyOfPropertyChange(nameof(UninstallView));
            }
        }

        private InstallViewModel _installView;
        public InstallViewModel InstallView
        {
            get { return _installView; }
            set
            {
                _installView = value;
                NotifyOfPropertyChange(nameof(InstallView));
            }
        }

        public override int DisplayOrder => 20;

        public override string DisplayName
        {
            get { return Resources.System_Installation; }
            set { /* ignore */ }
        }

        #endregion

        #region Constructors

        public TabItemInstallationViewModel(
            IUIMessageBroker messageBroker,
            InstallViewModel installViewModel,
            UninstallViewModel uninstallViewModel
            ) : base(messageBroker,"IconInstallation")
        {
            QuickLeap.AssertNotNull(messageBroker, installViewModel, uninstallViewModel);
            _installView = installViewModel;
            _uninstallView = uninstallViewModel;
        }

        #endregion

        #region Methods

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposed)return;
            if (disposing)
            {
                _uninstallView?.Dispose();
                _installView?.Dispose();
            }
            disposed = true;
            base.Dispose(disposing);
        }

        //This is the Conductor, inform other Screens of beeing on display
        protected override void OnActivate()
        {
            base.OnActivate();
            UninstallView.OnDisplay();
            InstallView.OnDisplay();
        }
        protected override void HandleLanguageChange(IUILanguageChangedEvent message) {  }
    }
}
