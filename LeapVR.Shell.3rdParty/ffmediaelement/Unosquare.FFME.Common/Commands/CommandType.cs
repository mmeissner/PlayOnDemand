#region Licence
/****************************************************************
 *  Filename: CommandType.cs
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
namespace Unosquare.FFME.Commands
{
    /// <summary>
    /// Enumerates the different available Media Command Types
    /// </summary>
    internal enum CommandType
    {
        /// <summary>
        /// The open command id
        /// </summary>
        Open,

        /// <summary>
        /// The seek command id
        /// </summary>
        Seek,

        /// <summary>
        /// The play command id
        /// </summary>
        Play,

        /// <summary>
        /// The pause command id
        /// </summary>
        Pause,

        /// <summary>
        /// The stop command id
        /// </summary>
        Stop,

        /// <summary>
        /// The close command id
        /// </summary>
        Close,

        /// <summary>
        /// The set speed ratio command id
        /// </summary>
        SpeedRatio,

        /// <summary>
        /// The change media command id
        /// </summary>
        ChangeMedia,
    }
}
