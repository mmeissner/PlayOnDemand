#region Licence
/****************************************************************
 *  Filename: PlatformAccount.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Repository.Interfaces;

namespace LeapVR.Shell.Controllers.Platform.Account {
    public class PlatformAccount : IPlatformAccount
    {
        private readonly AccountManager _accountManager;
        private readonly Platform _platform;

        internal readonly IPlatformAccountData PlatformAccountData;

        public AccountType Type => PlatformAccountData.Type;
        public Guid PlatformId => PlatformAccountData.PlatformId;
        public string DisplayName => PlatformAccountData.Username;
        public string AccountId => GetAccountId(PlatformAccountData);
        public IReadOnlyList<Guid> LicensedAppIds ()=> new List<Guid>(_accountManager.AccountRepository.Get(PlatformId,PlatformAccountData.Username).Applications);
        public bool TryGetAccountAccess(out IAccountAccess access)
        {
            return _accountManager.TryGetAccountAccess(this, out access);
        }

        public bool UpdateLicensesFromPlatform()
        {
            if(!_platform.SupportedAccountType.HasFlag(AccountType.Automatic)) return false;
            throw new NotImplementedException("License update from Platform is not implemented yet");
        }

        PlatformAccount(AccountManager manager,Platform platform, IPlatformAccountData platformData)
        {
            _accountManager = manager;
            _platform = platform;
            PlatformAccountData = platformData;
        }
        internal static PlatformAccount GetAccount(Platform platform, AccountManager manager,IPlatformAccountData platformData)
        {
            if(string.IsNullOrWhiteSpace(platformData.Username))
                throw new ArgumentException("PlatformAccount must have a username");
            if(string.IsNullOrWhiteSpace(platformData.Password))
                throw new ArgumentException("PlatformAccount must have a password");
            return new PlatformAccount(manager,platform, platformData);
        }
        internal static string GetAccountId(IPlatformAccountData accountData)
        {
            return $"{accountData.Username.ToLowerInvariant()}{accountData.PlatformId}";
        }
    }
}