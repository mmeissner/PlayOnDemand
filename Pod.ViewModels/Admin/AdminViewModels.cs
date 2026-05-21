#region Licence
/****************************************************************
 *  Filename: AdminViewModels.cs
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

namespace Pod.ViewModels.Admin
{

    public class UserRoleViewModel
    {
        public string Name { get; set; }
    }

    public class SystemSettingsViewModel
    {
        public bool UserRegistrationEnabled { get; set; }
        public int MaxStationsPerUser { get; set; }
    }
}
