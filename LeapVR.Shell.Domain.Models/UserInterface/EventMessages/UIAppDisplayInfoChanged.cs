#region Licence
/****************************************************************
 *  Filename: UIAppDisplayInfoChanged.cs
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
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIAppDisplayInfoChanged
    {
        IAppDisplayInfo DisplayInfo { get; }
        IAppDisplayUpdateInfo UpdateInfo { get; }
    }

    public class UIAppDisplayInfoChanged : IUIAppDisplayInfoChanged
    {
        public UIAppDisplayInfoChanged(IAppDisplayInfo displayInfo, IAppDisplayUpdateInfo displayUpdateInfo)
        {
            DisplayInfo = displayInfo;
            UpdateInfo = displayUpdateInfo;
        }
        public IAppDisplayInfo DisplayInfo { get; }
        public IAppDisplayUpdateInfo UpdateInfo { get; }
    }
}
