#region Licence
/****************************************************************
 *  Filename: StationMessage.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.Station
{
    public enum StationMessage
    {
        /// <summary>
        /// Unknown Message as default value to detect unspecified values by devs
        /// </summary>
        Unkown = 0,
        /// <summary>
        /// The Message the Station will send to let all receivers initialize before
        /// the GUI will be loaded or any other critical operation will be executed
        /// </summary>
        InitStart = 10,
        /// <summary>
        /// The Start Message will lead to loadup of the GUI
        /// All mission critical systems needs to be initialized at this point
        /// </summary>
        Start = 20,
        /// <summary>
        /// After the GUI was started
        /// </summary>
        GuiStarted = 25,
        /// <summary>
        /// System Message that informs that the application is going to stop
        /// All mission criticial system components needs to prepare for stop
        /// They need tp close sessions, stop drivers and so on
        /// </summary>
        InitStop = 30,
        /// <summary>
        /// The Stop of the application. All Tasks should now been canceled, I/O Operations stopped
        /// The App should now beeing in an Idle state
        /// </summary>
        Stop = 40,
        /// <summary>
        /// The termination of the app is happening
        /// </summary>
        Quit = 50,
    }

}
