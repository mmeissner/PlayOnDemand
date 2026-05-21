#region Licence
/****************************************************************
 *  Filename: IOpenVrSettingEntityDetails.cs
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
    /// Describes one SteamVR settings file metadata.
    /// </summary>
    
    public interface IOpenVrSettingEntityDetails
    {
        /// <summary>
        /// Base location of file, see <see cref="OpenVrConfigLocation"/>.
        /// </summary>
        OpenVrConfigLocation BaseLocation { get; }

        /// <summary>
        /// Path to file relative to <see cref="BaseLocation"/>.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Type of SteamVR config file, see <see cref="OpenVrConfigFileType"/>.
        /// </summary>
        OpenVrConfigFileType FileType { get; }

        /// <summary>
        /// File specific override of behavior when applying this setting file.
        /// If null then behavior is choosen base on <see cref="FileType"/>.
        /// </summary>
        OpenVrConfigApplyBehavior? BehaviorOverride { get; }
    }
}
