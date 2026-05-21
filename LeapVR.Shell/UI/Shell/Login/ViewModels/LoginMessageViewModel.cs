#region Licence
/****************************************************************
 *  Filename: LoginMessageViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-4
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
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Shell.Login.ViewModels
{
    /// <summary>
    /// General class for different kinds of awaiting reports
    /// </summary>
    public class LoginMessageViewModel : InputControllerScreen,IHandle<IUILanguageChangedEvent>,IDisposable, ILoginPageViewModel
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IUIMessageBroker _messageBroker;
        private readonly MessageType _messageType = MessageType.Unknown;
        private readonly LoginDecisionResultType _resultType = LoginDecisionResultType.Unknown;
        private string _information;
        public string Information
        {
            get => _information;
            set
            {
                _information = value;
                NotifyOfPropertyChange();
            }
        }

        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set
            {
                _image = value;
                NotifyOfPropertyChange();
            }
        }
        public LanguageSelectViewModel LanguageSelect { get; set; } 
        #endregion

        #region Constructors
        public LoginMessageViewModel(IUIMessageBroker messageBroker,IViewInputHandler inputHandler,LanguageSelectViewModel languageSelectViewModel,MessageType type = MessageType.Connecting):base(inputHandler)
        {
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _messageType = type;
            SetInformation(type);
            RegisterLanguageSelect(languageSelectViewModel);
        }

        public LoginMessageViewModel(IUIMessageBroker messageBroker,IViewInputHandler inputHandler,LanguageSelectViewModel languageSelectViewModel,LoginDecisionResultType loginDecisionResult):base(inputHandler)
        {
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _resultType = loginDecisionResult;
            SetInformation(loginDecisionResult);
            RegisterLanguageSelect(languageSelectViewModel);
        }

        private void RegisterLanguageSelect(LanguageSelectViewModel languageSelectViewModel)
        {
            LanguageSelect = languageSelectViewModel;
            AddControllerInputAction(ControllerInput.NextTwo, languageSelectViewModel.NavigateToNext);
            AddControllerInputAction(ControllerInput.PreviousTwo, languageSelectViewModel.NavigateToPrevious);
        }
        private void SetInformation(LoginDecisionResultType information)
        {
            var result = GetResponseForResult(information);
            Information = result.Message;
            Image = result.Image;
        }

        private void SetInformation(MessageType messageType)
        {
            var result = GetResponseForResult(messageType);
            Information = result.Message;
            Image = result.Image;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Cope with the response according to login decision result.
        /// </summary>
        /// <param name="result">Login intention result</param>
        /// <returns></returns>
        private (string Message, ImageSource Image) GetResponseForResult(LoginDecisionResultType result)
        {
            string text;
            ImageSource picture;
            switch (result)
            {
                case LoginDecisionResultType.SessionStarted:
                    text = Language.Resources.Shell_Enjoy;
                    picture = Application.Current.Resources["ImageSuccess"] as ImageSource;
                    break;
                case LoginDecisionResultType.Canceled:
                    text ="";
                    picture = Application.Current.Resources["ImageGoodbye"] as ImageSource;
                    break;
                case LoginDecisionResultType.IntentionExpired:
                    text = Language.Resources.Global_LoginTips_IntentionExpired;
                    picture = Application.Current.Resources["ImageTimeExpired"] as ImageSource;
                    break;
                case LoginDecisionResultType.NotEnoughBalance:
                    text = Language.Resources.Global_LoginTips_NotEnoughBalance;
                    picture = Application.Current.Resources["ImageAddBalance"] as ImageSource;
                    break;
                case LoginDecisionResultType.SessionRateChanged:
                    text = Language.Resources.Global_LoginTips_SessionRateChanged;
                    picture = Application.Current.Resources["ImageExchangeRate"] as ImageSource;
                    break;
                case LoginDecisionResultType.StationHaveRunningSession:
                    text = Language.Resources.Global_LoginTips_StationHaveRunningSession;
                    picture = Application.Current.Resources["ImageNotAvailable"] as ImageSource;
                    break;
                case LoginDecisionResultType.UserHaveRunningSession:
                    text = Language.Resources.Global_LoginTips_UserHaveRunningSession;
                    picture = Application.Current.Resources["ImageNotAvailable"] as ImageSource;
                    break;
                case LoginDecisionResultType.UserBlocked:
                    text = Language.Resources.Global_LoginTips_UserBlocked;
                    picture = Application.Current.Resources["ImageUserBlocked"] as ImageSource;
                    break;
                case LoginDecisionResultType.UserNotActive:
                    text = Language.Resources.Global_LoginTips_UserNotActive;
                    picture = Application.Current.Resources["ImageUserActivation"] as ImageSource;
                    break;
                default:
                    text = Language.Resources.Exceptions_UnknownError;
                    picture = Application.Current.Resources["ImageError"] as ImageSource;
                    break;
            }
            Logger.Debug($"Get corresponding information by {nameof(LoginDecisionResultType)} = {result}.");
            return (text, picture);
        }

        private (string Message, ImageSource Image) GetResponseForResult(MessageType messageTypes)
        {
            ImageSource image;
            string information;
            switch (messageTypes)
            {
                case MessageType.Connecting:
                    image = Application.Current.Resources["ImageConnecting"] as ImageSource;
                    information = Language.Resources.Global_ConnectingToService;
                    break;
                case MessageType.LocalConnectionProblem:
                    image = Application.Current.Resources["ImageOffline"] as ImageSource;
                    information = Language.Resources.Global_LoginTips_LocalNetworkProblem;
                    break;
                case MessageType.CloudNotAccessibleProblem:
                    image = Application.Current.Resources["ImageCloudNotAccessible"] as ImageSource;
                    information = Language.Resources.Global_LoginTips_RemoteServerProblem;
                    break;
                case MessageType.ShellVersionOutOfDate:
                    image = Application.Current.Resources["ImageVersionError"] as ImageSource;
                    information = Language.Resources.Exceptions_VersionOutOfDate;
                    break;
                case MessageType.InvalidUsernameOrPassword:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_InvalidUsernamePassword;
                    break;
                case MessageType.LicenseNotDeployed:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_LicenseNotDeployed;
                    break;
                case MessageType.LicenseNotFound:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_LicenseNotFound;
                    break;
                case MessageType.LicenseNotLinked:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_LicenseNotFound;
                    break;
                case MessageType.LicenseSuspended:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_LicenseSuspended;
                    break;
                case MessageType.LicenseRevoked:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_LicenseRevoked;
                    break;
                case MessageType.StationRevoked:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_StationRevoked;
                    break;
                case MessageType.StationSuspended:
                    image = Application.Current.Resources["ImageLicenseError"] as ImageSource;
                    information = Language.Resources.Exceptions_StationSuspended;
                    break;
                default:
                    image = Application.Current.Resources["ImageError"] as ImageSource;
                    information = Language.Resources.Exceptions_UnknownError;
                    break;
            }
            return (information, image);
        }
        #endregion

        public void Handle(IUILanguageChangedEvent message)
        {
            if(_messageType == MessageType.Unknown)SetInformation(_resultType);
            else SetInformation(_messageType);
        }
        public new void Dispose()
        {
            _messageBroker?.Unsubscribe(this);
            LanguageSelect?.Dispose();
            base.Dispose();
        }
    }
}
