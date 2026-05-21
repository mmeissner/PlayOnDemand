#region Licence
/****************************************************************
 *  Filename: SubtitleBlock.cs
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
    using System.Collections.Generic;

    /// <summary>
    /// A subtitle frame container. Simply contains text lines.
    /// </summary>
    public sealed class SubtitleBlock : MediaBlock
    {
        #region Properties

        /// <summary>
        /// Gets the media type of the data
        /// </summary>
        public override MediaType MediaType => MediaType.Subtitle;

        /// <summary>
        /// Gets the lines of text for this subtitle frame with all formatting stripped out.
        /// </summary>
        public List<string> Text { get; } = new List<string>(16);

        /// <summary>
        /// Gets the original text in SRT or ASS fromat.
        /// </summary>
        public List<string> OriginalText { get; } = new List<string>(16);

        /// <summary>
        /// Gets the type of the original text.
        /// Returns None when it's a bitmap or when it's None
        /// </summary>
        public AVSubtitleType OriginalTextType { get; internal set; }

        #endregion
    }
}
