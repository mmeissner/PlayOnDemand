#region Licence
/****************************************************************
 *  Filename: UpdateEvent.cs
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
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels {
    public class UpdateEvent
    {
        UpdateEvent(Guid appGuid, UpdateType type)
        {
            ApplicationGuid = appGuid;
            Type = type;
        }
        public Guid ApplicationGuid { get; }
        public UpdateType Type { get; }
        public enum UpdateType
        {
            Installed,
            Uninstalled,
        }

        public static UpdateEvent FromUninstallEvent(IUIAppUninstalledEvent uninstalledEvent)
        {
            return new UpdateEvent(uninstalledEvent.ApplicationGuid, UpdateType.Uninstalled);
        }
        public static UpdateEvent FromInstallEvent(IUIAppInstalledEvent installedEvent)
        {
            return new UpdateEvent(installedEvent.ApplicationGuid, UpdateType.Installed);
        }
    }
}