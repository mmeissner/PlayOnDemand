#region Licence
/****************************************************************
 *  Filename: Enums.cs
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
    /// Specifies one of possible locations for SteamVR config files.
    /// </summary>
    public enum OpenVrConfigLocation
    {
        Unknown = 0,

        OpenVrConfigDirectory = 1,
        SteamDirectory = 2,
        OpenVrDirectory = 3,
    }
    /// <summary>
    /// Specifies type of SteamVR config files.
    /// </summary>
    public enum OpenVrConfigFileType
    {
        Unknown = 0,

        Other = 1,
        Json = 2,
    }

    /// <summary>
    /// Specifies desired bahavior when applying new config over current one.
    /// </summary>
    public enum OpenVrConfigApplyBehavior
    {
        Unknown = 0,

        /// <summary>
        /// Replaces old or creates new file with desired content. Old file content is deleted.
        /// </summary>
        ReplaceFile = 1,

        /// <summary>
        /// Replaces contradicting parts of file with new file content, leaving old non-contradicting parts not touched.
        /// </summary>
        Override = 2,
    }

    /// <summary>
    /// Specifies HMD activity status fetched from OpenVR API.
    /// </summary>
    public enum HmdActivityStatus
    {
        Unknown = 0,

        /// <summary>
        /// Indicates that HMD is currently in use by the user.
        /// </summary>
        Active = 1,

        /// <summary>
        /// Indicates that HMD is not currently in use by the user and it entered Idle mode (display is turned off).
        /// </summary>
        Inactive = 2,
    }
    public enum ControllerSide
    {
        Unknown = 0,
        LeftHand = 1,
        RightHand = 2,
    }
    public enum ControllerButtonAction
    {
        Unknown = 0,
        Press = 1,
        UnPress = 2,
        Touch = 3,
        UnTouch = 4,
    }
    public enum ControllerButton
    {
        Unknown = 0,
        Touchpad = 1,
        Trigger = 2,
        ApplicationMenu = 3,
        Grip = 4,
    }
    public enum VrModuleState
    {
        None,
        Started,
        Stopped
    }
    public enum VrGuiState
    {
        None,
        Started,
        Stopped
    }
}
