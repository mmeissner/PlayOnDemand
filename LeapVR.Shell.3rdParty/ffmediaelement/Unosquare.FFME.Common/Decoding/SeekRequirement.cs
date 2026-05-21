#region Licence
/****************************************************************
 *  Filename: SeekRequirement.cs
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
namespace Unosquare.FFME.Decoding
{
    /// <summary>
    /// Enumerates the seek target requirement levels.
    /// </summary>
    internal enum SeekRequirement
    {
        /// <summary>
        /// Seek requirement is satisfied when
        /// the main component has frames in the seek range.
        /// This is the fastest option.
        /// </summary>
        MainComponentOnly,

        /// <summary>
        /// Seek requirement is satisfied when
        /// the both audio and video comps have frames in the seek range.
        /// This is the recommended option.
        /// </summary>
        AudioAndVideo,

        /// <summary>
        /// Seek requirement is satisfied when
        /// ALL components have frames in the seek range
        /// This is NOT recommended as it forces large amounts of
        /// frames to get decoded in subtitle files.
        /// </summary>
        AllComponents,
    }
}
