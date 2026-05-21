#region Licence
/****************************************************************
 *  Filename: AppInstallationRepository.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Repository.Database;
using LeapVR.Shell.Repository.Entities;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository
{
    public class AppInstallationRepository : IAppInstallationRepository
    {

        public IEnumerable<IAppInstallationData> GetAll()
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<AppInstallationDataDb>, AppInstallationDataDb>(collection => collection.FindAll()).ToList();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAll)}",exception);
            }
        }

        public IEnumerable<IAppInstallationData> GetAllByPlatformId(Guid platformId)
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<AppInstallationDataDb>, AppInstallationDataDb>(collection => collection.Find(x=> x.PlatformPluginGuid.Equals(platformId))).ToList();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAllByPlatformId)}",exception);
            }
        }

        public IAppInstallationData Get(Guid applicationGuid)
        {
            try
            {
                return Database.Database.QueryDatabase<AppInstallationDataDb, AppInstallationDataDb>(collection => collection.FindOne(x => x.ApplicationGuid == applicationGuid));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} with ApplicationGuid = {applicationGuid}", exception);
            }
            
        }

        public bool Store(IAppInstallationData appInstallationData)
        {
            try
            {
                var storeObj = EntityConverter.Convert(appInstallationData);
                storeObj.Store();
                return true;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(IAppInstallationData)}  with ApplicationGuid = {appInstallationData?.ApplicationGuid}", exception);
            }
        }

        public bool Delete(Guid applicationGuid)
        {
            try
            {
                var entity = Database.Database.QueryDatabase<AppInstallationDataDb, AppInstallationDataDb>(collection =>
                    collection.FindOne(x => x.ApplicationGuid == applicationGuid));
                if (entity == null) return true;
                return entity.Delete<AppInstallationDataDb>();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryDeleteDbException($"Error on {nameof(Delete)} of {nameof(IAppInstallationData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }
    }
}
