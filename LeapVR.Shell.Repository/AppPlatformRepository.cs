#region Licence
/****************************************************************
 *  Filename: AppPlatformRepository.cs
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
    public class AppPlatformRepository : IAppPlatformRepository
    {
        public IAppPlatformData Get(Guid applicationGuid)
        {
            try
            {
                return Database.Database.QueryDatabase<AppPlatformDataDb, AppPlatformDataDb>(collection => collection.FindOne(x => x.ApplicationGuid == applicationGuid));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} of {nameof(IAppPlatformData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }

        public bool IsAppEnabled(Guid applicationGuid)
        {
            try
            {
                var app = Database.Database.QueryDatabase<AppPlatformDataDb, AppPlatformDataDb>(collection => collection.FindOne(x=> x.ApplicationGuid == applicationGuid));
                return app != null && app.IsEnabled;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} of {nameof(IAppPlatformData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }

        public bool SetAppEnabled(Guid applicationGuid, bool enabledValue, out Guid platformId)
        {
            var app = Database.Database.QueryDatabase<AppPlatformDataDb, AppPlatformDataDb>(collection => collection.FindOne(x=> x.ApplicationGuid == applicationGuid));
            if(app == null)
            {
                platformId = Guid.Empty;
                return false;
            }
            platformId = app.PlatformPluginId;
            if(app.IsEnabled == enabledValue)
            {
                return true;
            }
            app.IsEnabled = enabledValue;
            return app.Store();
        }

        public bool TryGetEnabledApp(Guid applicationGuid, out IAppPlatformData platformData)
        {
            try
            {
                platformData = Database.Database.QueryDatabase<AppPlatformDataDb, AppPlatformDataDb>(collection => collection.FindOne(x => x.ApplicationGuid == applicationGuid && x.IsEnabled));
                return platformData != null;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} of {nameof(IAppPlatformData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }
        public IEnumerable<IAppPlatformData> GetAll()
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<AppPlatformDataDb>, AppPlatformDataDb>(collection => collection.FindAll()).ToList();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAll)} of {nameof(IAppPlatformData)}", exception);
            }
        }

        public IEnumerable<Guid> GetAllEnabledApps()
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<Guid>, AppPlatformDataDb>(collection =>collection.Find(x => x.IsEnabled).Select(x=> x.ApplicationGuid));
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetAllEnabledApps)} of {nameof(IAppPlatformData)}", exception);
            }
        }

        public bool Update(Guid applicationGuid,IEnumerable<IProcessExecutionLogic> processExecutionLogic)
        {
            var platformData = Database.Database.QueryDatabase<AppPlatformDataDb, AppPlatformDataDb>(collection => collection.FindOne(x => x.ApplicationGuid == applicationGuid));
            platformData.ExecutionLogicInstructions = EntityConverter.Convert(processExecutionLogic);
            return platformData.Store();
        }

        public bool Store(IAppPlatformData appPlatformData)
        {
            try
            {
                if (appPlatformData.ApplicationGuid == Guid.Empty)
                {
                    throw new NotSupportedException("ApplicationGuid for PlatformInfo must be set to save");
                }
                var storeObj = EntityConverter.Convert(appPlatformData);
                storeObj.Store();
                return true;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(IAppPlatformData)}  with ApplicationGuid = {appPlatformData.ApplicationGuid}", exception);
            }
        }

        public bool Delete(Guid applicationGuid)
        {
            try
            {
                var entity = Database.Database.QueryDatabase<AppPlatformDataDb, AppPlatformDataDb>(collection => collection.FindOne(x => x.ApplicationGuid == applicationGuid));
                return entity == null || entity.Delete<AppPlatformDataDb>();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryDeleteDbException($"Error on {nameof(Delete)} of {nameof(IAppPlatformData)}  with ApplicationGuid = {applicationGuid}", exception);
            }
        }
    }
}
