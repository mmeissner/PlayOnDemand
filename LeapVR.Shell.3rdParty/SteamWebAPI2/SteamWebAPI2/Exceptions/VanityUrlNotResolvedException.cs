#region Licence
/****************************************************************
 *  Filename: VanityUrlNotResolvedException.cs
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
    /// Represents an exception that has been thrown as a result of using the Steam Web API to resolve a vanity url only to have the response indicate "no match".
    /// </summary>
    public class VanityUrlNotResolvedException : Exception
    {
        public VanityUrlNotResolvedException()
        {
        }

        public VanityUrlNotResolvedException(string message) : base(message)
        {
        }

        public VanityUrlNotResolvedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}