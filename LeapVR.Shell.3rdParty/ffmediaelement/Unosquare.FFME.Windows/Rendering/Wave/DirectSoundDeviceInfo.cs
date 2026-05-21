#region Licence
/****************************************************************
 *  Filename: DirectSoundDeviceInfo.cs
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
namespace Unosquare.FFME.Rendering.Wave
{
    using System;

    /// <summary>
    /// Class for enumerating DirectSound devices
    /// </summary>
    internal class DirectSoundDeviceInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectSoundDeviceInfo"/> class.
        /// </summary>
        internal DirectSoundDeviceInfo()
        {
            // placeholder
        }

        /// <summary>
        /// The device identifier
        /// </summary>
        public Guid Guid { get; internal set; }

        /// <summary>
        /// Device description
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Device module name
        /// </summary>
        public string ModuleName { get; internal set; }
    }
}
