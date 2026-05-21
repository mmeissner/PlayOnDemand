#region Licence
/****************************************************************
 *  Filename: IUIConnectionStateChangedEvent.cs
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
    public interface IUIConnectionStateChangedEvent
    {
        ConnectState PreviousState { get; }
        ConnectState CurrentState { get; }
    }

    public class UIConnectionStateChangedEvent : IUIConnectionStateChangedEvent
    {
        public UIConnectionStateChangedEvent(ConnectState previousState, ConnectState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public ConnectState PreviousState { get; set; }
        public ConnectState CurrentState { get; set; }
    }
}
