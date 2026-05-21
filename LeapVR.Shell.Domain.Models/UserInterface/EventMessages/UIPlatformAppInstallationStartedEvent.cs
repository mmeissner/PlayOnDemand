#region Licence
/****************************************************************
 *  Filename: UIPlatformAppInstallationStartedEvent.cs
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
using LeapVR.Shell.Domain.Models.Platform;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public class UIPlatformAppInstallationStartedEvent : IUIPlatformAppInstallationStartedEvent
    {
        public UIPlatformAppInstallationStartedEvent(IPlatformInstallationProcessInfo processInfo)
        {
            ProcessInfo = processInfo;
        }
        public IPlatformInstallationProcessInfo ProcessInfo { get; }
    }
    public interface IUIPlatformAppInstallationStartedEvent
    {
        IPlatformInstallationProcessInfo ProcessInfo { get; }
    }
}
