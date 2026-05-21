#region Licence
/****************************************************************
 *  Filename: UILoginDecisionResultEvent.cs
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
    public interface IUILoginDecisionResultEvent
    {
        LoginDecisionResultType Decision { get; }
    }

    /// <summary>
    /// Representing a message when a login intention is confirmed.
    /// </summary>
    public class UILoginDecisionResultEvent : IUILoginDecisionResultEvent
    {
        #region Fields & Properties
        public LoginDecisionResultType Decision { get; }
        #endregion

        #region Constructors
        public UILoginDecisionResultEvent(LoginDecisionResultType decision)
        {
            Decision = decision;
        }
        #endregion

        #region Methods

        #endregion


    }
}
