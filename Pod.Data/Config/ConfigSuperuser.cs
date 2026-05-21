#region Licence
/****************************************************************
 *  Filename: ConfigSuperuser.cs
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
using Pod.Data.Models.Users;

namespace Pod.Data.Config
{
    /// <summary>
    /// Configuration for <see cref="IDbSetupTask"/> to create a <see cref="ApplicationUser"/> as superuser having all roles available
    /// </summary>
    public class ConfigSuperuser
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string StationPassword { get; set; }
    }
}
