#region Licence
/****************************************************************
 *  Filename: UIConnectDialogEvent.cs
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
using LeapVR.Shell.Domain.Models.Controllers;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIConnectDialogEvent
    {
        IRemoteServiceController Controller { get; }
        bool AutoConnect { get; }
    }
    
    public class UIConnectDialogEvent : IUIConnectDialogEvent
    {
        public UIConnectDialogEvent(IRemoteServiceController controller, bool autoConnect)
        {
            Controller = controller;
            AutoConnect = autoConnect;
        }
        public IRemoteServiceController Controller { get; }
        public bool AutoConnect { get;}
    }
}
