#region Licence
/****************************************************************
 *  Filename: Wmi.cs
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
using System.Collections.Generic;
using System.Management;

namespace LeapVR.Shared.Lib.Win
{
    public static class Wmi
    {
        public static List<ManagementBaseObject> Query(string query)
        {
            var result = new List<ManagementBaseObject>();

            var sq = new SelectQuery(query);
            var searcher = new ManagementObjectSearcher(sq);
            foreach (var obj in searcher.Get())
            {
                result.Add(obj);
            }
            return result;
        }
    }
}
