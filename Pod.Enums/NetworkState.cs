#region Licence
/****************************************************************
 *  Filename: NetworkState.cs
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
namespace Pod.Enums
{
    /// <summary>
    /// The Network State of an Station
    /// </summary>
    public enum NetworkState
    {
        /// <summary>
        /// Station is considered not connected to the server 
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// Station requested an server but is still not connected
        /// </summary>
        Connecting = 10,
        /// <summary>
        /// Station is considered connected to the server
        /// </summary>
        Connected = 20,
    }
}