#region Licence
/****************************************************************
 *  Filename: PlatformAccountDataDb.cs
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository.Entities
{
    
    class PlatformAccountDataDb:IPlatformAccountData, IEntity
    {
        public Guid Id { get; set; }
        public Guid PlatformId { get; set; }
        public AccountType Type { get; set; }
        public HashSet<Guid> Applications { get; set; }
        public string Username { get; set; }
        public string Password {get; set; }

        public PlatformAccountDataDb(){}
        public PlatformAccountDataDb(IPlatformAccountData platformAccountData)
        {
            PlatformId = platformAccountData.PlatformId;
            Type = platformAccountData.Type;
            Applications = platformAccountData.Applications ?? new HashSet<Guid>();
            Username = platformAccountData.Username;
            Password = platformAccountData.Password;
        }
    }
}
