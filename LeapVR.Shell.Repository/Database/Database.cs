#region Licence
/****************************************************************
 *  Filename: Database.cs
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
#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Repository.Database.Migrations;
using LeapVR.Shell.Repository.Entities;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LiteDB;
using NLog;
using Logger = NLog.Logger;
#endregion

namespace LeapVR.Shell.Repository.Database
{
    public static class Database
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object DBLock = new object();
        private static LiteDatabase _liteDatabase;
        private static string _databaseFile;
        private static bool _isClosed = false;

        public static BsonMapper Mapper { get; private set; }

        static Database()
        {
            #region Connection String Parameter
            //Options for connection string
            //LiteDatabase can be initialize using a string connection, with key1 = value1; key2 = value2; ... syntax.
            //If there is no; in your connection string, LiteDB assume that your connection string is Filename key.Keys are case insensitive.
            //Filename(string): Full path or relative path from DLL directory.
            //Journal(bool): Enabled or disable double write check to ensure durability (default: true)
            //Password(string): Encrypt(using AES) your datafile with a password(default: null - no encryption)
            //Cache Size (int): Max number of pages in cache.After this size, flush data to disk to avoid too memory usage(default: 5000)
            //Timeout(TimeSpan): Timeout for waiting unlock operations(thread lock and locked file)
            //Mode(Exclusive | ReadOnly | Shared): How datafile will be open(defult: Shared in NET35 and Exclusive in NetStandard)
            //Initial Size (string | long): If database is new, initialize with allocated space -support KB, MB, GB(default: null)
            //Limit Size (string | long): Max limit of datafile -support KB, MB, GB(default: null)
            //Upgrade(bool): If true, try upgrade datafile from old version (v2)(default: null)
            //Log(byte): Debug messages from database -use LiteDatabase.Log(default: Logger.NONE)
            //Async(bool): Support "sync over async" file stream creation to use in UWP access any disk folder(only for NetStandard, default: false)
            #endregion

            Initialize();
        }

        private static void Initialize()
        {
            try
            {
                var globalConfig = GlobalConfig.GetGlobalConfiguration();
                _databaseFile = globalConfig.DatabaseFilePath;

                bool isNewDb = !File.Exists(_databaseFile);

                //Migrate DB if needed
                if(!isNewDb)
                {
                    Migration.Migrate($"Filename={_databaseFile};Mode=Exclusive");
                }
                // Create DB and insert initial data if does not exists yet
                else
                {
                    SetupDb.WriteDbInfo($"Filename={_databaseFile};Mode=Exclusive");
                }

                //Log for now Open DB only if we are in Trace LogLevel
                _liteDatabase = OpenDb(Logger.IsTraceEnabled,out var mapper);
                Mapper = mapper;

                //Do Entity Mapping
                EntityMapping(_liteDatabase,Mapper);


                //Populate with ability to use Repositories
                if(isNewDb) SetupDb.Populate(GlobalConfig.GetGlobalConfiguration());

                Logger.Info($"SelectBaseDirectory called; New DatabaseFile  =`{_databaseFile}`.");
            }
            catch(System.Exception e)
            {
                Logger.Fatal(e);
                throw;
            }
        }

        public static void CloseDb()
        {
            lock(DBLock)
            {
                if(_isClosed) return;
                _isClosed = true;
                if(_liteDatabase == null) return;
                _liteDatabase.Dispose();
            }
        }

        internal static LiteDatabase GetLiteDB()
        {
            try
            {
                if(_isClosed) throw new System.Exception("Database was already closed!");
                return _liteDatabase;
            }
            catch(System.Exception exception)
            {
                Logger.Fatal(exception);
                throw;
            }
        }

        #region Entity Functions
        public static TResult QueryDatabase<TResult, T>(Func<LiteCollection<T>, TResult> action)
                where T : IEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(typeof(T).Name);
            return action(col);
        }

        public static bool Store<T>(this T storeObject) where T : IEntity, new()
        {
            if(storeObject == null) return false;
            if(storeObject.Id != Guid.Empty)
            {
                if(!Update(storeObject))
                {
                    throw new System.Exception("Cant update object in database!");
                }
            }
            else
            {
                Insert(storeObject);
                if(storeObject.Id == Guid.Empty)
                {
                    throw new RepositoryStoreDbException("Cant insert object in database!");
                }
            }

            return true;
        }

        public static void Store<T>(this IEnumerable<T> objects) where T : IEntity, new()
        {
            if(objects == null) return;
            var x = GetLiteDB();
            var col = x.GetCollection<T>(typeof(T).Name);
            col.Insert(objects);
        }

        public static bool Delete<T>(this IEntity entity) where T : IEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(typeof(T).Name);
            return col.Delete(entity.Id);
        }
        #endregion

        #region Entity Functions with Specific collection Id for Generic Repository
        public static T GetLiteDB<T>(string collectionId, Guid entityId) where T : ICacheEntity, new()
        {
            var x = GetLiteDB();
            var collection = x.GetCollection<T>(collectionId);
            return collection.FindById(new BsonValue(entityId));
        }

        public static TResult QueryDatabase<TResult, T>(string collectionId, Func<LiteCollection<T>, TResult> action)
                where T : ICacheEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(collectionId);
            return action(col);
        }

        public static bool Store<T>(this T storeObject, string collectionId) where T : ICacheEntity, new()
        {
            if(storeObject == null) return false;
            if(storeObject.PersistenceId != Guid.Empty)
            {
                if(!Update(storeObject, collectionId))
                {
                    throw new System.Exception("Cant update object in database!");
                }
            }
            else
            {
                Insert(storeObject, collectionId);
                if(storeObject.PersistenceId == Guid.Empty)
                {
                    throw new RepositoryStoreDbException("Cant insert object in database!");
                }
            }

            return true;
        }

        public static void Store<T>(this IEnumerable<T> objects, string collectionId) where T : ICacheEntity, new()
        {
            if(objects == null) return;
            var x = GetLiteDB();
            var col = x.GetCollection<T>(collectionId);
            col.Insert(objects);
        }

        public static bool Delete<T>(this T entity, string collectionId) where T : ICacheEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(collectionId);
            return col.Delete(entity.PersistenceId);
        }

        public static void DeleteAll<T>(string collectionId) where T : ICacheEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(collectionId);
            var objects = col.FindAll();
            foreach(T obj in objects)
            {
                col.Delete(obj.PersistenceId);
            }
        }
        #endregion

        #region File Functions
        public static DBFileInfo FileUpload(string id, string filePathName)
        {
            return new DBFileInfo(GetLiteDB().FileStorage.Upload(id, filePathName));
        }
        public static DBFileInfo FileUpload(string id, string fileName, Stream stream)
        {
            return new DBFileInfo(GetLiteDB().FileStorage.Upload(id, fileName, stream));
        }
        public static DBFileInfo FileDownload(string id, Stream stream)
        {
            return new DBFileInfo(GetLiteDB().FileStorage.Download(id, stream));
        }

        public static bool FileDelete(string id) { return GetLiteDB().FileStorage.Delete(id); }

        public static IEnumerable<DBFileInfo> FileFind(string id)
        {
            return GetLiteDB().FileStorage.Find(id).Select(x => new DBFileInfo(x));
        }

        public static IEnumerable<DBFileInfo> FileFindAll()
        {
            return GetLiteDB().FileStorage.FindAll().Select(x => new DBFileInfo(x));
        }

        public static DBFileInfo FileFindById(string id)
        {
            var file = GetLiteDB().FileStorage.FindById(id);
            return file != null ? new DBFileInfo(file) : null;
        }

        public static Stream FileOpenRead(string id) { return GetLiteDB().FileStorage.OpenRead(id); }
        #endregion

        #region Mapping
        private static void EntityMapping(LiteDatabase liteDatabase, BsonMapper mapper)
        {

            //General
            Logger.Debug($"Mapping {nameof(DiskEntityDb)}");
            mapper.Entity<DiskEntityDb>();

            //Application Display Data Config
            Logger.Debug($"Mapping {nameof(AppDisplayDataDb)}");
            mapper.Entity<AppDisplayDataDb>().Id(x => x.Id);
            liteDatabase.GetCollection<AppDisplayDataDb>().EnsureIndex(x => x.ApplicationGuid);

            //Application Hardware Config
            Logger.Debug($"Mapping {nameof(AppHardwareDataDb)}");
            mapper.Entity<AppHardwareDataDb>().Id(x => x.Id);
            liteDatabase.GetCollection<AppHardwareDataDb>().EnsureIndex(x => x.ApplicationGuid);

            //Application Platform Config
            Logger.Debug($"Mapping {nameof(AppPlatformDataDb)}");
            mapper.Entity<AppPlatformDataDb>().Id(x => x.Id);
            //Register of Type seems here to be required as otherwise the type can not be resolved
            Logger.Debug($"Registering Type {nameof(IProcessExecutionLogic)}");
            mapper.RegisterType(
                    typeof(IProcessExecutionLogic),
                    o => mapper.ToDocument(o),
                    o => mapper.ToObject<ProcessExecutionLogicDb>(o.AsDocument));
            liteDatabase.GetCollection<AppPlatformDataDb>().EnsureIndex(x => x.ApplicationGuid);

            //Platform Account Config
            Logger.Debug($"Mapping {nameof(PlatformAccountDataDb)}");
            mapper.Entity<PlatformAccountDataDb>().Id(x => x.Id);
            liteDatabase.GetCollection<PlatformAccountDataDb>().EnsureIndex(x => x.Username);
            liteDatabase.GetCollection<PlatformAccountDataDb>().EnsureIndex(x => x.PlatformId);
            liteDatabase.GetCollection<PlatformAccountDataDb>().EnsureIndex(x => x.Applications);

            //AppInstallation Config
            Logger.Debug($"Mapping {nameof(AppInstallationDataDb)}");
            mapper.Entity<AppInstallationDataDb>().Id(x => x.Id);
            liteDatabase.GetCollection<AppInstallationDataDb>().EnsureIndex(x => x.ApplicationGuid);
            liteDatabase.GetCollection<AppInstallationDataDb>().EnsureIndex(x => x.PlatformPluginGuid);

            //AppStatistic Config
            Logger.Debug($"Mapping {nameof(AppStatisticsDataDb)}");
            mapper.Entity<AppStatisticsDataDb>().Id(x => x.Id);
            liteDatabase.GetCollection<AppStatisticsDataDb>().EnsureIndex(x => x.ApplicationGuid);
            liteDatabase.GetCollection<AppStatisticsDataDb>().EnsureIndex(x => x.PlatformGuid);

            //Multimedia Settings
            Logger.Debug($"Mapping {nameof(MultimediaSettingsDataDb)}");
            mapper.Entity<MultimediaSettingsDataDb>().Id(x => x.Id);
            liteDatabase.GetCollection<MultimediaSettingsDataDb>().EnsureIndex(x => x.Identifier);

            Logger.Debug($"Mapping {nameof(MultimediaPlaylistDataDb)}");
            mapper.Entity<MultimediaPlaylistDataDb>().Id(x => x.Id);
            liteDatabase.GetCollection<MultimediaPlaylistDataDb>().EnsureIndex(x => x.Identifier);
        }
        #endregion

        #region Private Methods
        private static LiteDatabase OpenDb(bool enableLogging, out BsonMapper mapper)
        {
            lock(DBLock)
            {
                _isClosed = false;
                mapper = new BsonMapper();
                var connectionString = $"Filename={_databaseFile};Mode=Exclusive";
                Logger.Debug($"Using Connection String = {connectionString}");
                var retval = new LiteDatabase(connectionString, mapper);
                if(enableLogging)
                {
                    retval.Log.Level = LiteDB.Logger.FULL;
                    retval.Log.Logging += Log_Logging;
                }

                return retval;
            }
        }

        private static void Log_Logging(string obj) { Logger.Debug($"LITEDB = {obj}"); }

        private static BsonValue Insert<T>(this T newObject) where T : IEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(typeof(T).Name);
            return col.Insert(newObject);
        }

        private static bool Update<T>(this T updatedObject) where T : IEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(typeof(T).Name);
            return col.Update(updatedObject);
        }

        private static BsonValue Insert<T>(this T newObject, string collectionId) where T : ICacheEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(collectionId);
            return col.Insert(newObject);
        }

        private static bool Update<T>(this T updatedObject, string collectionId) where T : ICacheEntity, new()
        {
            var x = GetLiteDB();
            var col = x.GetCollection<T>(collectionId);
            return col.Update(updatedObject);
        }
        #endregion
    }
}