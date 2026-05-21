#region Licence
/****************************************************************
 *  Filename: SingleImageInformationViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-27
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
using System.Reactive.Linq;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Input;
using LeapVR.Shell.Domain.Models.Input.XInput;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using NLog;
using LogManager = NLog.LogManager;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    public class SingleImageInformationViewModel : InputControllerScreen
        , IHandle<IUISessionStopedEvent>
    {
        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public bool EnableGamepadIndicator { get; private set; }

        #endregion

        #region Constructors

        public SingleImageInformationViewModel(IViewInputHandler viewInputHandler, IUIMessageBroker messageBroker,ControllerInput[] closeButtons):base(viewInputHandler, InputExclusivity.AllControllerInputs)
        {
            messageBroker.Subscribe(this);
            EnableGamepadIndicator = viewInputHandler.IsControllerInputEnabled;
            foreach (var input in closeButtons)
            {
                AddControllerInputAction(input, Dismiss);
            }
        }
        #endregion

        #region Methods
        public void Dismiss()
        {
            TryClose(null);
        }

        public void Handle(IUISessionStopedEvent message)
        {
            Dismiss();
        }

        #endregion
    }
}
