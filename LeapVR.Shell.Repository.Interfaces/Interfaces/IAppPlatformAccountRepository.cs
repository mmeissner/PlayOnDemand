#region Licence
/****************************************************************
 *  Filename: IAppPlatformAccountRepository.cs
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
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces {
    public interface IAppPlatformAccountRepository {

        /// <summary>
        /// Gets all Platform Accounts in the system.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPlatformAccountData> GetAll();

        /// <summary>
        /// Gets all Platform Accounts for and Platform
        /// </summary>
        /// <param name="platformId">The platform identifier.</param>
        /// <returns>Data of PlatformAccounts</returns>
        IEnumerable<IPlatformAccountData> GetAccountsForPlatform(Guid platformId);

        /// <summary>
        /// Gets all Accounts for an Application
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns>Data of PlatformAccounts</returns>
        IEnumerable<IPlatformAccountData> GetAccountsForApp(Guid applicationGuid);

        /// <summary>
        /// Gets the Licenses the count for an Application by all availible Accounts
        /// </summary>
        /// <param name="applicationId">The application identifier.</param>
        /// <returns>number of licenses</returns>
        int LicenseCount(Guid applicationId);

        /// <summary>
        /// Gets all Licenses availible on an Platform from all Accounts
        /// </summary>
        /// <param name="platformId">The platform identifier.</param>
        /// <returns>ApplicationIds with license</returns>
        HashSet<Guid> GetAllLicenseByPlatform(Guid platformId);

        /// <summary>
        /// Gets the specified platform account data.
        /// </summary>
        /// <param name="platformId">The platform identifier.</param>
        /// <param name="username">The Account Username.</param>
        /// <returns></returns>
        IPlatformAccountData Get(Guid platformId, string username);

        /// <summary>
        /// Deletes the specified Platform account.
        /// </summary>
        /// <param name="platformAccount">The platform account.</param>
        /// <returns>Success</returns>
        bool Delete(IPlatformAccountData platformAccount);

        /// <summary>
        /// Updates the specified Platform account data.
        /// </summary>
        /// <param name="platformAccountData">The application platform account data.</param>
        /// <returns>Success</returns>
        bool Update(IPlatformAccountData platformAccountData);

        /// <summary>
        /// Stores the specified application platform account.
        /// </summary>
        /// <param name="appPlatformAccountData">The application platform account data.</param>
        /// <param name="storedObject">The stored object.</param>
        /// <returns>Success</returns>
        bool Store(IPlatformAccountData appPlatformAccountData, out IPlatformAccountData storedObject);
    }
}