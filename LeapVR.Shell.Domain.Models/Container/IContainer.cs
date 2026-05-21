#region Licence
/****************************************************************
 *  Filename: IContainer.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Collections.Generic;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Application (game) container. Composed of multiple packages (see <see cref="IPackageData"/>) that contains application and LeapVR system-specific files.
    /// </summary>
    /// <typeparam name="T">Type of package implementing <see cref="IPackageData"/>.</typeparam>
    public interface IContainer<T> where T : IPackageData
    {
        /// <summary>
        /// Globally unique identifier of application.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Version of container. Reserved for further use.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Total amount of files stored in all packages in container, sumed.
        /// </summary>
        int TotalFilesCount { get; }

        /// <summary>
        /// Combined size of all files contained in all packages, sumed.
        /// </summary>
        long TotalFilesSize { get; }

        /// <summary>
        /// Fills up all data about container.
        /// Must be called just after creation of container object, before reading/writing/executing any object's members.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Throws exception if container cannot be used due to serious coherence problems.
        /// Can be called to assure container can be used.
        /// </summary>
        void AssertCoherence();

        /// <summary>
        /// Returns collection of all packages contained in container.
        /// </summary>
        /// <returns>Collection of all packages contained in container.</returns>
        IEnumerable<T> GetPackages();
    }
}
