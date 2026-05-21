#region Licence
/****************************************************************
 *  Filename: SteamAccount.cs
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
#region Using Directives
using System;
#endregion

namespace LeapVR.Utilities.Steam.Steam.VDF
{
    public class SteamAccount
    {
        public String Name { get; protected set; }
        public String SteamID { get; protected set; }

        public SteamAccount(NestedElement uele)
        {
            Name = uele.Name;
            if (uele.Children.ContainsKey("SteamID"))
            {
                SteamID = uele.Children["SteamID"].Value;
            }
        }
    }
}