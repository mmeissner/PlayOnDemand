#region Licence
/****************************************************************
 *  Filename: UINetworkStateChanged.cs
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
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUINetworkStateChanged
    {
        NetworkConnectionStatus OldStatus { get; }
        NetworkConnectionStatus NewStatus { get; }
    }
    public class UINetworkStateChanged :IUINetworkStateChanged
    {
        public UINetworkStateChanged(NetworkConnectionStatus oldStatus, NetworkConnectionStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
        public NetworkConnectionStatus OldStatus { get; set; }
        public NetworkConnectionStatus NewStatus { get; set; }
    }
}
