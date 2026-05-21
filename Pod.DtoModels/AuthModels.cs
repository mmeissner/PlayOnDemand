#region Licence
/****************************************************************
 *  Filename: AuthModels.cs
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
using System.ComponentModel.DataAnnotations;

namespace Pod.DtoModels
{
    /// <summary>
    /// Login Request Model
    /// </summary>
    public class RequestLoginModelDto
    {
        /// <summary>
        /// The username of the account to login with
        /// </summary>
        [Required, MinLength(8), MaxLength(30)]
        public string Username { get; set; }

        /// <summary>
        /// The password for the Login
        /// </summary>
        [Required, MinLength(10), MaxLength(80)]
        public string Password { get; set; }
    }

    /// <summary>
    /// Refresh Token Request Model
    /// Allows to receive a new access token by an longer lived refresh Token
    /// </summary>
    public class RequestTokenRefreshDto
    {
        [Required, MinLength(240), MaxLength(240)]
        public string RefreshToken { get; set; }
    }
}
