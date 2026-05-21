#region Licence
/****************************************************************
 *  Filename: IOpenVrSettingFile.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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

namespace LeapVR.Shell.Modules.Interfaces.Vr
{
    /// <summary>
    /// Describes one actual file, with it's <see cref="IOpenVrSettingEntityDetails"/> metadata and <see cref="FileContent"/>.
    /// </summary>
    
    public interface IOpenVrSettingFile
    {
        /// <summary>
        /// Metadata of this config file.
        /// </summary>
        IOpenVrSettingEntityDetails EntityDetails { get; }

        /// <summary>
        /// Actual file content.
        /// </summary>
        string FileContent { get; }
    }
}
