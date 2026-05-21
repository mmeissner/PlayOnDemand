#region Licence
/****************************************************************
 *  Filename: UsbStorageAccess.cs
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
using LeapVR.Shell.Managers.UsbStorage.Interfaces;

namespace LeapVR.Shell.Managers.UsbStorage
{
    public class UsbStorageAccess<T> : IUsbStorageAccess<T> where T: IFile, new()
    {
        #region Properties & Fields

        public DriveInfo DriveInfo { get; }
        public IFolder<T> RootFolder { get; }

        private UsbStorage _usbStorage;

        #endregion Properties & Fields

        #region Constructors

        internal UsbStorageAccess(UsbStorage usbStorage)
        {
            if (usbStorage == null)
            {
                throw new ArgumentNullException();
            }
            _usbStorage = usbStorage;

            DriveInfo = _usbStorage.DriveInfo;
            RootFolder = new Folder<T>(usbStorage, null, DriveInfo.Name);
        }

        #endregion Constructors

        #region Method

        //

        #endregion Method
    }
}
