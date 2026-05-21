#region Licence
/****************************************************************
 *  Filename: PlatformAccountData.cs
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
using LeapVR.Shell.Domain.Models.Platform.Account;

namespace LeapVR.Shell.Repository.Interfaces.Entities
{
    
    public class PlatformAccountData : IPlatformAccountData
    {
        public PlatformAccountData(Guid platformId,AccountType accountType, string username, string password)
        {
            PlatformId = platformId;
            Type = accountType;
            Username = username;
            Password = password;
            Applications = new HashSet<Guid>();
        }
        public Guid PlatformId { get; set; }
        public AccountType Type { get; set; }
        public HashSet<Guid> Applications { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}