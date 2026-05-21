#region Licence
/****************************************************************
 *  Filename: ShellViewModel.cs
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
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Blocker.ViewModels;
using LeapVR.Shell.UI.Shell.Connect.ViewModels;
using LeapVR.Shell.UI.Shell.Views;
using LeapVR.Shell.UI.Universal.ViewModels;
using LeapVR.Utilities.Windows;
using NLog;

namespace LeapVR.Shell.UI.Shell.ViewModels
{
    public sealed class ShellViewModel : InputControllerConductorOneActive<IScreen>,IDisposable, IShell
        , IHandle<IUISessionStartedEvent>
        , IHandle<IUISessionStopedEvent>
        , IHandle<UIAdminAccessAttemptEvent>
        , IHandle<UIAdminAccessDismissEvent>
        , IHandle<IUIAppExecutionEvent>
        , IHandle<IUIAppInstallationStartedEvent>
        , IHandle<IUIAppUninstallationStartedEvent>
        , IHandle<IUIConnectDialogEvent>
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IWindowManager _windowManager;
        private readonly IVirtualRealityController _virtualRealityController;
        private readonly IUIMessageBroker _messageBroker;
        private readonly IViewInputHandler _viewInputHandler;
        private readonly UiConfig _uiConfig;
        private readonly BitmapImage _originalGlobalBackground;
        private readonly BitmapImage _originalSplashBackground;
        private readonly BitmapImage _originalSplashButtomBackground;
        private readonly ILanguageSelector _languageSelector;
        private readonly IAdministrationViewModel _adminViewModel;
        private readonly object _transparencyAreaLock = new object();
        private TransparancyArea _transparencyArea;

        private ILoginViewModel _loginViewModel;
        private IDashboardViewModel _dashboardViewModel;
        private IAdministrationViewModel _administrationViewModel;
        private IBlockShellViewModel _shellBlockingViewModel;
        private IScreen _previousActivatedItem;
        private ShellView _shellView;
        private bool _isConnectDialogOpen;

        public bool IsAdministrationActive => _administrationViewModel != null;
        public string ActiveViewName => ActiveItem?.GetType().Name;
        public ILoginViewModel LoginViewModel
        {
            get => _loginViewModel;
            set
            {
                _loginViewModel = value;
                NotifyOfPropertyChange();
            }
        }
        public IDashboardViewModel DashboardViewModel
        {
            get => _dashboardViewModel;
            set
            {
                _dashboardViewModel = value;
                NotifyOfPropertyChange();
            }
        }
        public IAdministrationViewModel AdministrationViewModel
        {
            get => _administrationViewModel;
            set
            {
                _administrationViewModel = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsAdministrationActive);
            }
        }
        public IBlockShellViewModel ShellBlockingViewModel
        {
            get => _shellBlockingViewModel;
            set
            {
                _shellBlockingViewModel = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors

        public ShellViewModel(
            IUIMessageBroker messageBroker,
            IWindowManager windowManager,
            IVirtualRealityController virtualRealityController,
            IDashboardViewModel dashboardViewModel,
            ILoginViewModel loginViewModel,
            IAdministrationViewModel administrationViewModel,
            IViewInputHandler viewInputHandler,
            ILanguageSelector languageSelector,
            UiConfig uiConfig
            ) :base(viewInputHandler)
        {
            QuickLeap.AssertNotNull(
                languageSelector,
                viewInputHandler,
                messageBroker,
                windowManager,
                virtualRealityController,
                uiConfig,
                loginViewModel,
                dashboardViewModel,
                administrationViewModel);

            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);

            _windowManager = windowManager;
            _viewInputHandler = viewInputHandler;
            _languageSelector = languageSelector;
            _virtualRealityController = virtualRealityController;

            _uiConfig = uiConfig;

            LoginViewModel = loginViewModel;
            DashboardViewModel = dashboardViewModel;
            _adminViewModel = administrationViewModel;

            Items.AddRange(new IScreen[]{LoginViewModel,DashboardViewModel, _adminViewModel});
            DisplayName = Language.Resources.Global_ProductName;

            // always last. 
            _originalGlobalBackground = (BitmapImage)Application.Current.Resources["BackgroundGlobal"];
            _originalSplashBackground = (BitmapImage)Application.Current.Resources["BackgroundSplashscreen"];
            _originalSplashButtomBackground = (BitmapImage)Application.Current.Resources["BackgroundSplashscreenButtom"];
            AddControllerInputAction(ControllerInput.Guide,ShowInputControllerMapping);
        }
        #endregion

        #region Methods
        #endregion

        #region Message Handlers
        public void Handle(IUIConnectDialogEvent connectDialogEvent)
        {
            Logger.Info($"Try to open {nameof(ConnectViewModel)} as Dialog.");
            if(_isConnectDialogOpen)return;
            try
            {
                _isConnectDialogOpen = true;
                _windowManager.ShowDialog(new ConnectViewModel(connectDialogEvent.Controller, _messageBroker, connectDialogEvent.AutoConnect), null, ShellClientHelper.GetUniversalDialogSettings());
            }
            finally
            {
                _isConnectDialogOpen = false;
            }
        }

        /// <summary>
        /// Handles the Admin Access Attempt Event.
        /// </summary>
        /// <param name="accessAdministrationEventArgs">The event.</param>
        public void Handle(UIAdminAccessAttemptEvent accessAdministrationEventArgs)
        {
            Logger.Info($"Try to activate {nameof(AdministrationViewModel)} on attemptted access.");
            GotoSystemAdministration();
        }

        /// <summary>
        /// Handles the Admin Access Dismiss Event.
        /// </summary>
        /// <param name="leaveAdministrationEventArgs">The event.</param>
        public void Handle(UIAdminAccessDismissEvent leaveAdministrationEventArgs)
        {
            switch (leaveAdministrationEventArgs.DismissReason)
            {
                case ViewDismissReason.ActivelyClose:
                    Logger.Info($"Try to dismiss {nameof(AdministrationViewModel)} due to user actively go back.");
                    break;
                case ViewDismissReason.Timeout:
                    Logger.Info($"Try to dismiss {nameof(AdministrationViewModel)} due to inactivity timeout.");
                    break;
            }
            BackFromSystemAdministration();
        }

        /// <summary>
        /// Handles the Session Started Event.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(IUISessionStartedEvent message)
        {
            // switch the state that triggers the awaiting view going up.
            ActivateItem(DashboardViewModel);
            Logger.Info($"View switched to {nameof(DashboardViewModel)} on login intention confirmed.");
        }

        /// <summary>
        /// Handles the Session Stopped Event.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(IUISessionStopedEvent message)
        {
            var sessionStopReason = message?.Session?.StopReason;
            if (_uiConfig.AllowDisplayInfoWhenSessionAutoLogout)
            {
                IBlockShellViewModel viewModel = null;
                switch (sessionStopReason)
                {
                    case SessionStopReason.SessionLimitReached:
                        viewModel = GetBlockShellViewModel(
                            Language.Resources.Session_Info_SessionLimitReached,
                            Application.Current.Resources["ThumbnailSessionEndedByLimitReached"] as ImageSource);
                        break;
                    case SessionStopReason.StationInactivity:
                        viewModel = GetBlockShellViewModel(
                            Language.Resources.Session_Info_StationInactive,
                            Application.Current.Resources["ThumbnailStationInactive"] as ImageSource);
                        break;
                    case SessionStopReason.UserBlocked:
                        viewModel = GetBlockShellViewModel(
                        Language.Resources.Session_Info_UserBlocked,
                        Application.Current.Resources["ThumbnailUserBlocked"] as ImageSource);
                        break;
                    case SessionStopReason.AbandonedSession:
                        viewModel = GetBlockShellViewModel(
                        Language.Resources.Session_Info_InternetProblems,
                         Application.Current.Resources["ThumbnailInternetProblems"] as ImageSource);
                        break;
                    case SessionStopReason.Unknown:
                        viewModel = GetBlockShellViewModel(
                        Language.Resources.Exceptions_UnknownError,
                        Application.Current.Resources["ImageError"] as ImageSource);
                        Logger.Warn($"Session stopped unexpectedly. StopReason ={sessionStopReason}");
                        break;
                }
                if (viewModel != null)
                {
                    Logger.Debug($"{nameof(InformationShellBlockingViewModel)} Showing {ShellBlockingViewModel} due to StopReason: '{sessionStopReason}.");
                    ShellBlockingViewModel?.Dispose();
                    ShellBlockingViewModel = viewModel;
                }
            }
            Logger.Info($"View switched to {nameof(LoginViewModel)} due to session stopped. StopReason: {sessionStopReason}");
            ActivateItem(LoginViewModel);
            _languageSelector.ActivateDefaultCultureInfo();

        }

        /// <summary>
        /// Handles the specified Application Execution event.
        /// </summary>
        /// <param name="appExecutionEvent">The execution event.</param>
        public void Handle(IUIAppExecutionEvent appExecutionEvent)
        {
            switch (appExecutionEvent.ExecutionPhase)
            {
                case UIApplicationExecutionPhase.BeginnExecution:
                    Logger.Info($"Try to load busy running view on [{appExecutionEvent.ApplicationGuid}] {nameof(ExecutionPhase.BeforeStart)}.");
                    ShellBlockingViewModel = new AppExecutingShellBlockingViewModel(appExecutionEvent.DisplayInfo, _viewInputHandler,_messageBroker);
                    ScreenExtensions.TryActivate(ShellBlockingViewModel);
                    break;
                case UIApplicationExecutionPhase.EndedSuccefully:
                    Logger.Info($"Try to dismiss busy running view on [{appExecutionEvent.ApplicationGuid}] {nameof(ExecutionPhase.OnFinished)}.");
                    ScreenExtensions.TryDeactivate(ShellBlockingViewModel, false);
                    ShellBlockingViewModel?.Close();
                    ShellBlockingViewModel?.Dispose();
                    ShellBlockingViewModel = null;
                    break;
            }
        }

        /// <summary>
        /// Shell reacts to new app installation in progress.
        /// </summary>
        /// <param name="appInstallationStartedEvent"></param>
        public void Handle(IUIAppInstallationStartedEvent appInstallationStartedEvent)
        {
            ShellBlockingViewModel = new InstallationShellBlockingViewModel(appInstallationStartedEvent.ProcessInfo, _viewInputHandler);
            Logger.Debug($"{nameof(InstallationShellBlockingViewModel)} gets pop up on new installation started.");
        }

        /// <summary>
        /// Shell reacts to new app Uninstallation in progress.
        /// </summary>
        /// <param name="appUninstallationStartedEvent"></param>
        public void Handle(IUIAppUninstallationStartedEvent appUninstallationStartedEvent)
        {
            ShellBlockingViewModel = new UninstallationShellBlockingViewModel(_messageBroker,appUninstallationStartedEvent.ProcessInfo,_viewInputHandler);
            Logger.Info($"{nameof(UninstallationShellBlockingViewModel)} gets pop up on new installation started.");
        }
        #endregion

        #region Overrides
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            //We must get references to Images we going to process during cutout of VR Driver Window
            //As well as provide a Window Handle to the VRController
            if(view is ShellView shellView)
            {
                _shellView = shellView;
                _virtualRealityController.SetUiInteractivity(GetTransparencyArea);
            }
            else
            {
                Logger.Error("Could not get ShellView & WindowHandle!");
            }
            ActivateItem(LoginViewModel);
            CleanScreenHelper.MoveWindowsCursorToSafePosition();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ActivateItem(LoginViewModel);
        }
        public override void ActivateItem(IScreen item)
        {
            _previousActivatedItem = ActiveItem;
            base.ActivateItem(item);
            NotifyOfPropertyChange(() => ActiveViewName);
        }
        #endregion

        #region Private Methods
        private void BackFromSystemAdministration()
        {
            if(ActiveItem.Equals(AdministrationViewModel))
            {
                ActivateItem(_previousActivatedItem);
            }
            AdministrationViewModel = null;
            Logger.Info($"{nameof(AdministrationViewModel)} dismissed.");
        }

        private void GotoSystemAdministration()
        {
            AdministrationViewModel = _adminViewModel;
            ActivateItem(AdministrationViewModel);
            Logger.Info($"{nameof(AdministrationViewModel)} activated.");
        }

        private void ShowInputControllerMapping()
        {
            Logger.Debug($"Poping up {nameof(SingleImageInformationViewModel)}.");
            var viewModel = new SingleImageInformationViewModel(_viewInputHandler, _messageBroker, new[] { ControllerInput.Guide });
            // TODO [FH] later redesign the whole screen management.
            Logger.Debug( $"Deactivate {this} before the popup.");
            //ScreenExtensions.TryDeactivate(this, false);
            try
            {
                Logger.Info("XButtons mapping window poped up.");
                _windowManager.ShowDialog(viewModel, "XButtonsMapping", ShellClientHelper.GetUniversalDialogSettings());
            }
            finally
            {
                Logger.Debug( $"Re-activate {this} after {nameof(SingleImageInformationViewModel)} popup dialog.");
                //ScreenExtensions.TryActivate(this);
            }
            Logger.Info("XButtons mapping window closed.");
        }

        private TransparancyArea GetTransparencyArea(double width, double height)
        {
            lock(_transparencyAreaLock)
            {
                //On 100% 1.Call Width = 264
                //On 100% 1.Call Height = 149
                //On 100% 2.Call Height = 173
                //On 125% 1.Call Width = 309
                //On 125% 1.Call Height = 161
                //On 125% 2.Call Height 197
                if(_transparencyArea != null &&
                   Math.Abs(_transparencyArea.Height - height) < 0.001 &&
                   Math.Abs(_transparencyArea.Width - width) < 0.001) return _transparencyArea;
                if(_shellView == null) return null;
                _transparencyArea  = new TransparancyArea(new DisplayContainer(_shellView.Root,_shellView),height,width, new Margins(5,0,0,0), AlignmentX.Left, AlignmentY.Bottom);
                return _transparencyArea;
            }
        }

        private TransparancyArea GetTransparencyArea()
        {
            lock(_transparencyAreaLock)
            {
                return _transparencyArea;
            }
        }

        private IBlockShellViewModel GetBlockShellViewModel(string message,
            ImageSource image,
            TimeSpan? minimumDisplayTime = null)
        {
                if (minimumDisplayTime == null)
                    minimumDisplayTime = TimeSpan.FromMilliseconds(_uiConfig.MillisecondsToStayBeforeAutoCloseViewsClosing);
                return new InformationShellBlockingViewModel(minimumDisplayTime.Value, image, message, _viewInputHandler);
        }

        #endregion

        public new void Dispose()
        {
            _messageBroker?.Unsubscribe(this);
            base.Dispose();
        }
    }
}
