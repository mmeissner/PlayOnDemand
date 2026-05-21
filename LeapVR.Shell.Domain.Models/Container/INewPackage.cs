#region Licence
/****************************************************************
 *  Filename: INewPackage.cs
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
    /// Interface used to edit files content of newly created package.
    /// </summary>
    
    public interface INewPackage : IProgressAwarePackage
    {
        /// <summary>
        /// Version of package. Reserved for further use.
        /// </summary>
        new uint PackageVersion { get; set; }

        /// <summary>
        /// Specifies file to be added to new package.
        /// </summary>
        /// <param name="fullFilePath">Full path to file to be added</param>
        /// <param name="packageRelativePath">Relative path inside of package where file will be placed at</param>
        void AddFile(string fullFilePath, string packageRelativePath);

        /// <summary>
        /// Spefifies directory to be added to new package.
        /// </summary>
        /// <param name="fullDirectoryPath">Full path to directory to be added</param>
        /// <param name="packageRelativePath">Relative path inside of package where directory will be placed at</param>
        void AddDirectory(string fullDirectoryPath, string packageRelativePath);
    }
}
