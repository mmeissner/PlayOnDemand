#region Licence
/****************************************************************
 *  Filename: MediaContainerException.cs
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
    using System;

    /// <summary>
    /// A Media Container Exception
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class MediaContainerException : Exception
    {
        // TODO: Add error code property and enumerate error codes.

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaContainerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MediaContainerException(string message)
            : base(message)
        {
            // placeholder
        }
    }
}
