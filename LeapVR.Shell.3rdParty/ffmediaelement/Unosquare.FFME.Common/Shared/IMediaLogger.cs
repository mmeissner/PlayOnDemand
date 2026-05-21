#region Licence
/****************************************************************
 *  Filename: IMediaLogger.cs
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
    /// A very simple and standard interface for message logging
    /// </summary>
    internal interface IMediaLogger
    {
        /// <summary>
        /// Logs the specified message of the given type.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="message">The message.</param>
        void Log(MediaLogMessageType messageType, string message);
    }
}
