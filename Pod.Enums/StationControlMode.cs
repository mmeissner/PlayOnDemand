#region Licence
/****************************************************************
 *  Filename: StationControlMode.cs
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
namespace Pod.Enums {
    public enum StationControlMode
    {
        /// <summary>
        /// Invalid Value
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// The Station can be operated locally and remotely, local settings are unlocked for every user
        /// </summary>
        Local,
        /// <summary>
        /// The Station can only be operated remotely, local settings are protected by a PIN
        /// </summary>
        Remote,
        /// <summary>
        /// The Station shows a QRCode and can only be operated remotely, local settings are protected by a PIN
        /// </summary>
        RemoteWithQrCode
    }
}