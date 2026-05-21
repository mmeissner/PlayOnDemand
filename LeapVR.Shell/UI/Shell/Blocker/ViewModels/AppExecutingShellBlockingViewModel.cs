#region Licence
/****************************************************************
 *  Filename: AppExecutingShellBlockingViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-9-7
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
using System.Threading;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.UserInterface;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Blocker.Abstract;
using NLog;

namespace LeapVR.Shell.UI.Shell.Blocker.ViewModels
{
    /// <summary>
    /// Representing a view that will block the shell with its application execution.
    /// </summary>
    public class AppExecutingShellBlockingViewModel : ApplicationBlockShellViewModel, IHandle<IUIPlatformNotificationsAvailableEvent>
    {

        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private string _currentState = "";
        private IPlatformStateDetails _cancelableState;
        private bool _canCancel;
        private bool _showCancelButton;
        #endregion

        /// <summary>
        /// Gets the translated state of the current Application Execution.
        /// </summary>
        /// <value>
        /// The state of the Application Execution.
        /// </value>
        public string CurrentState
        {
            get => _currentState;
            private set
            {
                if(value == _currentState) return;
                _currentState = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanCancel
        {
            get => _canCancel;
            set
            {
                if(value == _canCancel) return;
                _canCancel = value;
                NotifyOfPropertyChange();
            }
        }
        public bool ShowCancelButton
        {
            get => _showCancelButton;
            set
            {
                if(value == _showCancelButton) return;
                _showCancelButton = value;
                NotifyOfPropertyChange();
            }
        }

        #region Constructors
        public AppExecutingShellBlockingViewModel(
                IAppDisplayInfo appDisplayInfo, IViewInputHandler inputHandler, IUIMessageBroker messageBroker) : base(
                appDisplayInfo,
                inputHandler)
        {
            messageBroker.Subscribe(this);
        }
        #endregion

        public void Cancel()
        {
            var cancelableState = _cancelableState;
            if(cancelableState == null)return;
            if(cancelableState.IsCanceled)return;
            cancelableState.Cancel();
            CanCancel = false;
        }

        /// <summary>
        /// Handles the specified Platform notifications available Event by subscribing to it.
        /// </summary>
        /// <param name="availableNotifications">The available notifications.</param>
        public void Handle(IUIPlatformNotificationsAvailableEvent availableNotifications)
        {
            availableNotifications.Subscribe(StateNotification, NotificationsEnded, SynchronizationContext.Current);
        }

        /// <summary>
        /// Receives rthe Platform Notifications and Updates the Current State accordingly.
        /// </summary>
        /// <param name="state">The state.</param>
        private void StateNotification(IPlatformStateDetails state)
        {
            CurrentState = StateToString(state.State);
            //Check if its a cancelable state
            if(state.IsCancelable)
            {
                //Already in CanCancel State
                if(ShowCancelButton)
                {
                    //Update Token and re-enable Button
                    _cancelableState = state;
                    CanCancel = true;
                    return;
                }
                //New to Cancel Mode
                CanCancel = true;
                ShowCancelButton = true;
                _cancelableState = state;
                AddControllerInputAction(ControllerInput.Cancel,Cancel);
            }
            //Received non Cancelable State
            else
            {
                //Allready in Cant Cancel Mode
                if(!ShowCancelButton)return;

                //Switch from CanCancel to cant Cancel
                ClearAllControllerInputActions();
                CanCancel = false;
                ShowCancelButton = false;
                _cancelableState = null;
            }
        }

        /// <summary>
        /// Converts the PlatformState to an translated String
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        private string StateToString(PlatformState state)
        {
            switch(state)
            {
                case PlatformState.Unavailable:
                    return Resources.PlatformState_Unavailible;
                case PlatformState.StartingClient:
                    return Resources.PlatformState_StartingClient;
                case PlatformState.StartingApplication:
                    return Resources.PlatformState_StartingApplication;
                case PlatformState.UpdateingApplication:
                    return Resources.PlatformState_UpdatingApplication;
                case PlatformState.UpdateingClient:
                    return Resources.PlatformState_UpdatingClient;
                case PlatformState.StoppingClient:
                    return Resources.PlatformState_StoppingClient;
                case PlatformState.LoggingIn:
                    return Resources.PlatformState_LoggingIn;
                case PlatformState.LoggingOut:
                    return Resources.PlatformState_LoggingOut;
                case PlatformState.ApplicationRunning:
                    return Resources.PlatformState_ApplicationRunning;
                case PlatformState.ApplicationUpdateRequired:
                    return Resources.PlatformState_ApplicationUpdateRequired;
                case PlatformState.StartingClientError:
                    return Resources.PlatformState_StartingClientError;
                case PlatformState.StartingApplicationError:
                    return Resources.PlatformState_StartingApplicationError;
                default:
                    return state.ToString();
            }
        }

        /// <summary>
        /// Handles the Event when the Notifications finished and sets the State accordingly.
        /// </summary>
        private void NotificationsEnded()
        {
            CanCancel = false;
            CurrentState = Resources.PlatformState_Done;

        }
    }
}
