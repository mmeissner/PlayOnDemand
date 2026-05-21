#region Licence
/****************************************************************
 *  Filename: AccountManager.cs
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
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using LeapVR.Utilities.Windows;
using NLog;

namespace LeapVR.Shell.Controllers.Platform.Account
{
    class AccountManager
    {
        #region Private Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly HashSet<string> _accountsInUse = new HashSet<string>();
        //Note: Consider Lock per Platform instead of global lock
        private readonly object _accountUsageLock = new object();
        private readonly Dictionary<string, PlatformAccount> _platformAccounts = new Dictionary<string, PlatformAccount>();
        private readonly Subject<AccountChangeInfo> _whenAccountChangeOccures = new Subject<AccountChangeInfo>();
        private readonly Dictionary<Guid, Platform> _platforms = new Dictionary<Guid, Platform>();

        internal readonly IAppPlatformAccountRepository AccountRepository;
        private readonly string _passwordKey;
        #endregion

        #region Public Properties
        public IObservable<AccountChangeInfo> WhenAccountChangeOccures => _whenAccountChangeOccures.AsObservable();
        #endregion

        #region Constructor
        public AccountManager(IAppPlatformAccountRepository accountRepository, ILocalMachine localMachine)
        {
            AccountRepository = accountRepository;
            _passwordKey = localMachine.VBoxFingerprint;
        }
        #endregion

        #region Public Methods
        public void AddPlatform(Platform platform)
        {
            if(_platforms.ContainsKey(platform.PlatformGuid))return;
            _platforms.Add(platform.PlatformGuid,platform);
        }

        public bool CreateAccount(Guid platformId, string username, string password, out IPlatformAccount account)
        {
            account = null;
            PlatformAccount platformAccount = null;
            var resultVal = false;
            if(!_platforms.TryGetValue(platformId, out var platform))
            {
                Logger.Error($"Could not find Platform Module with Id={platformId}");
                return false;
            }
            lock(_accountUsageLock)
            {
                if(AccountRepository.Store(
                        new PlatformAccountData(platformId,platform.SupportedAccountType, username, EncryptPassword(password)),
                        out var storedAccountData))
                {
                    platformAccount = PlatformAccount.GetAccount(platform,this, storedAccountData);
                    if(_platformAccounts.ContainsKey(platformAccount.AccountId))
                    {
                        throw new InvalidDataException(
                                $"The Dictionary {nameof(platformAccount)} contained already and account with the same Id={platformAccount.AccountId} as it was just now created. This case is unexpected, please check the code");
                    }
                    _platformAccounts.Add(platformAccount.AccountId, platformAccount);
                    account = platformAccount;
                    resultVal = true;
                }
            }

            if(resultVal)
                _whenAccountChangeOccures.OnNext(new AccountChangeInfo(platformAccount, AccountEventType.AddAccount));
            return resultVal;
        }

        public bool DeleteAccount(IPlatformAccount account)
        {
            var resultVal = false;
            IReadOnlyList <Guid> licensedAppIds = new List<Guid>();
            lock(_accountUsageLock)
            {
                if(account is PlatformAccount platformAccount)
                {
                    if(_accountsInUse.Contains(platformAccount.AccountId))
                    {
                        Logger.Warn($"Can't delete account that is currently in use, Id = {platformAccount.AccountId}");
                    }
                    else
                    {
                        licensedAppIds = account.LicensedAppIds();
                        if(AccountRepository.Delete(platformAccount.PlatformAccountData) 
                           && _platformAccounts.ContainsKey(platformAccount.AccountId))
                        {
                            _platformAccounts.Remove(platformAccount.AccountId);
                            resultVal = true;
                        }
                    }
                }
                else
                {
                    throw new InvalidCastException(
                            $"The underlying type of {nameof(IPlatformAccount)} must be of type={typeof(PlatformAccount)}");
                }
            }

            if(resultVal)
            {
                if(licensedAppIds.Any())
                {
                    
                    _whenAccountChangeOccures.OnNext(
                            new AccountChangeInfo(
                                    account,
                                    AccountEventType.RemoveApps,
                                    GetLicenseInfo(licensedAppIds)));
                }

                _whenAccountChangeOccures.OnNext(
                        new AccountChangeInfo(account, AccountEventType.RemoveAccount));
            }

            return resultVal;
        }
        public bool HasAccount(Guid applicationId)
        {
            lock(_accountUsageLock)
            {
                //Try to find availible account from cache
                var availibleAccount = _platformAccounts.Values.FirstOrDefault(
                        x =>
                                x.LicensedAppIds().Contains(
                                        applicationId) &&
                                !_accountsInUse.Contains(x.AccountId));
                if(availibleAccount != null)
                {
                    return true;
                }

                //Try to find availible account from db
                foreach(var accountData in AccountRepository.GetAccountsForApp(applicationId))
                {
                    if(_platformAccounts.ContainsKey(PlatformAccount.GetAccountId(accountData))) continue;
                    if(!_platforms.TryGetValue(accountData.PlatformId, out var platform))continue;
                    var foundAccount = PlatformAccount.GetAccount(platform,this, accountData);
                    _platformAccounts.Add(foundAccount.AccountId, foundAccount);
                    return true;
                }

                return false;
            }
        }
        public List<IPlatformAccount> GetPlatformAccounts(Guid platformId)
        {
            lock(_accountUsageLock)
            {
                var retvalAccounts = new Dictionary<string, IPlatformAccount>();
                foreach(KeyValuePair<string, PlatformAccount> account in _platformAccounts)
                {
                    if(account.Value.PlatformId.Equals(platformId)) retvalAccounts.Add(account.Key, account.Value);
                }

                foreach(var accountData in AccountRepository.GetAccountsForPlatform(platformId))
                {
                    if(_platformAccounts.ContainsKey(PlatformAccount.GetAccountId(accountData))) continue;
                    if(_platforms.TryGetValue(accountData.PlatformId, out var platform))
                    {
                        var canditade = PlatformAccount.GetAccount(platform,this, accountData);
                        _platformAccounts.Add(canditade.AccountId, canditade);
                        retvalAccounts.Add(canditade.AccountId, canditade);
                    }
                }

                return retvalAccounts.Values.ToList();
            }
        }
        public List<IPlatformAccount> GetAllAccountsForApp(Guid applicationId)
        {
            var result = new List<IPlatformAccount>();
            var accountsForApp = AccountRepository.GetAccountsForApp(applicationId);
            if(accountsForApp == null) return result;
            lock(_accountUsageLock)
            {
                foreach(IPlatformAccountData accountData in accountsForApp)
                {
                    var accountId = PlatformAccount.GetAccountId(accountData);
                    if(_platformAccounts.TryGetValue(accountId,out var platformAccount))
                    {
                        result.Add(platformAccount);
                    }
                    else
                    {
                        if(!_platforms.TryGetValue(accountData.PlatformId, out var platform))continue;
                        var newAccount = PlatformAccount.GetAccount(platform,this, accountData);
                        _platformAccounts.Add(accountId,newAccount);
                        result.Add(newAccount);
                    }
                }
            }

            return result;
        }

        public bool TryGetAccountAccessForApp(Guid applicationId, out IPlatformAccount account, out IAccountAccess accountAccess)
        {
            account = null;
            accountAccess = null;
            var accountsForApp = AccountRepository.GetAccountsForApp(applicationId);
            if(accountsForApp == null) return false;
            lock(_accountUsageLock)
            {
                foreach(IPlatformAccountData accountData in accountsForApp)
                {
                    var accountId = PlatformAccount.GetAccountId(accountData);
                    if(_platformAccounts.TryGetValue(accountId,out var platformAccount))
                    {
                        //Account is already alive
                        if(platformAccount.TryGetAccountAccess(out accountAccess))
                        {
                            account = platformAccount;
                            return true;
                        }
                    }
                    else
                    {
                        if(!_platforms.TryGetValue(accountData.PlatformId, out var platform))
                        {
                            Logger.Warn($"Could not find Platform with Id={accountData.PlatformId} for Account Id={PlatformAccount.GetAccountId(accountData)}");
                            continue;
                        }
                        var newAccount = PlatformAccount.GetAccount(platform,this, accountData);
                        _platformAccounts.Add(accountId,newAccount);
                        if(newAccount.TryGetAccountAccess(out accountAccess))
                        {
                            account = newAccount;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public AppLicenseInfo GetLicenseInfo(Guid applicationId)
        {
            return new AppLicenseInfo(applicationId,GetAllAccountsForApp(applicationId));
        }
        public List<AppLicenseInfo> GetLicenseInfo(IEnumerable<Guid> appIdsToCheck)
        {
            var retval = new List<AppLicenseInfo>();
            foreach(Guid appId in appIdsToCheck)
            {
                retval.Add(GetLicenseInfo(appId));
            }

            return retval;
        }
        public HashSet<Guid> GetAppLicensesByPlatform(Guid platformGuid)
        {
            return AccountRepository.GetAllLicenseByPlatform(platformGuid);
        }
        public Task<bool> UpdateLicensesOnline(IPlatformAccount platformAccount)
        {
            throw new NotImplementedException("UpdateLicensesOnline is not yet implemented");
        }
        public Task<bool> UpdateLicensesOnline(IEnumerable<IPlatformAccount> platformAccounts)
        {
            throw new NotImplementedException("UpdateLicensesOnline is not yet implemented");
        }

        public bool GetAccountAccess(Guid applicationId, out IAccountAccess platformAccount)
        {
            lock(_accountUsageLock)
            {
                //Try to find availible account from cache
                var availibleAccount = _platformAccounts.Values.FirstOrDefault(
                        x =>
                                x.LicensedAppIds().Contains(
                                        applicationId) &&
                                !_accountsInUse.Contains(x.AccountId));
                if(availibleAccount != null)
                {
                    return availibleAccount.TryGetAccountAccess(out platformAccount);
                }

                //Try to find availible account from db
                foreach(var accountData in AccountRepository.GetAccountsForApp(applicationId))
                {
                    //If it is already an cached Account we continue  
                    if(_platformAccounts.ContainsKey(PlatformAccount.GetAccountId(accountData))) continue;
                    if(!_platforms.TryGetValue(accountData.PlatformId, out var platform))continue;
                    var foundAccount = PlatformAccount.GetAccount(platform,this, accountData);
                    _platformAccounts.Add(foundAccount.AccountId, foundAccount);
                    return foundAccount.TryGetAccountAccess(out platformAccount);
                }

                platformAccount = null;
                return false;
            }
        }
        #endregion
        

        #region Internal Methods
        internal bool TryGetAccountAccess(PlatformAccount platformAccount, out IAccountAccess access)
        {
            access = null;
            if(_accountsInUse.Contains(platformAccount.AccountId)) return false;
            lock(_accountUsageLock)
            {
                if(_accountsInUse.Contains(platformAccount.AccountId)) return false;
                _accountsInUse.Add(platformAccount.AccountId);
                access = new AccountAccess(this, platformAccount.PlatformAccountData);
                return true;
            }
        }
        internal void ReleaseAccountAccess(AccountAccess access)
        {
            lock(_accountUsageLock)
            {
                _accountsInUse.Remove(access.AccountId);
            }
        }
        internal bool AddApplicationId(IPlatformAccount platformAccount, Guid applicationId)
        {
            if(platformAccount.LicensedAppIds().Contains(applicationId)) return true;
            bool retval = false;
            if(platformAccount is PlatformAccount account)
            {
                account.PlatformAccountData.Applications.Add(applicationId);
                retval = AccountRepository.Update(account.PlatformAccountData);
                if(retval)
                    _whenAccountChangeOccures.OnNext(
                            new AccountChangeInfo(
                                    platformAccount,
                                    AccountEventType.AddApps,
                                    GetLicenseInfo(new[] {applicationId})));
            }
            else
            {
                Logger.Error($"{nameof(IPlatformAccount)} must be of type {typeof(PlatformAccount)}");
            }
            return retval;
        }
        internal bool RemoveApplicationId(IPlatformAccount platformAccount, Guid applicationId)
        {
            if(!platformAccount.LicensedAppIds().Contains(applicationId)) return true;
            bool retval = false;
            if(platformAccount is PlatformAccount account)
            {
                account.PlatformAccountData.Applications.Remove(applicationId);
                retval = AccountRepository.Update(account.PlatformAccountData);
                if(retval)
                    _whenAccountChangeOccures.OnNext(
                            new AccountChangeInfo(
                                    platformAccount,
                                    AccountEventType.RemoveApps,
                                    GetLicenseInfo(new[] {applicationId})));
            }

            return retval;
        }
        internal string DecryptPassword(string encryptedPassword)
        {
            return StringCipher.Decrypt(encryptedPassword, _passwordKey);
        }
        private string EncryptPassword(string plainPassword)
        {
            return StringCipher.Encrypt(plainPassword, _passwordKey);
        }
        
        #endregion


    }
}