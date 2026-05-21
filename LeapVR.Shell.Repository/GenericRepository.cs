#region Licence
/****************************************************************
 *  Filename: GenericRepository.cs
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
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Repository.Database;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using LiteDB;
using NLog;
using Logger = NLog.Logger;

namespace LeapVR.Shell.Repository {
    public sealed class GenericRepository<T> : IGenericRepository<T> where T : ICacheEntity,new() 
    {
        // ReSharper disable StaticMemberInGenericType
        // Not Shared Static Values among instances of different close constructed types is required to
        // only register once per close constructed type
        private static bool _isRegistered = false;
        private static readonly object RegistrationLock = new object();
        private readonly string _collectionId;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        GenericRepository(string collectionId)
        {
            if(_isRegistered) return;
            lock(RegistrationLock)
            {
                if(_isRegistered) return;
                _collectionId = collectionId;
                Register();
                _isRegistered = true;
            }
        }

        public bool Store(T obj)
        {
            try
            {
                if(obj.CacheObjId == Guid.Empty)
                {
                    throw new NotSupportedException($"Object Id for {typeof(T)} must be set to save");
                }

                obj.Store(_collectionId);
                return true;
            }
            catch(LiteException exception)
            {
                throw new RepositoryStoreDbException(
                        $"Exception during storage to DB of {typeof(T)} with Guid={obj.CacheObjId}",
                        exception);
            }
        }

        public bool Delete(Guid objectId)
        {
            try
            {
                var obj =
                        Database.Database.QueryDatabase<T, T>(_collectionId,
                                col => col.FindOne(x => x.CacheObjId == objectId));
                if(obj == null) return true;
                return obj.Delete(_collectionId);
            }
            catch(LiteException exception)
            {
                throw new RepositoryDeleteDbException(
                        $"Exception during delete of {typeof(T)} with objectId={objectId}",
                        exception);
            }
        }
        public void DeleteAll()
        {
            try
            {
                Database.Database.DeleteAll<T>(_collectionId);
            }
            catch(LiteException exception)
            {
                throw new RepositoryDeleteDbException(
                        $"Exception during delete of all entities from collection with Id={_collectionId}",
                        exception);
            }
        }

        public T Get(Guid objectId)
        {
            try
            {
                return Database.Database.QueryDatabase<T, T>(_collectionId,
                        collection => collection.FindOne(x => x.CacheObjId == objectId));
            }
            catch(LiteException exception)
            {
                throw new RepositoryGetDbException(
                        $"Error during Get of {typeof(T)}  with objectId = {objectId}!",
                        exception);
            }
        }

        public IEnumerable<T> GetAll()
        {
            try
            {
                return Database.Database.QueryDatabase<IEnumerable<T>, T>(_collectionId,
                                        collection => collection.FindAll()).
                                ToList();
            }
            catch(LiteException exception)
            {
                throw new RepositoryGetDbException($"Error during GetAll of {typeof(T)}!", exception);
            }
        }

        internal static GenericRepository<T> CreateRepository(string collectionId)
        {
            if(string.IsNullOrEmpty(collectionId))throw new ArgumentNullException($"Parameter {nameof(collectionId)} can not be null or empty");
            if(collectionId.Length >15)throw new ArgumentException($"Parameter {nameof(collectionId)} can not be longer then 15 Chars");
            return new GenericRepository<T>(collectionId);
        }

        private void Register()
        {
            Logger.Debug($"Registering Type for Repository Type={typeof(T)}");
            //Don't change the order as otherwise Mapper will be null
            var liteDb = Database.Database.GetLiteDB();
            var mapper = Database.Database.Mapper;

            mapper.Entity<T>().Id(x => x.PersistenceId);
            liteDb.GetCollection<T>(_collectionId).EnsureIndex(x => x.PersistenceId);
        }
    }
}