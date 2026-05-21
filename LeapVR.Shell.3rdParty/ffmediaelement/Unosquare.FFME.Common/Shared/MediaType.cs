#region Licence
/****************************************************************
 *  Filename: MediaType.cs
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
    using FFmpeg.AutoGen;

    /// <summary>
    /// Enumerates the different Media Types compatible with AVMEDIATYPE_* constants
    /// defined by FFmpeg
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// Represents an unexisting media type (-1)
        /// </summary>
        None = AVMediaType.AVMEDIA_TYPE_UNKNOWN,

        /// <summary>
        /// The video media type (0)
        /// </summary>
        Video = AVMediaType.AVMEDIA_TYPE_VIDEO,

        /// <summary>
        /// The audio media type (1)
        /// </summary>
        Audio = AVMediaType.AVMEDIA_TYPE_AUDIO,

        /// <summary>
        /// The subtitle media type (3)
        /// </summary>
        Subtitle = AVMediaType.AVMEDIA_TYPE_SUBTITLE,
    }
}
