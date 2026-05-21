#region Licence
/****************************************************************
 *  Filename: IPlatform.cs
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
    /// Contains factory methods and properties containing platfrom-specific implementations
    /// of the functionality that is required by an instance of the Media Engine
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// Retrieves the platform-specific Native methods
        /// </summary>
        INativeMethods NativeMethods { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is in debug mode.
        /// </summary>
        bool IsInDebugMode { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is in design time.
        /// </summary>
        bool IsInDesignTime { get; }

        /// <summary>
        /// Creates a renderer of the specified media type.
        /// </summary>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="mediaEngine">The media engine.</param>
        /// <returns>The renderer</returns>
        IMediaRenderer CreateRenderer(MediaType mediaType, MediaEngine mediaEngine);

        /// <summary>
        /// Handles global FFmpeg library messages
        /// </summary>
        /// <param name="message">The message.</param>
        void HandleFFmpegLogMessage(MediaLogMessage message);
    }
}
