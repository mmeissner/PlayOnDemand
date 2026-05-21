#region Licence
/****************************************************************
 *  Filename: ConfirmationDialogViewModel.cs
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
using System.Windows;
using Caliburn.Micro;
using NLog;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.Dialog.Views;

namespace LeapVR.Shell.UI.Universal.Dialog.ViewModels
{
    public class ConfirmationDialogViewModel : InputControllerScreen
        , IHandle<IUISessionStopedEvent>
    ,IHandle<IUILanguageChangedEvent>
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IUIMessageBroker _messageBroker;
        private Window _dialogWindow;
        public IDialogContent DisplayContent { get;  }
        public bool IsCancelOnly { get;  }
        public bool EnableGamepadIndicator { get;  }
        #endregion

        #region Constructors
        public ConfirmationDialogViewModel(IUIMessageBroker messageBroker, IViewInputHandler viewInputHandler, IDialogContent displayContent, bool isCancelOnly = false):base(viewInputHandler, InputExclusivity.RegisterdControllerInputs)
        {
            QuickLeap.AssertNotNull(messageBroker, displayContent);
            IsCancelOnly = isCancelOnly;
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            EnableGamepadIndicator = viewInputHandler.IsControllerInputEnabled;
            DisplayContent = displayContent;
            AddControllerInputAction(ControllerInput.Accept,Confirm);
            AddControllerInputAction(ControllerInput.Cancel,Cancel);
        }
        #endregion

        #region Methods
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            if(close) _messageBroker.Unsubscribe(this);
        }

        public void Confirm()
        {
            if (IsCancelOnly)
            {
                return;
            }
            TryClose(true);
        }
        public void Cancel()
        {
            TryClose(false);
        }
        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            if(view is ConfirmationDialogView dialogView && dialogView.Parent is Window dialogWindow)
            {
                _dialogWindow = dialogWindow;
            }

        }
        /// <summary>
        /// Cancel the dialog in case the dashboard get closed before user interacts.
        /// </summary>
        /// <param name="message"></param>
        public void Handle(IUISessionStopedEvent message)
        {
            Cancel();
        }
        #endregion

        public void Handle(IUILanguageChangedEvent message)
        {
            DisplayContent.UpdateContentString();
            CenterLocation();
        }
        private void CenterLocation()
        {
            if(_dialogWindow != null)
            {
                _dialogWindow.UpdateLayout();
                //center this message window w/r to the WorkArea (i.e the screen)
                _dialogWindow.Top  = (_dialogWindow.Owner.Top + _dialogWindow.Owner.ActualHeight / 2) - _dialogWindow.ActualHeight /2 ;
                _dialogWindow.Left = (_dialogWindow.Owner.Left + _dialogWindow.Owner.ActualWidth / 2) - _dialogWindow.ActualWidth /2 ;
            }
            //
        }
    }
}
