#region Licence
/****************************************************************
 *  Filename: UsbStickItemViewModel.cs
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
using Caliburn.Micro;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using LeapVR.Shell.Modules.Container;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels
{
    public class UsbStickItemViewModel : Screen
    {

        #region Fields & Properties


        private IUsbStorageAccess<AppInstallationFile> _usbStorageAccess;
        public IUsbStorageAccess<AppInstallationFile> UsbStorageAccess
        {
            get => _usbStorageAccess;
            set
            {
                _usbStorageAccess = value;
                NotifyOfPropertyChange();
            }
        }


        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors

        public UsbStickItemViewModel(IUsbStorageAccess<AppInstallationFile> usbStorageAccess)
        {
            _usbStorageAccess = usbStorageAccess;
            Name = _usbStorageAccess.DriveInfo.Name;
        }
        #endregion

        #region Methods

        #endregion

    }
}
