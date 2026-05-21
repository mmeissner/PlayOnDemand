#region Licence
/****************************************************************
 *  Filename: PlaybackState.cs
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
    /// <summary>
    /// Enumerates the various wave output playback states
    /// </summary>
    internal enum PlaybackState
    {
        /// <summary>
        /// Stopped
        /// </summary>
        Stopped,

        /// <summary>
        /// Playing
        /// </summary>
        Playing,
    }
}
