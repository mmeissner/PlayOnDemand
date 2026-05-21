#region Licence
/****************************************************************
 *  Filename: AdminModels.cs
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
    public class RequestAddRemoveUserToRoleDto
    {
        [Required, MinLength(10), MaxLength(30)]
        public string Username { get; set; }
        [Required]
        public string Role { get; set; }
    }

    public class RequestSetSystemSettings
    {
        public bool UserRegistrationEnabled { get; set; }
        public int MaxStationsPerUser { get; set; }
    }
}
