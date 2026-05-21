#region Licence
/****************************************************************
 *  Filename: UsbStorageManager.cs
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using LeapVR.Utilities.Windows.WindowsMessages;
using NLog;

namespace LeapVR.Shell.Managers.UsbStorage
{
    public class UsbStorageManager : IUsbDevicesManager
    {
        #region Properties & Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const int DbtDeviceArrival = 0x8000;
        private const int DbtDeviceRemoveComplete = 0x8004;
        private const int WmDeviceChange = 0x0219;

        // TODO [RM]: change to IObservable?
        public ObservableCollection<IUsbStorage> UsbDrives { get; }
            = new ObservableCollection<IUsbStorage>();

        private DriveInfo[] _lastCheckDrives;

        private readonly UsbDeviceMessageOnlyWindow _usbDeviceMessageOnlyWindow;

        private int _isDisposed; // 0 = false, 1 = true

        #endregion Properties & Fields

        #region Constructors

        /// <summary>
        /// Must be called from the UI thread.
        /// </summary>
        public UsbStorageManager()
        {
            AcquireInitialDrives();

            // Create new MessageOnly window with defining messages filter
            _usbDeviceMessageOnlyWindow = new UsbDeviceMessageOnlyWindow(
                messageData =>
                    messageData.Msg == WmDeviceChange &&
                    ((int) messageData.WParam == DbtDeviceArrival || (int) messageData.WParam == DbtDeviceRemoveComplete)
            );
            _usbDeviceMessageOnlyWindow.MessageArrived += OnMessageArrived;
        }

        ~UsbStorageManager()
        {
            Dispose(false);
        }

        #endregion Constructors

        #region Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            bool wasDisposed = QuickLeap.OperateInterlockedFlag(ref _isDisposed, true);
            if (wasDisposed)
            {
                return;
            }

            // free unmanaged

            if (disposing)
            {
                _usbDeviceMessageOnlyWindow.Dispose();
            }
        }

        private void AcquireInitialDrives()
        {
            var drives = DriveInfo.GetDrives();
            _lastCheckDrives = drives;

            foreach (var drive in drives)
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                if (drive.DriveType != DriveType.Removable) // TODO [RM]: USB hard drives have DriveType = DriveType.Fixed, find another way to check/detect
                {
                    continue;
                }

                UsbDrives.Add(new UsbStorage(drive));
            }
        }

        private void OnMessageArrived(object sender, MessageData messageData)
        {
            switch (messageData.Msg)
            {
                case WmDeviceChange:
                    Logger.Debug("Receivecd WmDeviceChange Message from Windows");
                    HandleRecivedDeviceChange(messageData);
                    break;
            }
        }

        private void HandleRecivedDeviceChange(MessageData e)
        {
            switch ((int) e.WParam)
            {
                case DbtDeviceArrival:
                case DbtDeviceRemoveComplete:
                    Thread.Sleep(100); // TODO [RM]: tempfix, make this smarter
                    DifferentiateDrives();
                    break;
            }
        }

        private void DifferentiateDrives()
        {
            var currentDrives = DriveInfo.GetDrives();

            var arrivedDrives = currentDrives.Where(q => !_lastCheckDrives.Select(x => x.Name).Contains(q.Name)).ToList();
            var removedDrives = _lastCheckDrives.Where(q => !currentDrives.Select(x => x.Name).Contains(q.Name)).ToList();

            _lastCheckDrives = currentDrives;

            foreach (var removedDrive in removedDrives)
            {
                var toRemove = UsbDrives.Where(q => q.DriveInfo.Name == removedDrive.Name).ToList();
                foreach (var t in toRemove)
                {
                    UsbDrives.Remove(t);
                }
            }

            foreach (var arrivedDrive in arrivedDrives)
            {
                UsbDrives.Add(new UsbStorage(arrivedDrive));
            }
        }

        #endregion Methods
    }
}
