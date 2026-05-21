#region Licence
/****************************************************************
 *  Filename: IContainerPackage.cs
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

using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Extends <see cref="IProgressAwarePackage"/> with ability to read data from package.
    /// </summary>
    
    public interface IContainerPackage : IProgressAwarePackage,IExtractablePackage, IZipablePackage
    {
    }

    /// <summary>
    /// Extends <see cref="IProgressAwarePackage"/> with ability to read data from package.
    /// </summary>
    
    public interface IExtractablePackage
    {
        /// <summary>
        /// Extracts files content of the package (perserving internal package folder structure) to given directory.
        /// </summary>
        /// <param name="directoryPath">Destination directory.</param>
        void ExtractToDirectory(string directoryPath);
    }

    /// <summary>
    /// Extends <see cref="IProgressAwarePackage"/> with ability to read data from package.
    /// </summary>
    
    public interface IZipablePackage
    {
        void CreateZipFromPackage(string filePathName);
    }
}
