#region Licence
/****************************************************************
 *  Filename: Server.cs
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
using System.Text;

namespace Pod.DtoModels
{

    public class RequestNewServerDto
    {
        public string DisplayName { get; set; }
        public string HostAddress { get; set; }
        public uint HostPort { get; set; }
        public uint InterfaceVersion { get; set; }
    }
    public class RequestServerDisplayNameUpdateDto
    {
        public string DisplayName { get; set; }
    }

    public class RequestServerTimeSettingsUpdateDto
    {
        public TimeSpan? HeartbeatInterval { get; set; } 
        public TimeSpan? HeartbeatTimeout { get; set; }
        public TimeSpan? ConnectTimeout { get; set; }
    }

    public class RequestServerConnectionSettingsUpdateDto
    {
        public string HostAddress { get; set; }
        public uint HostPort { get; set; }
        public uint InterfaceVersion { get; set; }
    }
}
