#region Licence
/****************************************************************
 *  Filename: UIAppUninstalledEvent.cs
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Controllers.Platform.Installation {
    internal class UIAppUninstalledEvent : IUIAppUninstalledEvent
    {
        #region Fields & Properties
        public Guid ApplicationGuid { get;}
        public Guid PlatformGuid { get;}
        public AppInstallationType Type { get;}
        public AppUninstallationType UninstallationType { get; }
        #endregion

        #region Constructors

        internal UIAppUninstalledEvent(UninstallProcessData processData)
        {
            ApplicationGuid = processData.ApplicationGuid;
            PlatformGuid = processData.PlatformGuid;
            Type = processData.Type;
            UninstallationType = processData.UninstallationType;
        }
        #endregion

        #region Methods

        #endregion
    }
}