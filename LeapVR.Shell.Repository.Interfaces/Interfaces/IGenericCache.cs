#region Licence
/****************************************************************
 *  Filename: IGenericCache.cs
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
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Repository.Interfaces.Entities;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces
{
    public interface IGenericCache
    {
        /// <summary>
        /// Gets a file cache repository to persist files.
        /// </summary>
        /// <value>
        /// The file cache.
        /// </value>
        IGenericFileRepository FileCache { get; }

        /// <summary>
        /// Gets or creates repository for cache objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IGenericRepository<T> GetModuleRepository<T>() where T : ICacheEntity, new();
    }
}