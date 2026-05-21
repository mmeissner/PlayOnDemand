#region Licence
/****************************************************************
 *  Filename: GamepadController.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using System.Diagnostics;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Input.XInput;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Modules.Interfaces.XInput;
using NLog;

namespace LeapVR.Shell.Controllers.GamePad
{
    public class GamepadController: IGamepadController, IRunLevelMsgReceiver
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IXInputModule _xInputModule;

        public IObservable<XInputButtonArgs> WhenXInputButtonStateChanged { get; }

        //UNDONE Should take an IEnumerable of diffrent Input types that can call actions, currently just moved out to not polute StationController
        //Currently it's tightly bound to XInput and needs urgend refactoring as its current design is clusterfuck
        public GamepadController(IXInputModule xInputModule)
        {
            _xInputModule = xInputModule;
            WhenXInputButtonStateChanged = _xInputModule.WhenXInputButtonStateChanged;
        }

        public bool IsEnabled => _xInputModule.Enabled;
        public XInputButtons ConfirmButton => XInputButtons.A;
        public XInputButtons CancelButton => XInputButtons.B;
        public void RegisterTerminateAllApplications(Action terminationAction)
        {
            Logger.Debug("Register Terminate Action for XInput Module");
            _xInputModule.RequestAllAppTermination = terminationAction;
        }

        public void RegisterResetStationState(Action resetStationAction)
        {
            Logger.Debug("Register Reset Station State Action for XInput Module");
            _xInputModule.ResetStationState = resetStationAction;
        }

        public void OnStationMessage(StationMessage messages)
        {
            switch (messages)
            {
                case StationMessage.InitStart:
                    Logger.Debug("Received Init Start Message");
                    if(_xInputModule != null)
                    {
                        Logger.Debug("XInput Module is going to be enabled");
                        _xInputModule.Enabled = true;
                    }
                    break;
            }
        }
    }
}