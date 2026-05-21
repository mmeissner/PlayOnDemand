#region Licence
/****************************************************************
 *  Filename: StoredPackageRepository.cs
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
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Repository.Database;
using LeapVR.Shell.Repository.Entities;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Repository
{
    public class StoredPackageRepository : IStoredPackageRepository
    {
        #region Properties & Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion Properties & Fields

        #region Constructors
        //
        #endregion Constructors

        #region Methods
        public bool Delete(Guid packageGuid)
        {
            try
            {
                var entity = Database.Database.QueryDatabase<StoredPackageDataDb, StoredPackageDataDb>(
                        collection => collection.FindOne(x => x.PackageGuid == packageGuid));
                if(entity == null) return true;
                return entity.Delete<StoredPackageDataDb>();
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException(
                        $"Error on {nameof(Delete)}",
                        exception);
            }
        }

        public IStoredPackageData Get(Guid packageGuid)
        {
            try
            {
                return Database.Database.QueryDatabase<StoredPackageDataDb, StoredPackageDataDb>(
                        collection => collection.FindOne(x => x.PackageGuid == packageGuid));
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException(
                        $"Error on {nameof(Get)} of {nameof(IStoredPackageData)}",
                        exception);
            }
        }

        public IEnumerable<IStoredPackageData> GetAll()
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<StoredPackageDataDb>, StoredPackageDataDb>(
                                        collection => collection.FindAll()).
                                ToList();
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException(
                        $"Error on {nameof(GetAll)} of {nameof(IStoredPackageData)}",
                        exception);
            }
        }

        public IEnumerable<IStoredPackageData> GetAll(Guid applicationGuid)
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<StoredPackageDataDb>, StoredPackageDataDb>(
                                        collection => collection.Find(q => q.ApplicationGuid == applicationGuid)).
                                ToList();
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException(
                        $"Error on {nameof(GetAll)} with ApplicationGuid = {applicationGuid}",
                        exception);
            }
        }

        public IEnumerable<IStoredPackageData> GetAll(ContentType contentType)
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<StoredPackageDataDb>, StoredPackageDataDb>(
                                        collection => collection.Find(q => q.ContentType == contentType)).
                                ToList();
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException(
                        $"Error on {nameof(GetAll)} of ContentType{contentType}",
                        exception);
            }
        }

        public bool Store(IPackageData packageData, PackageState packageState, out IStoredPackageData storedPackage)
        {
            storedPackage = null;
            try
            {
                if(packageData.ApplicationGuid.Equals(Guid.Empty) ||
                   packageData.ContentType.Equals(ContentType.Unset) ||
                   packageData.PackageGuid.Equals(Guid.Empty))
                {
                    Logger.Error("Can not persist Package Data that is incomplete", packageData);
                    return false;
                }

                var storeObj = new StoredPackageDataDb(packageData, packageState);
                storedPackage = storeObj;
                return storeObj.Store();
            }
            catch(LiteDB.LiteException exception)
            {
                Logger.Error($"Error on Store of {nameof(IPackageData)}", packageData);
                throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(IPackageData)}", exception);
            }
        }

        public bool Store(IStoredPackageData packageData)
        {
            try
            {
                if(!(packageData is StoredPackageDataDb storedPackageDataDb))
                {
                    Logger.Error("Can not persist Package Data as it's not the same as returned by the repository!", packageData);
                    throw new InvalidOperationException($"The provided {nameof(IStoredPackageData)}, must be the same object as returned from the repository!");
                }

                if(packageData.ContentType == ContentType.Unset)
                {
                    Logger.Error($"Can not persist Package Data with ContentType={packageData.ContentType}!", packageData);
                    throw new InvalidOperationException($"The provided {nameof(IStoredPackageData)}, can not have a ContentType={packageData.ContentType}!");
                }

                if(packageData.PackageGuid.Equals(Guid.Empty))
                {
                    Logger.Error($"Can not persist Package Data with PackageGuid={Guid.Empty}!", packageData);
                    throw new InvalidOperationException($"The provided {nameof(IStoredPackageData)}, can not have a PackageGuid={Guid.Empty}!");
                }
                return storedPackageDataDb.Store();
            }
            catch(LiteDB.LiteException exception)
            {
                Logger.Error($"Error on Store of {nameof(IPackageData)}", packageData);
                throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(IPackageData)}", exception);
            }
        }
        #endregion Methods
    }
}