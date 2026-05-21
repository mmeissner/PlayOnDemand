#region Licence
/****************************************************************
 *  Filename: AppPlatformAccountRepository.cs
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
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Repository.Database;
using LeapVR.Shell.Repository.Entities;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Repository
{
    public class AppPlatformAccountRepository : IAppPlatformAccountRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public IEnumerable<IPlatformAccountData> GetAll()
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<PlatformAccountDataDb>, PlatformAccountDataDb>(collection => collection.FindAll()).ToList();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAll)} of {nameof(IPlatformAccountData)}", exception);
            }
        }

        public HashSet<Guid> GetAllLicenseByPlatform(Guid platformId)
        {
            var accountLicenses = Database.Database.QueryDatabase<IEnumerable<HashSet<Guid>>, PlatformAccountDataDb>(collection => collection.Find(x=>x.PlatformId.Equals(platformId)).Select(y=> y.Applications));
            var result = new HashSet<Guid>();
            foreach(HashSet<Guid> hashSet in accountLicenses)
            {
                result.UnionWith(hashSet);
            }
            return result;
        }
        public IPlatformAccountData Get(Guid platformId, string username)
        {
            try
            {
                return Database.Database.QueryDatabase<PlatformAccountDataDb, PlatformAccountDataDb>(collection => collection.FindOne(x=> x.PlatformId.Equals(platformId) && x.Username.Equals(username)));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} of {nameof(IPlatformAccountData)}", exception);
            }
        }

        public int LicenseCount(Guid applicationId)
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<PlatformAccountDataDb>, PlatformAccountDataDb>(collection => collection.Find(x=> x.Applications.Contains(applicationId))).Count();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAll)} of {nameof(IPlatformAccountData)}", exception);
            }
        }
        public IEnumerable<IPlatformAccountData> GetAccountsForPlatform(Guid platformId)
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<PlatformAccountDataDb>, PlatformAccountDataDb>(collection => collection.Find(x => x.PlatformId.Equals(platformId)));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAccountsForPlatform)} for PlatformId= {platformId}", exception);
            }
        }
        public IEnumerable<IPlatformAccountData> GetAccountsForApp(Guid applicationGuid)
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<IPlatformAccountData>, PlatformAccountDataDb>(collection => collection.Find(x=> x.Applications.Contains(applicationGuid)));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAccountsForApp)} for ApplicationGuid= {applicationGuid}", exception);
            }
        }

        public bool Delete(IPlatformAccountData platformAccount)
        {
            try
            {
                if(platformAccount is PlatformAccountDataDb platformAccountDataDb)
                {
                    return platformAccountDataDb.Delete<PlatformAccountDataDb>();
                }
                Logger.Warn($"Cant delete  Type of {typeof(IPlatformAccountData)} that is not of type= {typeof(PlatformAccountDataDb)}");
                return false;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryDeleteDbException($"Error on {nameof(Delete)} of {nameof(IPlatformAccountData)}  with Username = {platformAccount.Username}, PlatformId={platformAccount.PlatformId}", exception);
            }
        }

        public bool Update(IPlatformAccountData platformAccountData)
        {
            if(platformAccountData is PlatformAccountDataDb platformAccountDb)
            {
                return platformAccountDb.Store();
            }
            Logger.Error($"Type to Update was not {nameof(PlatformAccountDataDb)}! It is {platformAccountData.GetType()}",platformAccountData);
            return false;
        }

        public bool Store(IPlatformAccountData appPlatformAccountData, out IPlatformAccountData storedObject)
        {
            try
            {
                storedObject = null;
                if (String.IsNullOrWhiteSpace(appPlatformAccountData.Password) || String.IsNullOrWhiteSpace(appPlatformAccountData.Username))
                {
                    throw new NotSupportedException("Platform Account Data must have a Platfrom other then Custom and a Username and Password");
                }
                //Check if there is already an Account with the same Platform and Username
                var entity = Database.Database.QueryDatabase<PlatformAccountDataDb, PlatformAccountDataDb>(collection => collection.FindOne(x => x.Username.Equals(appPlatformAccountData.Username) && x.PlatformId.Equals(appPlatformAccountData.PlatformId)));
                if(entity != null)
                {
                    if(!(appPlatformAccountData is PlatformAccountDataDb accountDataDb) ||
                       !accountDataDb.Id.Equals(entity.Id))
                    {
                        Logger.Error("Can't store an Platform Account with same Username and Platform twice!");
                        return false;
                    }
                }
                //Dont need to go over Converter as we did a previous check for not beeing an existing Object
                var storeObj = new PlatformAccountDataDb(appPlatformAccountData);
                storedObject = storeObj;
                return  storeObj.Store();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(IPlatformAccountData)}  with Username = {appPlatformAccountData.Username}, PlatformId = {appPlatformAccountData.PlatformId}", exception);
            }
        }
    }
}
