#region Licence
/****************************************************************
 *  Filename: IEntity.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.Module;

namespace LeapVR.Shell.Repository.Interfaces.Entities
{
    
    public interface IEntity 
    {
        /// <summary>
        /// Systemwide Unique Database identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        Guid Id { get; set; }
    }
}