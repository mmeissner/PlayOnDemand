#region Licence
/****************************************************************
 *  Filename: GenericCache.cs
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
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Repository {
    class GenericCache : IGenericCache
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string CacheSuffix = "_Cache";
        private readonly object _dicAccessLock = new object();
        private readonly string _collectionId;
        private readonly Dictionary<Type,object> _repoDictionary = new Dictionary<Type, object>();
     
        internal GenericCache(Guid ownerId, string collectionId)
        {
            _collectionId = collectionId + CacheSuffix;
            Logger.Debug($"Construction of new Generic Cache with OwnerId={ownerId}, CollectionId={_collectionId}");
            FileCache = new GenericFileRepository(ownerId);
        }
        public IGenericFileRepository FileCache { get; }
        public IGenericRepository<T> GetModuleRepository<T>() where T:ICacheEntity, new()
        {
            lock(_dicAccessLock)
            {            
                Logger.Debug($"Trying to get Repository Type={typeof(T)}");
                if(_repoDictionary.TryGetValue(typeof(T), out var repo))
                {
                    Logger.Debug($"Returning Cached Repository of Type={typeof(T)}");
                    return (GenericRepository<T>)repo;
                }
                Logger.Debug($"Creating new cached Repository of Type={typeof(T)}");
                var newRepo = GenericRepository<T>.CreateRepository(_collectionId);
                _repoDictionary.Add(typeof(T),newRepo);
                return newRepo;
            }
        }
    }
}