#region Licence
/****************************************************************
 *  Filename: SteamIdNotConstructedException.cs
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
using System;

namespace SteamWebAPI2.Exceptions
{
    /// <summary>
    /// Represents an exception that has been thrown as a result of all parsing options failing to work with a given Steam ID.
    /// </summary>
    public class SteamIdNotConstructedException : Exception
    {
        public SteamIdNotConstructedException()
        {
        }

        public SteamIdNotConstructedException(string message) : base(message)
        {
        }

        public SteamIdNotConstructedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}