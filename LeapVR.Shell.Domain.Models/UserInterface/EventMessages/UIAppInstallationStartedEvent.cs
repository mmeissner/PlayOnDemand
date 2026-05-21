#region Licence
/****************************************************************
 *  Filename: UIAppInstallationStartedEvent.cs
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
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIAppInstallationStartedEvent
    {
        IInstallationProcessInfo ProcessInfo { get; }
    }

    public interface IUIAppUninstallationStartedEvent
    {
        IUninstallationProcessInfo ProcessInfo { get; }
    }

    public class UIAppUninstallationStartedEvent : IUIAppUninstallationStartedEvent
    {
        public UIAppUninstallationStartedEvent(IUninstallationProcessInfo processInfo)
        {
            ProcessInfo = processInfo;
        }
        public IUninstallationProcessInfo ProcessInfo { get; }
    }

    public class UIAppInstallationStartedEvent : IUIAppInstallationStartedEvent
    {
        public UIAppInstallationStartedEvent(IInstallationProcessInfo processInfo)
        {
            ProcessInfo = processInfo;
        }
        public IInstallationProcessInfo ProcessInfo { get; }
    }
}
