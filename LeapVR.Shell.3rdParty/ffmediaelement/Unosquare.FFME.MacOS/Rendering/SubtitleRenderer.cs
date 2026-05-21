#region Licence
/****************************************************************
 *  Filename: SubtitleRenderer.cs
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
namespace Unosquare.FFME.MacOS.Rendering
{
    using System;
    using Unosquare.FFME.Shared;

    /// <summary>
    /// Subtitle Renderer - Does nothing at this point.
    /// </summary>
    class SubtitleRenderer : IMediaRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Unosquare.FFME.MacOS.Rendering.SubtitleRenderer"/> class.
        /// </summary>
        /// <param name="mediaEngine">Media element core.</param>
        public SubtitleRenderer(MediaEngine mediaEngine)
        {
            MediaCore = mediaEngine;
        }

        /// <summary>
        /// Gets the media element core player component.
        /// </summary>
        /// <value>The media element core.</value>
        public MediaEngine MediaCore { get; }

        public void Close()
        {
        }

        public void Pause()
        {
        }

        public void Play()
        {
        }

        public void Render(MediaBlock mediaBlock, TimeSpan clockPosition)
        {
        }

        public void Seek()
        {
        }

        public void Stop()
        {
        }

        public void Update(TimeSpan clockPosition)
        {
        }

        public void WaitForReadyState()
        {
        }
    }
}
