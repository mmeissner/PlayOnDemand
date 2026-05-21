#region Licence
/****************************************************************
 *  Filename: IPlatformAccount.cs
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
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models.Platform.Account
{
    public interface IPlatformAccount {

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        string DisplayName { get; }

        /// <summary>
        /// Gets the account Type Flags.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        AccountType Type { get; }

        /// <summary>
        /// Gets the platform identifier the account belongs to.
        /// </summary>
        /// <value>
        /// The platform identifier.
        /// </value>
        Guid PlatformId { get; }

        /// <summary>
        /// Gets a platform unique account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        string AccountId { get; }

        /// <summary>
        /// Gets the licensed application ids.
        /// </summary>
        /// <value>
        /// The licensed application ids.
        /// </value>
        IReadOnlyList<Guid> LicensedAppIds();

        /// <summary>
        /// Tries to receive the account access object.
        /// </summary>
        /// <param name="access">The access.</param>
        /// <returns></returns>
        bool TryGetAccountAccess(out IAccountAccess access);

        /// <summary>
        /// Updates the licenses from a remote source.
        /// </summary>
        /// <returns></returns>
        bool UpdateLicensesFromPlatform();
    }
}
