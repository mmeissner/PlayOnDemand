#region Licence
/****************************************************************
 *  Filename: DotaEnumType.cs
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
namespace Steam.Models.DOTA2
{
    public abstract class DotaEnumType
    {
        protected readonly string key;
        protected readonly string displayName;
        protected readonly string description;

        public DotaEnumType(string key, string displayName, string description)
        {
            this.key = key;
            this.displayName = displayName;
            this.description = description;
        }

        public string Key { get { return key; } }

        public override string ToString()
        {
            return displayName;
        }
    }
}