#region Licence
/****************************************************************
 *  Filename: ConnectViewModel.cs
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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using NLog;
using Pod.Enums;


namespace LeapVR.Shell.UI.Shell.Connect.ViewModels
{

    public class ConnectViewModel : Screen, IHandle<IUIConnectionStateChangedEvent>
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private const string FakePasswordPlaceHolder = "@@@@@@@@@@@";
        private readonly object _displayMessageLock = new object();
        private readonly IRemoteServiceController _remoteServiceController;
        private readonly IUIMessageBroker _messageBroker;
        private readonly bool _autoConnect;
        private readonly DispatcherTimer _clearMessageTimer = new DispatcherTimer();
        private readonly Stopwatch _messageDisplayTimeStopwatch = new Stopwatch();
        private volatile bool _clearMessage;
        private TimeSpan _lastMessageMinDisplayTime = TimeSpan.Zero;
        private bool _autoLogin;
        private string _stationId;
        private string _actionButtonText;
        private string _password;
        private bool _hasFakePassword;
        private bool _hasValidPassword;
        private bool _hasValidStationId;
        private bool _isBusyWithConnectionAction;
        private bool _isBusyWithDisconnectAction;
        private bool _isConnected;
        private bool _hasInvalidCredentials;
        private string _messageText;
        private bool _canCancelOrExit = true;


        public ConnectViewModel(IRemoteServiceController remoteServiceController,IUIMessageBroker messageBroker, bool autoConnect = false)
        {

            _remoteServiceController = remoteServiceController;
            _autoConnect = autoConnect;
            _autoLogin = remoteServiceController.AutoLogin;
            _clearMessageTimer.Tick += ClearMessage;
            if (remoteServiceController.HasStationIdSet)
            {
                _stationId = remoteServiceController.StationId;
                _hasValidStationId = true;
            }
            if (remoteServiceController.HasPasswordSet)
            {
                _password = FakePasswordPlaceHolder;
                _hasValidPassword = true;
                _hasFakePassword = true;
            }
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);

        }

        public string ActionButtonText
        {
            get => _actionButtonText;
            set
            {
                if(value == _actionButtonText) return;
                _actionButtonText = value;
                NotifyOfPropertyChange(() => ActionButtonText);
            }
        }

        public string MessageText
        {
            get => _messageText;
            set
            {
                if(value == _messageText) return;
                _messageText = value;
                NotifyOfPropertyChange(() => MessageText);
            }
        }

        public string StationId
        {
            get => _stationId;
            set
            {
                if(value == _stationId) return;
                _stationId = value;
                HasValidStationId(value.Length >= 36 && _remoteServiceController.SetStationId(value));
                NotifyOfPropertyChange(() => StationId);
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if(value == _password) return;
                if(_hasFakePassword)
                {
                    string newValue;
                    if(String.IsNullOrEmpty(value))
                    {
                        newValue = value;
                    }
                    else
                    {
                        newValue = value.Replace(FakePasswordPlaceHolder, "");
                    }
                    _hasFakePassword = false;
                    _password = newValue;
                }
                else _password = value;
                HasValidPassword(_password != null && _remoteServiceController.SetPassword(_password));
                if(!_isBusyWithConnectionAction) _hasInvalidCredentials = false;
                NotifyOfPropertyChange(() => Password);
            }
        }

        public bool AutoLogin
        {
            get => _autoLogin;
            set
            {
                if(value == _autoLogin) return;
                _autoLogin = value;
                _remoteServiceController.SetAutoLogin(value);
                if(_hasFakePassword && value)
                {
                    _hasFakePassword = false;
                    Password = "";
                }
                NotifyOfPropertyChange(() => AutoLogin);
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if(value == _isConnected) return;
                _isConnected = value;
                NotifyOfPropertyChange(() => IsConnected);
            }
        }

        public bool CanConnectAction => IsReadyToConnect();
        public async Task ConnectAction()
        {
            try
            {
                if(_isBusyWithConnectionAction)return;
                _isBusyWithConnectionAction = true;
                NotifyOfPropertyChange(() => CanConnectAction);
                NotifyOfPropertyChange(() => CanCancelOrExit);
                var result = await _remoteServiceController.
                                   ConnectAsync().
                                   ConfigureAwait(true);
                if(result.HasError() &&
                   (result.ContainsKey(UserError.ShellClientInvalidPassword) ||
                    result.ContainsKey(UserError.ShellClientInvalidStationId)))
                {
                    _hasInvalidCredentials = true;
                    DisplayMessage(Language.Resources.Connect_Error_Invalid_Credentials, TimeSpan.FromSeconds(5));
                }
                else
                {
                    _hasInvalidCredentials = false;
                }
            }
            finally
            {
                _isBusyWithConnectionAction = false;
                NotifyOfPropertyChange(() => CanConnectAction);
                NotifyOfPropertyChange(() => CanCancelOrExit);
            }
        }
        public bool CanDisconnectAction => IsReadyToDisconnect();
        public async Task DisconnectAction()
        {
            try
            {
                if (_isBusyWithDisconnectAction) return;
                _isBusyWithDisconnectAction = true;
                NotifyOfPropertyChange(() => CanDisconnectAction);
                NotifyOfPropertyChange(() => CanCancelOrExit);
                await _remoteServiceController.DisconnectAsync().ConfigureAwait(true);
            }
            finally
            {
                _isBusyWithDisconnectAction = false;
                NotifyOfPropertyChange(() => CanDisconnectAction);
                NotifyOfPropertyChange(() => CanCancelOrExit);
            }
        }


        public bool CanCancelOrExit => !_isBusyWithConnectionAction && !_isBusyWithDisconnectAction;
        public async Task CancelOrExit()
        {
            if(!_isBusyWithConnectionAction)
            {
                CloseDialog();
            }
            else
            {
                await _remoteServiceController.DisconnectAsync();
            }
        }

        private void CloseDialog()
        {
            if (!_isBusyWithConnectionAction)
            {
                _messageDisplayTimeStopwatch.Stop();
                this.TryClose();
            }
        }

        private void HasValidPassword(bool isValid)
        {
            if(_hasValidPassword != isValid)
            {
                _hasValidPassword = isValid;
                NotifyOfPropertyChange(() => CanConnectAction);
            }
        }
        private void HasValidStationId(bool isValid)
        {
            if(_hasValidStationId != isValid)
            {
                _hasValidStationId = isValid;
                NotifyOfPropertyChange(() => CanConnectAction);
            }

        }
        private bool IsReadyToConnect()
        {
            if(_hasValidPassword && _hasValidStationId && !_isBusyWithConnectionAction)
            {
                if(_remoteServiceController.ConnectState == ConnectState.Disconnected)
                {
                    return true;
                }
            }

            return false;
        }
        private bool IsReadyToDisconnect()
        {
            if (!_isBusyWithDisconnectAction)
            {
                if (_remoteServiceController.ConnectState == ConnectState.Connected)
                {
                    return true;
                }
            }

            return false;
        }

        private void DisplayMessage(string message, TimeSpan? duration = null)
        {
            lock(_displayMessageLock)
            {
                if(!duration.HasValue || duration.Value <= TimeSpan.Zero)
                {
                    //No Time limitation
                    _clearMessage = false;
                    _lastMessageMinDisplayTime = TimeSpan.Zero;
                    _messageDisplayTimeStopwatch.Reset();
                    _clearMessageTimer.Stop();
                    MessageText = message;
                }
                else
                {
                    //Has Time limit
                    _clearMessage = true;
                    _clearMessageTimer.Stop();
                    _lastMessageMinDisplayTime = duration.Value;
                    _messageDisplayTimeStopwatch.Restart();
                    _clearMessageTimer.Interval = duration.Value;
                    _clearMessageTimer.Start();
                    MessageText = message;

                }
            }
        }

        private void ClearMessage(object obj,EventArgs args)
        {
            //Secure the Method to not remove messages accidentally 
            if(!_clearMessage)return;
            lock(_displayMessageLock)
            {
                if(!_clearMessage)return;
                if(_messageDisplayTimeStopwatch.Elapsed >= _lastMessageMinDisplayTime)
                {
                    MessageText = "";
                    _clearMessageTimer.Stop();
                }
            }
        }

        protected override async void OnViewReady(object view)
        {
            if(_autoConnect && _autoLogin && _hasValidStationId && _hasValidPassword)
            {
                await ConnectAction();
            }
        }
        protected override void OnDeactivate(bool close)
        {
            _messageBroker.Unsubscribe(this);
            base.OnDeactivate(close);
        }
        public void Handle(IUIConnectionStateChangedEvent message)
        {
            switch(message.CurrentState)
            {
                case ConnectState.Disconnected:
                    if (!_hasInvalidCredentials)DisplayMessage(Language.Resources.Connect_Disconnected, TimeSpan.FromSeconds(3));
                    IsConnected = false;
                    break;
                case ConnectState.Connecting:
                    DisplayMessage(Language.Resources.Connect_Connecting);
                    break;
                case ConnectState.Connected:
                    DisplayMessage(Language.Resources.Connect_Connected, TimeSpan.FromSeconds(3));
                    IsConnected = true;
                    // We can Close the Dialog When we are Connected. The
                    // Connected event fires from inside the await chain of
                    // ConnectAction, BEFORE the finally{} flips _isBusyWith-
                    // ConnectionAction back to false - so going through
                    // CloseDialog() (which guards on !_isBusyWithConnectionAction)
                    // would race-lose and leave the dialog stuck open. Close
                    // directly here; the guard on CloseDialog still protects
                    // CancelOrExit from killing an in-flight connect.
                    _messageDisplayTimeStopwatch.Stop();
                    this.TryClose();
                    break;
                case ConnectState.Disconnecting:
                    if(!_hasInvalidCredentials)DisplayMessage(Language.Resources.Connect_Disconnecting);
                    break;
                default:
                    Logger.Warn($"Unknown Connection State {message.CurrentState}");
                    break;
            }
            NotifyOfPropertyChange(() => CanDisconnectAction);
            NotifyOfPropertyChange(() => CanConnectAction);
        }
    }
}
