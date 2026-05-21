#region Licence
/****************************************************************
 *  Filename: UISessionUpdatedEvent.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using LeapVR.Shell.Domain.Models.Billing;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUISessionUpdatedEvent
    {
        IUISession Session { get; }
    }

    public class UISessionUpdatedEvent : IUISessionUpdatedEvent
    {
        public UISessionUpdatedEvent(IUISession session) { Session = session; }
        public IUISession Session { get; }
    }
}
