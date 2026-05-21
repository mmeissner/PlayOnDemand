#region Licence
/****************************************************************
 *  Filename: IWavePlayer.cs
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
    /// Represents the interface to a device that can play a Wave data
    /// </summary>
    internal interface IWavePlayer : IDisposable
    {
        /// <summary>
        /// Current playback state
        /// </summary>
        PlaybackState PlaybackState { get; }

        /// <summary>
        /// Gets a value indicating whether the audio playback is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets or sets the desired latency in milliseconds
        /// Should be set before a call to Init
        /// </summary>
        int DesiredLatency { get; }

        /// <summary>
        /// Gets the renderer that owns this wave player.
        /// </summary>
        AudioRenderer Renderer { get; }

        /// <summary>
        /// Begin playback
        /// </summary>
        void Start();
    }
}
