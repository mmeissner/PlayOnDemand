#region Licence
/****************************************************************
 *  Filename: ConfigShellServer.cs
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

namespace Pod.Data.Config
{
    /// <summary>
    /// Configuration for <see cref="IDbSetupTask"/> to create a Shell Server
    /// </summary>
    public class ConfigShellServer
    {
        public string DisplayName { get; set; }
        public string HostAddress { get; set; }
        public uint Port { get; set; }
        public uint InterfaceVersion { get; set; }
    }
}
