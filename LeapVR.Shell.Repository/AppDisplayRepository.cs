#region Licence
/****************************************************************
 *  Filename: AppDisplayRepository.cs
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
    public class AppDisplayRepository : IAppDisplayRepository
    {
        public bool Store(IAppDisplayData appDisplayData)
        {
            try
            {
                if (appDisplayData.ApplicationGuid == Guid.Empty)
                {
                    throw new NotSupportedException("ApplicationGuid for DisplayInfo must be set to save");
                }
                var storeObj = EntityConverter.Convert(appDisplayData);
                storeObj.Store();
                return true;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Exception during storage to DB of ApplicationDisplayData with Guid={appDisplayData.ApplicationGuid}",exception);
            }
        }

        public bool Delete(Guid applicationGuid)
        {
            try
            {
                var application =
                    Database.Database.QueryDatabase<AppDisplayDataDb, AppDisplayDataDb>(
                        col => col.FindOne(x => x.ApplicationGuid == applicationGuid));
                if (application == null) return true;
                return application.Delete<AppDisplayDataDb>();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryDeleteDbException($"Exception during delete of AppDisplayData with ApplicationGuid={applicationGuid}", exception);
            }
        }

        public IAppDisplayData Get(Guid applicationGuid)
        {
            try
            {
                return Database.Database.QueryDatabase<AppDisplayDataDb, AppDisplayDataDb>(collection => collection.FindOne(x => x.ApplicationGuid == applicationGuid));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error during Get of AppDisplayData with Guid = {applicationGuid}!", exception);
            }
        }

        public IEnumerable<IAppDisplayData> GetAll()
        {
            try
            {
                return Database.Database
                    .QueryDatabase<IEnumerable<AppDisplayDataDb>, AppDisplayDataDb>(collection => collection.FindAll())
                    .ToList();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException("Error during GetAll!",exception);
            }
        }


    }
}