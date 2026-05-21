#region Licence
/****************************************************************
 *  Filename: AppDisplayDataDto.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using ProtoBuf;

namespace LeapVR.Content.Shared.Container
{
    [ProtoContract]

    public class AppDisplayDataDto : IAppDisplayDataDto
    {
        [ProtoMember(1)]
        public Guid ApplicationGuid { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
        [ProtoMember(3)]
        public string Description { get; set; }
        [ProtoMember(4)]
        public string Category { get; set; }
        [ProtoMember(5)]
        public IDiskEntityDto MainPicture { get; set; }
        [ProtoMember(6)]
        public string[] Tags { get; set; }

    }
}
