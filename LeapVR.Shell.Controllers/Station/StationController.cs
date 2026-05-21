#region Licence
/****************************************************************
 *  Filename: StationController.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.RemoteService;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Authentication;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.System;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Modules.Interfaces.Network;
using NLog;
using Pod.Data.Infrastructure;

// ReSharper disable ExplicitCallerInfoArgument

namespace LeapVR.Shell.Controllers.Station
{
    public sealed class StationController : IStationController
    {

        #region Private Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private volatile HashSet<IApplicationExecution> _applicationsInApplicationExecutions =
                new HashSet<IApplicationExecution>();
        private readonly object _executionLock = new object();
        private readonly object _shutdownSequence = new object();
        private readonly object _sessionLock = new object();

        private readonly Subject<AppExecutionMessage> _whenExecutionMessage = new Subject<AppExecutionMessage>();
        private readonly Subject<StationMessage> _whenStationMessage = new Subject<StationMessage>();
        private readonly Subject<IUISession> _whenSessionStarted = new Subject<IUISession>();
        private readonly Subject<IUISession> _whenSessionStopped = new Subject<IUISession>();

        private readonly IConfigFileRepository<StationConfig> _stationConfigRepo;
        private readonly IPlatformController _platformController;
        private readonly IVirtualRealityController _virtualRealityController;
        private readonly RemoteServiceController _remoteServiceController;
        private readonly IUIMessageBroker _messageBroker;
        
        private NetworkConnectionStatus _currentNetworkStatus = NetworkConnectionStatus.Unknown;

        private volatile bool _currentlyExecuting = false;
        private volatile bool _shutdownSequenceInProgress = false;

        private StationMode _currentMode;

        private Action<StationMessage> _systemMsgCallback;
        private Action<TerminationSignal> _systemTermSignalCallback;
        
        #endregion

        #region Station Properties and Fields
        public StationMode Mode => _currentMode;
        public bool ForceVrDriverRestart => _stationConfigRepo.Get().ForceVrDriverRestart;
        public bool DisableVrDriverInteraction => _stationConfigRepo.Get().DisableVrDriverInteraction;
        public bool CurrentlyExecuting => _currentlyExecuting;
        public Version SoftwareVersion => VersionProvider.SoftwareVersion;
        #endregion

        #region Constructors
        public StationController(
            RemoteServiceController remoteServiceController,
            IGamepadController gamepadController,
            IPlatformController platformController,
            IStatisticsController statisticsController,
            IUIMessageBroker messageBroker,
            IConfigFileRepository<StationConfig> stationConfigRepo,
            INetworkModule networkModule,
            IVirtualRealityController virtualRealityController,
            ISecurityController securityController,
            IEnumerable<IRunLevelMsgReceiver> stationMessageReceiver,
            IEnumerable<IExecutionMessageReceiver> executionMessageReceivers)
        {
            QuickLeap.AssertNotNull(
                    securityController,
                virtualRealityController,
                platformController,
                stationConfigRepo,
                gamepadController,
                networkModule,
                statisticsController,
                    remoteServiceController);
            var stationConfig = stationConfigRepo.Get();

            //Initializing Fields
            _currentMode = stationConfig.DefaultStationMode;
            _remoteServiceController = remoteServiceController;
            _stationConfigRepo = stationConfigRepo;
            _platformController = platformController;
            _virtualRealityController = virtualRealityController;
            _messageBroker = messageBroker;
            networkModule.WhenNetworkConnectionChanged.Subscribe(OnNetworkConnectionChanged);

            //Wire Up all Station Message Receiver
            foreach(IRunLevelMsgReceiver receiver in stationMessageReceiver)
            {
                _whenStationMessage.Subscribe(receiver.OnStationMessage);
            }

            //Wire Up all Execution Message Receivers
            foreach(IExecutionMessageReceiver receiver in executionMessageReceivers)
            {
                _whenExecutionMessage.Subscribe(receiver.OnExecutionMessage);
            }

            //Wire Up Actions for SubControllers
            gamepadController.RegisterResetStationState(ResetStationState);
            gamepadController.RegisterTerminateAllApplications(TerminateApps);

            //Wire Up SessionStopped Subscription
            remoteServiceController.WhenSessionStopped.Subscribe(OnSessionStopped);
        }
        #endregion

        public void ResetStationState()
        {
            //Reset VR
            TerminateApps();
            var currentMode = _virtualRealityController.Mode;
            _virtualRealityController.ChangeMode(VrMode.Off);
            _virtualRealityController.ChangeMode(currentMode);
        }

        public void Initialize(
            Action<StationMessage> msgToSystemCallback,
            Action<TerminationSignal> msgToSystemTermSignalCallback)
        {
            Logger.Debug("Station Controller Initialization Requested");

            //Setup VR Controller
            _virtualRealityController.ForceDriverRestart = ForceVrDriverRestart;
            _virtualRealityController.DisableDriverInteraction = DisableVrDriverInteraction;

            //Get first available VR Module and set it to be used
            var vrModule = _virtualRealityController.AvailableVrModules.FirstOrDefault();
            if(vrModule != null)
            {
                //Preset VR- Mode
                _virtualRealityController.SetActiveVRModuleAsync(vrModule).Wait();
                switch(_currentMode)
                {
                    case StationMode.Screen:
                        _virtualRealityController.ChangeModeAsync(VrMode.OnDemand);
                        break;
                    case StationMode.VirtualReality:
                        _virtualRealityController.ChangeModeAsync(VrMode.AllwaysOn);
                        break;
                }
            }
            //We need to check if the Module is null but in previous run it was available
            else
            {
                if(_currentMode == StationMode.VirtualReality)
                {
                    //Correct the State
                    _currentMode = StationMode.Screen;
                    var systemConfig = _stationConfigRepo.Get();
                    systemConfig.DefaultStationMode = _currentMode;
                    _stationConfigRepo.Store(systemConfig);
                }
            }
            
            //Let Controllers Initialize
            _systemMsgCallback = msgToSystemCallback;
            _systemTermSignalCallback = msgToSystemTermSignalCallback;

            _whenStationMessage.OnNext(StationMessage.InitStart);
            _systemMsgCallback.Invoke(StationMessage.InitStart);
            _whenStationMessage.OnNext(StationMessage.Start);
            _systemMsgCallback.Invoke(StationMessage.Start);
            _whenStationMessage.OnNext(StationMessage.GuiStarted);
            _systemMsgCallback.Invoke(StationMessage.GuiStarted);

        }

        #region Internal Observables        
        /// <summary>
        /// Provides notification when a session was started and the session itself.
        /// </summary>
        /// <value>
        /// The Session that was started
        /// </value>
        internal IObservable<IUISession> WhenSessionStarted => _whenSessionStarted.AsObservable();
        #endregion

        #region Public Methods for Control
        public bool RequestExecution(IExecuteable executeables)
        {
            lock (_executionLock)
            {
                if (_applicationsInApplicationExecutions.Any()) return false;
                var executionObject = _platformController.RequestExecutionObject(executeables);
                if (executionObject == null) return false;
                executionObject.WhenExecutionPhaseChange.Subscribe(_whenExecutionMessage);
                _applicationsInApplicationExecutions.Add(executionObject);
                executionObject.WhenExecutionPhaseChange.Where(x => x.Phase == ExecutionPhase.OnFinished).
                                Subscribe(OnApplicationFinished);
                executionObject.Run();
                return true;
            }
        }
        public void TerminateApps()
        {
            Logger.Info("Station Controller TerminateApps Requested");
            InternalTerminateApps(false);
        }
        public void RequestShutdown()
        {
            Logger.Info("Station Controller RequestShutdown Requested");
            InitializeShutdown(TerminationSignal.Close);
        }
        public void RequestPowerOff()
        {
            Logger.Info("Station Controller RequestPoweroff Requested");
            InitializeShutdown(TerminationSignal.PowerOff);
        }
        public void RequestRestart()
        {
            Logger.Info("Station Controller RequestRestart Requested");
            InitializeShutdown(TerminationSignal.Restart);
        }

        public void RequestAdminAccess()
        {
            _messageBroker.Publish(new UIAdminAccessAttemptEvent());
        }
        public List<StationMode> GetAvailableModes()
        {
            var retval = new List<StationMode> {StationMode.Screen};
            if(_virtualRealityController.AvailableVrModules.Any())
            {
                retval.Add(StationMode.VirtualReality);
            }
            return retval;
        }
        public async Task SetStationModeAsync(StationMode requestedMode)
        {
            var systemConfig = _stationConfigRepo.Get();
            systemConfig.DefaultStationMode = requestedMode;
            _stationConfigRepo.Store(systemConfig);
            switch (requestedMode)
            {
                case StationMode.Screen when _currentMode == StationMode.VirtualReality:
                    await _virtualRealityController.ChangeModeAsync(VrMode.OnDemand);
                    _currentMode = StationMode.Screen;
                    break;
                case StationMode.VirtualReality when _currentMode == StationMode.Screen:
                    await _virtualRealityController.ChangeModeAsync(VrMode.AllwaysOn);
                    _currentMode = StationMode.VirtualReality;
                    break;
            }
        }
        public void SetRestartVrDriver(bool value)
        {
            _virtualRealityController.ForceDriverRestart = value;
            var stationConfig = _stationConfigRepo.Get();
            stationConfig.ForceVrDriverRestart = value;
            _stationConfigRepo.Store(stationConfig);
        }
        public void SetDisableVrDriverInteraction(bool value)
        {
            _virtualRealityController.DisableDriverInteraction = value;
            var stationConfig = _stationConfigRepo.Get();
            stationConfig.DisableVrDriverInteraction = value;
            _stationConfigRepo.Store(stationConfig);
        }
        public void OpenConnectDialog()
        {
            _remoteServiceController.OpenConnectDialog();
        }
        #endregion

        #region Callbacks
        private void OnNetworkConnectionChanged(NetworkConnectionStatus networkConnectionStatus)
        {
            var oldStatus = _currentNetworkStatus;
            _currentNetworkStatus = networkConnectionStatus;
            _messageBroker.Publish(new UINetworkStateChanged(oldStatus, networkConnectionStatus));
        }


        private void OnSessionStopped(IUISession session)
        {
            TerminateApps();
        }

        private void OnApplicationFinished(AppExecutionMessage executionMessage)
        {
            lock(_executionLock)
            {
                _applicationsInApplicationExecutions.Remove(executionMessage.AppExecutionData);
            }
        }

        #endregion

        #region Private Methods
        private void InitializeShutdown(TerminationSignal signal)
        {
            bool result = false;
            try
            {
                //Guard from multiple Shutdowns
                if(_shutdownSequenceInProgress) return;
                lock(_shutdownSequence)
                {
                    if(_shutdownSequenceInProgress) return;
                    _shutdownSequenceInProgress = true;
                }
                //Close all possible apps with Info about Shutdown
                InternalTerminateApps(true);

                //Send SystemMessage to Stop
                _whenStationMessage.OnNext(StationMessage.InitStop);
                _whenStationMessage.OnNext(StationMessage.Stop);
                result = true;
            }
            finally
            {
                if(result)
                {
                    _whenStationMessage.OnNext(StationMessage.Quit);
                    _systemTermSignalCallback?.Invoke(signal);
                }
                Logger.Info(
                    result
                        ? "InitializeShutdown Accepted"
                        : "InitializeShutdown Denied as InitializeShutdown already in Progress");
            }
        }
        private void InternalTerminateApps(bool isSystemShutdown)
        {
            lock(_executionLock)
            {
                Logger.Info("Station Controller TerminateApps Requested");
                foreach(IApplicationExecution execution in _applicationsInApplicationExecutions)
                {
                    execution.Terminate(isSystemShutdown);
                }
            }
        }
        #endregion

        public void Dispose()
        {
            _whenExecutionMessage?.Dispose();
            _whenStationMessage?.Dispose();
        }

    }
}