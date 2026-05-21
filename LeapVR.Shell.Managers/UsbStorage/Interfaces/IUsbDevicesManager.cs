#region Licence
/****************************************************************
 *  Filename: IUsbDevicesManager.cs
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

namespace LeapVR.Shell.Managers.UsbStorage.Interfaces
{
    /// <summary>
    /// Manages USB Devices connected to Station.
    /// </summary>
    public interface IUsbDevicesManager : IDisposable
    {
        /// <summary>
        /// Collection of <see cref="IUsbStorage"/> representing removeable storage devices connected to Station via USB.
        /// </summary>
        ObservableCollection<IUsbStorage> UsbDrives { get; }
    }
}
