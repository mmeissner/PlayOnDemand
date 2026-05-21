#region Licence
/****************************************************************
 *  Filename: ViewModelFactory.cs
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
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Input.XInput;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.Language;
using LeapVR.Shell.Modules.Interfaces.Network;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Dashboard.ViewModels;
using LeapVR.Shell.UI.Shell.Login.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.ContentHolder.ViewModels;
using LeapVR.Shell.UI.Universal.Dialog;
using LeapVR.Shell.UI.Universal.Dialog.ViewModels;
using LeapVR.Shell.UI.Universal.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.StationDetails.ViewModels;
using LeapVR.Shell.UI.Universal.ViewModels;
using NLog;
using SimpleInjector;

namespace LeapVR.Shell.UI
{
    public class ViewModelFactory
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IUIMessageBroker _messageBroker;
        private readonly IViewInputHandler _viewInputHandler;
        private readonly IStationController _stationController;
        private readonly IRemoteServiceController _remoteServiceController;
        private readonly IWindowManager _windowManager;
        private readonly IPlatformController _platformController;
        private readonly Core.PlatformProvider _platformProvider;
        private readonly ILanguageSelector _languageSelector;
        private readonly UiConfig _uiConfig;


        public ViewModelFactory(
            IUIMessageBroker messageBroker,
            IStationController stationController,
            IRemoteServiceController remoteServiceController,
            IViewInputHandler inputHandler,
            IWindowManager windowManager,
            IPlatformController platformController,
            ILanguageSelector languageSelector,
            Core.PlatformProvider platformProvider,
            UiConfig uiConfig)
        {
            QuickLeap.AssertNotNull(messageBroker,
                stationController,
                inputHandler,
                windowManager,
                platformController,
                    languageSelector,
                    platformProvider,
                uiConfig);
            _uiConfig = uiConfig;
            _messageBroker = messageBroker;
            _viewInputHandler = inputHandler;
            _stationController = stationController;
            _remoteServiceController = remoteServiceController;
            _windowManager = windowManager;
            _platformController = platformController;
            _languageSelector = languageSelector;
            _platformProvider = platformProvider;
            //_loginViewsLanguageSelectViewModel = new LanguageSelectViewModel(_messageBroker, languageSelector, null);
        }

        public AppExecutionSelectViewModel Build()
        {
            return new AppExecutionSelectViewModel(_viewInputHandler,_messageBroker);
        }
        public ApplicationExecutableViewModel Build(IAppPlatformInfo appPlatformInfo)
        {
            return new ApplicationExecutableViewModel(appPlatformInfo,_platformController,_stationController,_windowManager,this);
        }
        public ConfirmationDialogViewModel Build(DialogType type)
        {
            return new ConfirmationDialogViewModel(_messageBroker, _viewInputHandler, BuildContentViewModel(type, out var isCancelOnly),isCancelOnly);
        }
        public ILoginIntentionViewModel Build(ILoginIntention loginIntention)
        {
            ILoginIntentionViewModel retval = null;
            switch (loginIntention.SessionRate)
            {
                case INoBillingSessionRate noBillingSessionRate:
                    throw new NotImplementedException("LoginIntention for No Billing Sessions are not Implemented");
                case IPrepaidSessionRate prepaidSessionRate:
                    retval = new LoginIntentionPrepaidViewModel(
                        loginIntention,
                        _messageBroker,
                        _uiConfig,
                        _viewInputHandler);
                    break;
            }
            return retval;
        }
        public ILoginModeViewModel Build(ISessionSettings sessionSettings)
        {
            ILoginModeViewModel result = null;
            switch(sessionSettings)
            {

                case AnonymousLoginSettingsBase _:
                    Logger.Info($"Building {nameof(LoginOpenModeViewModel)}");
                    result = new LoginOpenModeViewModel(
                        _messageBroker,
                        _viewInputHandler,
                        this,
                        _windowManager,
                        new LanguageSelectViewModel(_messageBroker, _languageSelector, null),
                        _uiConfig,
                        _stationController,
                        _remoteServiceController);
                    break;
                case QrCodeLoginSettingsBase sessionSetup:
                    result = new LoginBusinessModeViewModel(sessionSetup, _messageBroker,new LanguageSelectViewModel(_messageBroker, _languageSelector, null),_viewInputHandler, _uiConfig.QrCodeWidth);
                    Logger.Info($"Building {nameof(LoginBusinessModeViewModel)}");
                    break;
                default:
                    Logger.Error("The View received an Session Type of either null or of unknown type!");
                    break;
            }
            return result;
        }
        public LoginMessageViewModel Build(LoginDecisionResultType loginDecision)
        {
            return new LoginMessageViewModel(_messageBroker,_viewInputHandler,new LanguageSelectViewModel(_messageBroker, _languageSelector, null), loginDecision);
        }
        public LoginMessageViewModel Build(MessageType type)
        {
            return new LoginMessageViewModel(_messageBroker,_viewInputHandler,new LanguageSelectViewModel(_messageBroker, _languageSelector, null), type);
        }

        public PlatformSelectorViewModel BuildPlatformSelector()
        {
            return new PlatformSelectorViewModel(_platformProvider);
        }

        private IDialogContent BuildContentViewModel(DialogType type, out bool isCancelOnly)
        {
            isCancelOnly = false;
            switch(type)
            {
                case DialogType.StartScreenGameInVrMode:
                    var imageSource = Application.Current.Resources["ImageXbuttonsMapping"] as ImageSource;
                    return new ImageContentViewModel(
                        ()=>Resources.Execution_StartScreenAppInVrModeConfirmation,
                        imageSource);
                case DialogType.StartVrGameInScreenMode:
                    return new StringContentViewModel(() => Resources.Execution_StartVrAppInScreenModeConfirmation);
                case DialogType.ResetVrBoxStatistics:
                    return new StringContentViewModel(() =>Resources.Statistics_ResetConfirmationMessage);
                case DialogType.AttemptToLogout:
                    return new StringContentViewModel(() =>Resources.Session_MessageBeforeLogout);
                case DialogType.AttemptToShutdown:
                    return new StringContentViewModel(() =>Resources.System_Exit_DoYouReallyWantToExit);
                case DialogType.AttemptToPowerOff:
                    return new StringContentViewModel(() =>Resources.System_Poweroff_DoYouReallyWantToPoweroff);
                case DialogType.AttemptToDeletePlatformAccount:
                    return new StringContentViewModel(() =>Resources.System_Platform_ConfirmDeleteAccount);
                case DialogType.NoSuitableExecution:
                    isCancelOnly = true;
                    return new StringContentViewModel(() => Resources.Executable_NoSuitableExecution);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}