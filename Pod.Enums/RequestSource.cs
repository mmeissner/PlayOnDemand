#region Licence
/****************************************************************
 *  Filename: RequestSource.cs
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
namespace Pod.Enums 
{
    /// <summary>
    /// Available sources that can request a new session
    /// </summary>
    public enum RequestSource
    {
        /// <summary>
        /// Invalid Value
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// When the request was issued by a Client 
        /// </summary>
        ShellClient = 10,
        /// <summary>
        /// When the request was issued through an API
        /// </summary>
        WebApi = 20
    }
}