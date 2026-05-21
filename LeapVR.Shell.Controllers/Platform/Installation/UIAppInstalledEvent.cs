#region Licence
/****************************************************************
 *  Filename: UIAppInstalledEvent.cs
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
    
    class UIAppInstalledEvent : IUIAppInstalledEvent
    {
        #region Fields & Properties
        public Guid ApplicationGuid { get; }
        public Guid PlatformGuid { get; }
        public AppInstallationType Type { get; }
        #endregion

        #region Constructors
        internal UIAppInstalledEvent(InstallProcessData installProcessData)
        {
            ApplicationGuid = installProcessData.InstallationData.ApplicationGuid;
            PlatformGuid = installProcessData.InstallationData.PlatformPluginGuid;
            Type = installProcessData.InstallationData.Type;
        }
        #endregion

        public override string ToString()
        {
            return $"{nameof(ApplicationGuid)} = {ApplicationGuid}{Environment.NewLine}" +
                   $"{nameof(PlatformGuid)} = {PlatformGuid}{Environment.NewLine}" +
                   $"{nameof(Type)} = {Type}";
        }
    }
}