#region Licence
/****************************************************************
 *  Filename: LoginIntentionPrepaidViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System.Threading.Tasks;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Shell.Login.ViewModels
{
    public class LoginIntentionPrepaidViewModel : LoginIntentionBaseViewModel
    {
        #region Constructors
        public LoginIntentionPrepaidViewModel(
            ILoginIntention loginIntention,
            IUIMessageBroker messageBroker,
            UiConfig uiconfig,
            IViewInputHandler viewInputHandler) : base(messageBroker,loginIntention,viewInputHandler, uiconfig)
        {
            AddControllerInputAction(ControllerInput.Accept, delegate{ Task.Run(Login);});
            AddControllerInputAction(ControllerInput.Cancel, delegate { Task.Run(Cancel); });
        }
        #endregion
    }
}
