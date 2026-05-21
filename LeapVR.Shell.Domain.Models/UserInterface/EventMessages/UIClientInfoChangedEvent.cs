#region Licence
/****************************************************************
 *  Filename: UIClientInfoChangedEvent.cs
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
    public interface IUIClientInfoChangedEvent
    {
        IShellClientInfo ClientInfo { get; }
    }

    public class UIClientInfoChangedEvent : IUIClientInfoChangedEvent
    {
        public UIClientInfoChangedEvent(IShellClientInfo shellClientInfo)
        {
            ClientInfo = shellClientInfo;
        }
        public IShellClientInfo ClientInfo { get; }
    }
}
