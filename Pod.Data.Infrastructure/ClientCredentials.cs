#region Licence
/****************************************************************
 *  Filename: ClientCredentials.cs
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

namespace Pod.Data.Infrastructure {
    /// <summary>
    /// Holding Shell Client Credentials
    /// </summary>
    public struct ClientCredentials
    {
        /// <summary>
        /// The Stations Id
        /// </summary>
        public Guid StationId;

        /// <summary>
        /// The Stations Password
        /// </summary>
        public string Password;
    }
}