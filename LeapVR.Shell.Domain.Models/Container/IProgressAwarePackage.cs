#region Licence
/****************************************************************
 *  Filename: IProgressAwarePackage.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-2-27
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
using System.Reflection;
using LeapVR.Shared.Lib;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Extends <see cref="IPackageData"/> to include progress (e.g. install, package creation and so on) releated data.
    /// </summary>
    
    public interface IProgressAwarePackage : IPackageData
    {
        /// <summary>
        /// Indicates if operation of package creation was already requested.
        /// </summary>
        bool WasPackageOperationStarted { get; }

        /// <summary>
        /// Indicates if operation of package creation has ended.
        /// </summary>
        bool IsPackageOperationEnded { get; }

        /// <summary>
        /// Indicates amount of files in package that were already asynchronously processed succesfully.
        /// </summary>
        int DoneFilesCount { get; }

        /// <summary>
        /// Indicates size of files in package that were already asynchronously processed succesfully.
        /// </summary>
        long DoneFilesSize { get; }

        /// <summary>
        /// Indicates exception occured in asynchronous process of dealing (e.g. install, package creation and so on) with package. Set after creation process finishes. Not null indicates failure.
        /// </summary>
        Exception PackageOperationException { get; }

        /// <summary>
        /// Emit new instance of <see cref="PackageProgress"/> when package read progress changed.
        /// When subscribed afterwards all notifications will be pushed imiedietly (like ReplaySubject).
        /// </summary>
        IObservable<PackageProgress> WhenPackageProgressChanged { get; }
    }
}
