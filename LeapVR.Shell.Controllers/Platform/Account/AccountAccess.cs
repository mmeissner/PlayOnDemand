#region Licence
/****************************************************************
 *  Filename: AccountAccess.cs
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

using LeapVR.Shell.Domain.Models.Platform.Account;

namespace LeapVR.Shell.Controllers.Platform.Account {
    public class AccountAccess : IAccountAccess
    {
        private readonly AccountManager _manager;
        private readonly IPlatformAccountData _accountData;
        private bool _isReleased = false;
        public string Username => _accountData.Username;
        public string Password => _manager.DecryptPassword(_accountData.Password);
        public bool IsReleased => _isReleased;
        internal AccountAccess(AccountManager manager, IPlatformAccountData platformAccountData)
        {
            _manager = manager;
            _accountData = platformAccountData;
        }
        internal string AccountId => PlatformAccount.GetAccountId(_accountData);
        public void Release()
        {
            if(!_isReleased)
            {            
                _manager.ReleaseAccountAccess(this);
                _isReleased = true;
            }
        }
    }
}