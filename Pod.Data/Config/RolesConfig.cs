#region Licence
/****************************************************************
 *  Filename: RolesConfig.cs
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
namespace Pod.Data.Config
{
    /// <summary>
    /// Provide all the Roles for for the Application
    /// </summary>
    public static class RolesConfig
    {
        public static string[] Roles = {UserRole,AccountantRole,ServerManagerRole,CustomerSupportRole,AdministratorRole};
        /// <summary>
        /// A Default Role that is assigned to every Account
        /// </summary>
        public const string UserRole = "User";

        /// <summary>
        /// A Role for Billing purposes
        /// </summary>
        public const string AccountantRole = "Accountant";

        /// <summary>
        /// A role to manage Shell Server
        /// </summary>
        public const string ServerManagerRole = "ServerManager";

        /// <summary>
        /// A role to manage User Accounts
        /// </summary>
        public const string CustomerSupportRole = "CustomerSupport";

        /// <summary>
        /// Super User role with access to system critical functions 
        /// </summary>
        public const string AdministratorRole = "Administrator";

    }
}
