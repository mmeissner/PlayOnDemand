#region Licence
/****************************************************************
 *  Filename: PlaybackStatus.cs
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
namespace Unosquare.FFME.Shared
{
    /// <summary>
    /// Media States compatible with MediaState enumeration
    /// </summary>
    public enum PlaybackStatus
    {
        /// <summary>
        /// The manual status
        /// </summary>
        Manual = 0,

        /// <summary>
        /// The play status
        /// </summary>
        Play = 1,

        /// <summary>
        /// The close status
        /// </summary>
        Close = 2,

        /// <summary>
        /// The pause status
        /// </summary>
        Pause = 3,

        /// <summary>
        /// The stop status
        /// </summary>
        Stop = 4
    }
}
