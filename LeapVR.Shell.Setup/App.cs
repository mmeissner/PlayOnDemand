#region Licence
/****************************************************************
 *  Filename: App.cs
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

namespace LeapVR.Shell.Setup
{
    public partial class App 
    {
        public App(SetupType type) { Type = type; }
        public SetupType Type { get;  }
    }

    public enum SetupType
    {
        Config,
        Uninstall
    }
}
