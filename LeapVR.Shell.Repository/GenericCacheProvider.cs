#region Licence
/****************************************************************
 *  Filename: GenericCacheProvider.cs
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
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository {
    public class GenericCacheProvider : IGenericCacheProvider
    {

        private readonly object _cacheLock = new object();
        private readonly Dictionary<Guid,GenericCache> _caches = new Dictionary<Guid, GenericCache>();

        /// <summary>
        /// Gets or creates a cache instance to persist data.
        /// </summary>
        /// <param name="ownerId">The owner identifier.</param>
        /// <param name="identifier">The identifier.</param>
        /// <returns></returns>
        public IGenericCache GetModuleCache(Guid ownerId,string identifier)
        {
            lock(_cacheLock)
            {
                if(_caches.TryGetValue(ownerId, out var cache)) return cache;
                var newCache = new GenericCache(ownerId,identifier);
                _caches.Add(ownerId,newCache);
                return newCache;
            }
        } 
    }
}