#region Licence
/****************************************************************
 *  Filename: UsbStorage.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;

namespace LeapVR.Shell.Managers.UsbStorage
{
    public class UsbStorage : IUsbStorage
    {
        #region Properties & Fields

        public DriveInfo DriveInfo { get; }

        // TODO [RM]: handle storage dead
        private int _isStorageDead; // 0 = false, 1 = true

        #endregion Properties & Fields

        #region Constructors

        internal UsbStorage(DriveInfo driveInfo)
        {
            if (driveInfo == null)
            {
                throw new ArgumentNullException();
            }

            DriveInfo = driveInfo;
        }

        #endregion Constructors

        #region Method

        public IUsbStorageAccess<T> GetStorageAccess<T>() where T : IFile, new()
        {
            return new UsbStorageAccess<T>(this);
        }

        internal bool CheckIfDriveRemoved(bool handleIfRemoved)
        {
            var wasDriveRemoved = !Directory.Exists(DriveInfo.Name);
            if (handleIfRemoved && wasDriveRemoved)
            {
                HandleDriveRemoved();
            }

            return wasDriveRemoved;
        }

        private void HandleDriveRemoved()
        {
            var wasStorageDead = QuickLeap.OperateInterlockedFlag(ref _isStorageDead, true);
            if (wasStorageDead)
            {
                return;
            }

            // TODO [RM]: notify sub-objects about drive remove
        }

        #endregion Method
    }
}
