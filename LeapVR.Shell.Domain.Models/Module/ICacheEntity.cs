#region Licence
/****************************************************************
 *  Filename: ICacheEntity.cs
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

namespace LeapVR.Shell.Domain.Models.Module
{
    public interface ICacheEntity
    {
        /// <summary>
        /// Id set by the persistence Layer
        /// Do not set manually
        /// </summary>
        /// <value>
        /// The persistence identifier.
        /// </value>
        Guid PersistenceId { get; set; }
        /// <summary>
        /// Individual object identifier. Must be Unique
        /// </summary>
        /// <value>
        /// The object identifier.
        /// </value>
        Guid CacheObjId { get; set; }
    }
}
