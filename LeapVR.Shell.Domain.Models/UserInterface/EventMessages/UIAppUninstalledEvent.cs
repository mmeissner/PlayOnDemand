#region Licence
/****************************************************************
 *  Filename: UIAppUninstalledEvent.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIAppUninstalledEvent
    {
        Guid ApplicationGuid { get; }
        Guid PlatformGuid { get; }
        AppInstallationType Type { get;  }
        AppUninstallationType UninstallationType { get; }
    }
}
