#region Licence
/****************************************************************
 *  Filename: UIAdminAccessDismissEvent.cs
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

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIAdminAccessDismissEvent
    {
        ViewDismissReason DismissReason { get; }
    }

    /// <summary>
    /// Representing a message when access to administration is cancelled.
    /// </summary>
    public class UIAdminAccessDismissEvent : IUIAdminAccessDismissEvent
    {

        #region Fields & Properties

        public ViewDismissReason DismissReason { get; }
        #endregion

        #region Constructors

        public UIAdminAccessDismissEvent(ViewDismissReason viewDismissReason)
        {
            DismissReason = viewDismissReason;
        }
        #endregion

        #region Methods

        #endregion
    }
}
