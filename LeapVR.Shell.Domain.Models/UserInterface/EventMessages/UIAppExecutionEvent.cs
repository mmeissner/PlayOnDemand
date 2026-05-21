#region Licence
/****************************************************************
 *  Filename: UIAppExecutionEvent.cs
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

using System;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIAppExecutionEvent
    {
        Guid ApplicationGuid { get; }
        IAppDisplayInfo DisplayInfo { get; }
        UIApplicationExecutionPhase ExecutionPhase { get; }
        TerminationReason[] TerminationReasons { get; }
    }

    public class UIAppExecutionEvent : IUIAppExecutionEvent
    {
        #region Fields & Properties

        public Guid ApplicationGuid { get; }
        public IAppDisplayInfo DisplayInfo { get; }
        public UIApplicationExecutionPhase ExecutionPhase { get; }
        public TerminationReason[] TerminationReasons { get; }
        #endregion

        #region Constructors

        public UIAppExecutionEvent(Guid applicationGuid,IAppDisplayInfo displayInfo, UIApplicationExecutionPhase phase, params TerminationReason[] terminationReason)
        {
            ApplicationGuid = applicationGuid;
            ExecutionPhase = phase;
            TerminationReasons = terminationReason;
            DisplayInfo = displayInfo;
        }
        #endregion

        #region Methods

        #endregion
    }
}
