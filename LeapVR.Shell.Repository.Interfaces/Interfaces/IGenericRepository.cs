#region Licence
/****************************************************************
 *  Filename: IGenericRepository.cs
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
using LeapVR.Shell.Repository.Interfaces.Entities;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces {
    public interface IGenericRepository<T> where T : ICacheEntity, new() {
        bool Store(T obj);
        bool Delete(Guid objectId);
        void DeleteAll();
        T Get(Guid objectId);
        IEnumerable<T> GetAll();
    }
}