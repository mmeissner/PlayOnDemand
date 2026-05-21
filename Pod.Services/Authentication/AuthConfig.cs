#region Licence
/****************************************************************
 *  Filename: AuthConfig.cs
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
using System.Collections.Generic;
using System.Text;

namespace Pod.Services.Authentication
{
    /// <summary>
    /// Configuration for JWT Authentication
    /// </summary>
    public class AuthConfig
    {
        /// <summary>
        /// The Secret Key to sign JWT with
        /// </summary>
        public string SecretKey { get; set; }
    }
}