#region Licence
/****************************************************************
 *  Filename: UIAppEnabledStateChangedEvent.cs
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

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIAppEnabledStateChangedEvent
    {
        Guid ApplicationGuid { get; }
        Guid PlatformGuid { get; }
        bool IsEnabled { get; }
    }

    public class UIAppEnabledStateChangedEvent : IUIAppEnabledStateChangedEvent
    {
        public UIAppEnabledStateChangedEvent(Guid applicationGuid,Guid platformGuid, bool isEnabled)
        {
            ApplicationGuid = applicationGuid;
            IsEnabled = isEnabled;
            PlatformGuid = platformGuid;
        }

        public Guid ApplicationGuid { get; }
        public Guid PlatformGuid { get; }
        public bool IsEnabled { get; }
    }
}