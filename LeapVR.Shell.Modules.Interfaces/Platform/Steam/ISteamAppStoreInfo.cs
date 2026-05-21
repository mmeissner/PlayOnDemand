#region Licence
/****************************************************************
 *  Filename: ISteamAppStoreInfo.cs
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

namespace LeapVR.Shell.Modules.Interfaces.Platform.Steam {
    public interface ISteamAppStoreInfo {
        UInt32 AppId { get; }
        string Title { get; }
        Byte[] Image { get; }
        string Description { get; }
        HashSet<string> Categories { get; }
        List<string> MovieUrls { get; }
    }
}