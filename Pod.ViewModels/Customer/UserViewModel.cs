#region Licence
/****************************************************************
 *  Filename: UserViewModel.cs
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

namespace Pod.ViewModels.Customer
{
    /// <summary>
    /// A User
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// The Id of the User
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The Username of the User
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// An internal CustomerNumber for the User and for references (e.g. Billing)
        /// </summary>
        public long CustomerNumber { get; set; }

        /// <summary>
        /// The E-Mail Address of the User
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Is true if the User confirmed its E-Mail
        /// </summary>
        public bool EmailConfirmed { get;set; }

        /// <summary>
        /// Is true if the User was locked out from login
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// The number of stations the User has created
        /// </summary>
        public int StationCount { get; set; }
    }


}
