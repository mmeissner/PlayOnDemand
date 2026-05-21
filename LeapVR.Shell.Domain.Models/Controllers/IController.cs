#region Licence
/****************************************************************
 *  Filename: IController.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-7-14
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

namespace LeapVR.Shell.Domain.Models.Controllers
{
    /// <summary>
    /// Abstract mark for concrete controller entities.
    /// </summary>
    public interface IController { }
    
    public interface IRunLevelMsgReceiver
    {
        void OnStationMessage(StationMessage messages);
    }
}
