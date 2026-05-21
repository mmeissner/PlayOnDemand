#region Licence
/****************************************************************
 *  Filename: CaptionsPacketType.cs
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
    /// Defines Closed-Captioning Packet types
    /// </summary>
    public enum CaptionsPacketType
    {
        /// <summary>
        /// The unrecognized packet type
        /// </summary>
        Unrecognized,

        /// <summary>
        /// The null pad packet type
        /// </summary>
        NullPad,

        /// <summary>
        /// The XDS class packet type
        /// </summary>
        XdsClass,

        /// <summary>
        /// The misc command packet type
        /// </summary>
        Command,

        /// <summary>
        /// The text packet type
        /// </summary>
        Text,

        /// <summary>
        /// The mid row packet type
        /// </summary>
        MidRow,

        /// <summary>
        /// The preamble packet type
        /// </summary>
        Preamble,

        /// <summary>
        /// The color packet type
        /// </summary>
        Color,

        /// <summary>
        /// The charset packet type
        /// </summary>
        PrivateCharset,

        /// <summary>
        /// The tabs packet type
        /// Section B.4 Tab Offsets
        /// </summary>
        Tabs,
    }
}
