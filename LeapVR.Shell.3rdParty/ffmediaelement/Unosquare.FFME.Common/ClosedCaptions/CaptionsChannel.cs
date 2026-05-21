#region Licence
/****************************************************************
 *  Filename: CaptionsChannel.cs
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
namespace Unosquare.FFME.ClosedCaptions
{
    /// <summary>
    /// Enumerates the 4 different CC channels
    /// </summary>
    public enum CaptionsChannel
    {
        /// <summary>
        /// No channel specified -- use previous
        /// </summary>
        CCP = 0,

        /// <summary>
        /// Field 1, Channel 1
        /// </summary>
        CC1 = 1,

        /// <summary>
        /// Field 1, Channel 2
        /// </summary>
        CC2 = 2,

        /// <summary>
        /// Field 2, Channel 1
        /// </summary>
        CC3 = 3,

        /// <summary>
        /// Field 2, Channel 2
        /// </summary>
        CC4 = 4,
    }
}
