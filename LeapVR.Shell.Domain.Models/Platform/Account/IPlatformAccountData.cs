#region Licence
/****************************************************************
 *  Filename: IPlatformAccountData.cs
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
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Platform.Account
{
    
    public interface IPlatformAccountData
    {
        Guid PlatformId { get;  }
        AccountType Type { get; }
        HashSet<Guid> Applications { get; }
        string Username { get; }
        string Password {get; }
    }

    
    [Flags]
    public enum AccountType
    {
        /// <summary>
        /// Do not use
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// Do not support Accounts
        /// </summary>
        None = 1,
        /// <summary>
        /// Support for Accounts were licenses can be added manually
        /// </summary>
        Manually = 2,
        /// <summary>
        /// Support for Accounts were licenses can be received from an online source
        /// </summary>
        Automatic = 4
    }

    
    [Flags]
    public enum InstallationType
    {
        /// <summary>
        /// A no value flag that cant be used with HasFlag
        /// </summary>
        None = 0,
        /// <summary>
        /// Installation through a VBox Container
        /// </summary>
        Container = 1,
        /// <summary>
        /// Dummy Installation by adding from a local sources
        /// </summary>
        Local = 2,
        /// <summary>
        /// Online Installation that requires first an download
        /// </summary>
        Online = 4
    }
}
