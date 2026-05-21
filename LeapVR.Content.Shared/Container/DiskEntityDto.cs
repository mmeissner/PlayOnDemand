#region Licence
/****************************************************************
 *  Filename: DiskEntityDto.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using ProtoBuf;

namespace LeapVR.Content.Shared.Container
{
    class DiskEntityDto : IDiskEntityDto
    {
        public Guid ApplicationGuid { get; set; }
        public Guid PackageGuid { get; set; }
        public string RelativePath { get; set; }
    }
}
