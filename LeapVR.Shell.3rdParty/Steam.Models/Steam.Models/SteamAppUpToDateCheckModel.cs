#region Licence
/****************************************************************
 *  Filename: SteamAppUpToDateCheckModel.cs
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
namespace Steam.Models
{
    public class SteamAppUpToDateCheckModel
    {
        public bool Success { get; set; }

        public bool UpToDate { get; set; }

        public bool VersionIsListable { get; set; }

        public int RequiredVersion { get; set; }

        public string Message { get; set; }
    }
}