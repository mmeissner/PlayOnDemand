#region Licence
/****************************************************************
 *  Filename: UIStationModeChangedEvent.cs
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

using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUIStationModeChangedEvent
    {
        StationMode OldMode { get; }
        StationMode NewMode { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UIStationModeChangedEvent : IUIStationModeChangedEvent
    {

        #region Fields & Properties

        public StationMode OldMode { get; }
        public StationMode NewMode { get; }
        #endregion

        #region Constructors

        public UIStationModeChangedEvent(StationMode oldMode, StationMode newMode)
        {
            OldMode = oldMode;
            NewMode = NewMode;
        }
        #endregion

        #region Methods

        #endregion
    }
}
