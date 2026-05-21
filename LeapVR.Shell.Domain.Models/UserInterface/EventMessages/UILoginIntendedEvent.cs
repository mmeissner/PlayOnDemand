#region Licence
/****************************************************************
 *  Filename: UILoginIntendedEvent.cs
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

using LeapVR.Shell.Domain.Models.Authentication;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUILoginIntendedEvent
    {
        ILoginIntention Intention { get; }
    }

    public class UILoginIntendedEvent : IUILoginIntendedEvent
    {
        #region Fields & Properties
        public ILoginIntention Intention { get; }
        #endregion

        #region Constructors

        public UILoginIntendedEvent(ILoginIntention loginIntention)
        {
            Intention = loginIntention;

        }
        #endregion

        #region Methods

        #endregion
    }
}
