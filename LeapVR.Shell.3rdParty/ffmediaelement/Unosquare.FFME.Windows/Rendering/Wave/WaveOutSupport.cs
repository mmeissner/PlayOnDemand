#region Licence
/****************************************************************
 *  Filename: WaveOutSupport.cs
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
    /// Flags indicating what features this WaveOut device supports
    /// </summary>
    [Flags]
    internal enum WaveOutSupport
    {
        /// <summary>supports pitch control (WAVECAPS_PITCH)</summary>
        Pitch = 0x0001,

        /// <summary>supports playback rate control (WAVECAPS_PLAYBACKRATE)</summary>
        PlaybackRate = 0x0002,

        /// <summary>supports volume control (WAVECAPS_VOLUME)</summary>
        Volume = 0x0004,

        /// <summary>supports separate left-right volume control (WAVECAPS_LRVOLUME)</summary>
        LRVolume = 0x0008,

        /// <summary>(WAVECAPS_SYNC)</summary>
        Sync = 0x0010,

        /// <summary>(WAVECAPS_SAMPLEACCURATE)</summary>
        SampleAccurate = 0x0020,
    }
}
