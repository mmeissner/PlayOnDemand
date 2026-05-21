#region Licence
/****************************************************************
 *  Filename: LoginOpenModeViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-11-14
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.Dialog;
using LeapVR.Shell.UI.Universal.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Shell.Login.ViewModels
{
    public sealed class LoginOpenModeViewModel : InputControllerConductorOneActive<MenueItemViewModel>,IHandle<IUISessionStartedEvent>, IHandle<IUISessionStopedEvent>,IHandle<IUILanguageChangedEvent>, ILoginModeViewModel, IDisposable
    {
        #region Fields & Properties
        private const int MessageDurationInMs = 3000;
        private const int LoginAttemptTimeoutMs = 30000; 
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IStationController _stationController;
        private readonly IRemoteServiceController _remoteServiceController;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly IWindowManager _windowManager;
        private WpfDirectionalNavigation<MenueItemViewModel> _directionalNavigation;
        private readonly IUIMessageBroker _messageBroker;
        private MenueItemViewModel _playButton;
        private DispatcherTimer _loginAttemptTimer;
        private bool _canPlay = true;
        private bool _loginAttemptInProgress = false;
        private string _tip;
        private Func<string> _lastTipString;


        public UiConfig UiConfig { get; }
        public LoginMode Mode { get; } = LoginMode.OpenMode;
        public bool CanPlay
        {
            get => _canPlay;
            private set
            {
                _canPlay = value;
                _playButton.IsEnabled = _canPlay;
                _playButton.IsBusy = !value;
                NotifyOfPropertyChange();
            }
        }
        public string Tip
        {
            get => _tip;
            set
            {
                _tip = value;
                NotifyOfPropertyChange();

            }
        }
        private MenueItemViewModel _selectedItem;
        public MenueItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                ActivateItem(_selectedItem);
                NotifyOfPropertyChange();
            }
        }
        public LanguageSelectViewModel LanguageSelect { get; set; } 
        #endregion

        #region Constructors
        public LoginOpenModeViewModel(
            IUIMessageBroker messageBroker,
            IViewInputHandler inputHandler,
            ViewModelFactory viewModelFactory,
            IWindowManager windowManager,
            LanguageSelectViewModel languageSelectViewModel,
            UiConfig uiConfig,
            IStationController stationController,
            IRemoteServiceController remoteServiceController) : base(inputHandler)
        {
            _stationController = stationController;
            _remoteServiceController = remoteServiceController;
            _viewModelFactory = viewModelFactory;
            _windowManager = windowManager;
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _loginAttemptTimer = new DispatcherTimer();
            _loginAttemptTimer.Tick += _loginAttemptTimer_Timeout;
            _loginAttemptTimer.Interval = TimeSpan.FromMilliseconds(LoginAttemptTimeoutMs);
            _loginAttemptTimer.IsEnabled = false;
            UiConfig = uiConfig;
            //AddControllerInputAction(ControllerInput.Accept,Play);
            SetTip(() => Resources.StartTip_Button);
            CreateMenue(_messageBroker);
            LanguageSelect = languageSelectViewModel;
            AddControllerInputAction(ControllerInput.DDown,()=>{Navigate(NavigationDirection.Down);});
            AddControllerInputAction(ControllerInput.Down, () => { Navigate(NavigationDirection.Down); });
            AddControllerInputAction(ControllerInput.DUp, () => { Navigate(NavigationDirection.Up); });
            AddControllerInputAction(ControllerInput.Up, () => { Navigate(NavigationDirection.Up); });
            AddControllerInputAction(ControllerInput.Left, () => { Navigate(NavigationDirection.Left); });
            AddControllerInputAction(ControllerInput.DLeft, () => { Navigate(NavigationDirection.Left); });
            AddControllerInputAction(ControllerInput.Right, () => { Navigate(NavigationDirection.Right); });
            AddControllerInputAction(ControllerInput.DRight, () => { Navigate(NavigationDirection.Right); });
            AddControllerInputAction(ControllerInput.Accept, ExecuteSelectedItem);
            
            AddControllerInputAction(ControllerInput.NextTwo, languageSelectViewModel.NavigateToNext);
            AddControllerInputAction(ControllerInput.PreviousTwo, languageSelectViewModel.NavigateToPrevious);
        }
        #endregion

        #region Methods
        public async void Play()
        {
            try
            {
                //Indicate Work
                Logger.Info("Play Button pressed, trying to start an new anonymous session.");
                CanPlay = false;

                //Send Request
                var result = await _remoteServiceController.SendAnonymousSessionLoginIntentionAsync().ConfigureAwait(true);
                Logger.Debug($"Anonymous Session Returned {result}");
                Func<string> errorMessage;

                //There are cases were we would receive a Success but there will still no Session start
                //This is due to the fact that we only place an Intention but this intention is not yet properly processed
                //This cases might happen when any of this Errors happen in RemoteServiceController
                //LongCallGetLoginIntentionsAsync fails
                //AcknowledgeLoginIntentionAsync call fails
                //SendLoginDecisionAsync call fails 
                switch(result)
                {
                    case IntendAnonymousSessionResult.Success:
                        Logger.Info("Intend anonymous session successfully.");
                        _loginAttemptInProgress = true;
                        _loginAttemptTimer.IsEnabled = true;
                        await Task.Delay(UiConfig.MillisecondsToStayBeforeAutoCloseViewsClosing).ConfigureAwait(true);
                        SetTip(() => Resources.StartTip_Button);

                        return;
                    case IntendAnonymousSessionResult.StationHaveRunningSession:
                        errorMessage = () => Resources.FailedToStart_StationHaveRunningSession;
                        break;
                    case IntendAnonymousSessionResult.StationHaveActiveIntention:
                        errorMessage = () => Resources.FailedToStart_StationHaveActiveLoginIntention;
                        break;
                    case IntendAnonymousSessionResult.AnonymousSessionsNotAccepted:
                        errorMessage = () => Resources.FailedToStart_AnonymouseSessionNotAllowed;
                        break;
                    default:
                        errorMessage = () => Resources.FailedToStart_UnknwonError;
                        break;
                }
                Logger.Warn($"Failed to intend anonymous session.Error = {result}");
                await ShowTipAsync(
                                errorMessage,
                                () => Resources.StartTip_Button,
                                MessageDurationInMs).
                        ConfigureAwait(true);
                CanPlay = true;
            }
            catch(Exception exception)
            {
                Logger.Error(exception, $"{exception.GetType().Name} thrown when trying to perform anonymouse play.");
            }
            finally
            {
                Logger.Info("Leaving Play Method");
            }
        }
        #endregion

        //We Reset our states when a session has Stopped to be ready for the next one
        public void Handle(IUISessionStopedEvent message)
        {
            //Reset Tip and PlayButton when session ended
            Logger.Debug("Session Ended, Setting Tip and Allowing CanPlay");
            SetTip(() => Resources.StartTip_Button);
            CanPlay = true;
        }

        public void Handle(IUISessionStartedEvent message)
        {
            //Stop Animation when session started to not consume GPU cycles
            Logger.Debug("Session Started, Disabling Animation on Play Button");
            _loginAttemptInProgress = false;
            _loginAttemptTimer.IsEnabled = false;
            _playButton.IsBusy = false;
        }

        public new void Dispose()
        {
            _messageBroker.Unsubscribe(this);
            LanguageSelect?.Dispose();
            base.Dispose();
        }
        public void Handle(IUILanguageChangedEvent message) { Tip = _lastTipString.Invoke(); }

        /// <summary>
        /// Is called each time after an LoginAttempt was requested and the Timeout run out
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _loginAttemptTimer_Timeout(object sender, EventArgs e)
        {
            //Disable on each entry
            _loginAttemptTimer.IsEnabled = false;
            if(!_loginAttemptInProgress) return;

            //Reset the Button
            _playButton.IsBusy = false;
            CanPlay = true;
        }

        private async Task ShowTipAsync(Func<string> popupMessageStr, Func<string> doneMessageStr, int waitTimeMs)
        {
            SetTip(popupMessageStr);
            await Task.Delay(waitTimeMs);
            SetTip(doneMessageStr);
        }
        private void SetTip(Func<string> resourceAccess)
        {
            //Ensure Update on UI Thread
            if(Application.Current.Dispatcher.CheckAccess())
            {            
                _lastTipString = resourceAccess;
                Tip = resourceAccess.Invoke();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => SetTip(resourceAccess));
            }
        }
        
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if (_directionalNavigation == null && view is FrameworkElement parent)
            {
                var itemsSelector = UIHelper.GetChildElementByNameFromVisualTree<Selector>(parent, nameof(Items));
                Logger.Debug($"Got {itemsSelector} on {nameof(OnViewLoaded)}");
                _directionalNavigation = new WpfDirectionalNavigation<MenueItemViewModel>(itemsSelector);
            }

            SelectFirstItemIfNothingSelected();
        }

        public void ExecuteSelectedItem()
        {
            Logger.Info( "Try to perform start application from UI entry.");
            SelectedItem?.MenueAction();
        }
        
        private void Navigate(NavigationDirection direction)
        {
            _directionalNavigation?.Navigate(direction, SelectedItem,x=>{SelectedItem = x;});
        }
        
        public void SelectItem(object dataContext)
        {
            if (dataContext is MenueItemViewModel viewModel)
            {
                SelectedItem = viewModel;
            }
        }

        private void SelectFirstItemIfNothingSelected()
        {
            if (!IsActive || Items == null || Items.Count <= 0)
            {
                Logger.Debug($"Ignore to selected first item. Is view active: '{IsActive}', Items: {Items?.Count}.");
                return;
            }
            if (SelectedItem == null)
            {
                SelectedItem = Items.FirstOrDefault();
            }
        }


        private void CreateMenue(IUIMessageBroker messageBroker)
        {
            _playButton = new MenueItemViewModel(
                    messageBroker,
                    () => Resources.Shell_Menue_Games,
                    () =>
                    {
                        if(CanPlay) Play();
                    },
                    Application.Current.Resources["IconMenuePlayWhite"] as ImageSource,
                    Application.Current.Resources["IconMenuePlayBlack"] as ImageSource);
            _playButton.IsEnabled = CanPlay;

            Items.Add(_playButton);
            Items.Add(new MenueItemViewModel(
                    messageBroker,
                    ()=>Resources.Shell_Menue_Settings,
                    _stationController.RequestAdminAccess,
                    Application.Current.Resources["IconMenueSettingsWhite"] as ImageSource,
                    Application.Current.Resources["IconMenueSettingsBlack"] as ImageSource));
            Items.Add(new MenueItemViewModel(
                    messageBroker,
                    ()=>Resources.Shell_Menue_PowerOff,
                    PowerOff,
                    Application.Current.Resources["IconMenueShutdownWhite"] as ImageSource,
                    Application.Current.Resources["IconMenueShutdownBlack"] as ImageSource));
            Items.Add(new MenueItemViewModel(
                    messageBroker,
                    ()=>Resources.Shell_Menue_ExitToWindows,
                    ShutDown,
                    Application.Current.Resources["IconMenueWindowsWhite"] as ImageSource,
                    Application.Current.Resources["IconMenueWindowsBlack"] as ImageSource));
        }

        private void ShutDown()
        {
            var viewModel = _viewModelFactory.Build(DialogType.AttemptToShutdown);
            var result = _windowManager.ShowDialog(viewModel, null, ShellClientHelper.GetUniversalDialogSettings());
            if (result != true)
            {
                return;
            }
            _stationController.RequestShutdown();
        }

        private void PowerOff()
        {
            var viewModel = _viewModelFactory.Build(DialogType.AttemptToPowerOff);
            var result = _windowManager.ShowDialog(viewModel, null, ShellClientHelper.GetUniversalDialogSettings());
            if (result != true)
            {
                return;
            }
            _stationController.RequestPowerOff();
        }
    }
}
