#region Licence
/****************************************************************
 *  Filename: Users.cs
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
using Microsoft.AspNetCore.Identity;
using Pod.Data.Models.Shell;

namespace Pod.Data.Models.Users
{
    /// <summary>
    /// An Account in the System
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        private HashSet<Station> _stations;

        //A customer number that is set automatically and can be used for reference, mainly due to billing
        public long CustomerNumber { get; private set; }

        /// <summary>
        /// Stations for this Account
        /// </summary>
        public IReadOnlyCollection<Station> Stations => _stations;
    }

    /// <summary>
    /// Role for an Account
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
