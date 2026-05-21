#region Licence
/****************************************************************
 *  Filename: SystemSettingsService.cs
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
using Pod.ViewModels.Admin;

namespace Pod.Services.System
{
    /// <summary>
    /// Singleton Service providing system settings to other services
    /// This service does not persist settings at the current moment
    /// </summary>
    public class SystemSettingsService
    {
        private SystemSettingsViewModel _settings = new SystemSettingsViewModel()
                                                    {
                                                            MaxStationsPerUser = 3,
                                                            UserRegistrationEnabled = true,
                                                    };

        /// <summary>
        /// Receives the current system settings
        /// </summary>
        /// <param name="userRegistrationEnabled">true to allow new users to register</param>
        /// <param name="maxStationsPerUser">limit per user for amount of stations</param>
        /// <returns></returns>
        public SystemSettingsViewModel SetSystemSettings(bool userRegistrationEnabled,int maxStationsPerUser)
        {
            var retval = new SystemSettingsViewModel()
                        {
                                UserRegistrationEnabled = userRegistrationEnabled,
                                MaxStationsPerUser = maxStationsPerUser,
                        };
            _settings = retval;
            return retval;
        }

        /// <summary>
        /// Receives the current system settings
        /// </summary>
        public SystemSettingsViewModel GetSystemSettings => _settings;
    }
}
