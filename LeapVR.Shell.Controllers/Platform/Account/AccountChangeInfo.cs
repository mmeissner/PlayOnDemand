#region Licence
/****************************************************************
 *  Filename: AccountChangeInfo.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Controllers.Platform.Account {
    class AccountChangeInfo
    {
        public AccountChangeInfo(IPlatformAccount account, AccountEventType eventType, List<AppLicenseInfo> licenseInfo = null)
        {
            Account = account;
            AccountId = account.AccountId;
            PlatformId = account.PlatformId;
            EventType = eventType;
            AccountType = account.Type;
            LicenseInfo = licenseInfo ?? new List<AppLicenseInfo>();
        }
        IPlatformAccount Account { get; }
        public string AccountId { get; }
        public Guid PlatformId { get; }
        public List<AppLicenseInfo> LicenseInfo { get; }
        public AccountType AccountType { get; }
        public AccountEventType EventType { get; }
    }

    class AppLicenseInfo : IAppLicenseInfo
    {
        public AppLicenseInfo(Guid applicationId, IEnumerable<IPlatformAccount> accounts)
        {
            ApplicationId = applicationId;
            Accounts = accounts == null ? new List<IPlatformAccount>() : new List<IPlatformAccount>(accounts);
            CurrentLicenseCount = Accounts.Count;
        }
        public IReadOnlyList<IPlatformAccount> Accounts { get; }
        public int CurrentLicenseCount { get; }
        public Guid ApplicationId { get; }
    }
}