#region Licence
/****************************************************************
 *  Filename: SelectableVrType.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.Controllers.VirtualReality
{
    class SelectableVrType : ISelectableVrType
    {
        internal readonly Guid ModuleId;
        public bool IsNonVrType { get; }
        public string DisplayName { get; }
        public string ToVrModuleGuidString()
        {
            if(ModuleId.Equals(Guid.Empty)) return null;
            return ModuleId.ToString();
        }
        public bool IsMatch(string guid)
        {
            if(String.IsNullOrWhiteSpace(guid))
            {
                if(IsNonVrType) return true;
                return false;
            }
            if(Guid.TryParse(guid, out var resultGuid) && resultGuid.Equals(ModuleId)) return true;
            return false;
        }

        public SelectableVrType()
        {
            IsNonVrType = true;
            ModuleId = Guid.Empty;
            DisplayName = "None";
        }

        public SelectableVrType(IVrModule vrModule)
        {
            IsNonVrType = false;
            ModuleId = vrModule.ModuleId;
            DisplayName = vrModule.DisplayName;
        }

        public SelectableVrType(Guid vrModuleId)
        {
            IsNonVrType = false;
            ModuleId = vrModuleId;
            DisplayName = vrModuleId.ToString();
        }

    }
}
