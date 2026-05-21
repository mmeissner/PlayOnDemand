#region Licence
/****************************************************************
 *  Filename: SteamAppDisplayInfo.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Modules.Platform.Steam
{
    class SteamAppDisplayInfo : IAppDisplayInfo
    {
        public SteamAppDisplayInfo(Guid applicationGuid, string name, IAppCategory category, string description, byte[] thumbnail, bool isSupportScreen, bool isSupportVirtualReality)
        {
            ApplicationGuid = applicationGuid;
            Name = name;
            Category = category;
            Description = description;
            Thumbnail = thumbnail;
            IsSupportScreen = isSupportScreen;
            IsSupportVirtualReality = isSupportVirtualReality;
            Tags = new string[0];
        }
        public Guid ApplicationGuid { get; }
        public string Name { get; }
        public IAppCategory Category { get;  }
        public string[] Tags { get;  }
        public string Description { get;  }
        public byte[] Thumbnail { get;  }
        public bool IsSupportScreen { get;  }
        public bool IsSupportVirtualReality { get;  }
        public event PropertyChangedEventHandler PropertyChanged;
        public IAppDisplayUpdate GetAppDisplayUpdate() { throw new NotImplementedException(); }
    }
}
