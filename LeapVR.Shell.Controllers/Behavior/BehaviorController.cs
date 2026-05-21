#region Licence
/****************************************************************
 *  Filename: BehaviorController.cs
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
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.FileConfig;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.RemoteService;
using LeapVR.Shell.Controllers.Station;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Modules.Interfaces.Vr;
using NLog;

namespace LeapVR.Shell.Controllers.Behavior
{
    //<inheritdoc />
    public class BehaviorController : IBehaviorController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _lock = new object();
        private Task _loopTask;
        private IUISession _currentSession;
        private HmdActivityStatus _hmdActivityStatus;
        private DateTime? _autoLogoutAt;
        private readonly IVirtualRealityController _virtualRealityController;
        private IDisposable _watchDogSubscription;

        private IDisposable[] _shortcutWatchesSubscriptions;
        private ControllerShortcutWatch[] _shortcutWatches;

        private int _isOpenVrRestartInProgressFlag; // 0 = false, 1 = true
        private int _isQuitGameInProgressFlag; // 0 = false, 1 = true

        private readonly BehaviorConfig _behaviorConfig;
        private readonly StationController _stationController;
        private readonly RemoteServiceController _remoteServiceController;

        public BehaviorController(
                IConfigFileRepository<BehaviorConfig> behaviorConfigRepo,
                StationController stationController,
                RemoteServiceController remoteServiceController,
                IVirtualRealityController virtualRealityController,
                IPlatformController platformController)
        {
            QuickLeap.AssertNotNull(
                    behaviorConfigRepo,
                    stationController,
                    platformController,
                    virtualRealityController);
            _behaviorConfig = behaviorConfigRepo.Get();
            _stationController = stationController;
            _remoteServiceController = remoteServiceController;
            _virtualRealityController = virtualRealityController;
            _virtualRealityController.WhenVrModuleStateChanged.Subscribe(OnOpenVrRunningChanged);

            _stationController.WhenSessionStarted.Subscribe(OnSessionStarted);
            _loopTask = AutoLogOutCheckAsync().Forget();
        }

        private void OnOpenVrRunningChanged(VrModuleState moduleState)
        {
            Logger.Debug($"VR Module change detected; State = {moduleState}.");

            //Get Rid of any possible old subscription
            _watchDogSubscription?.Dispose();

            //Check for the right state to get a new subscription
            if(moduleState == VrModuleState.Started &&
               _virtualRealityController.ActiveVrModule is IOpenVrModule openVrModule)
            {
                try
                {
                    if(openVrModule.GetWatchdog(out var watchdog))
                    {
                        _watchDogSubscription = watchdog.WhenEventOccures.Subscribe(OnOpenVrEventOccured);
                        MonitorShortcuts(
                                watchdog,
                                _behaviorConfig.IsVrResetEnabled,
                                _behaviorConfig.VrResetConditions,
                                _behaviorConfig.IsQuitGameShortcutsEnabled,
                                _behaviorConfig.QuitGameShortcuts);

                        Logger.Debug("HMD Activity watchdog successfuly created.");
                    }
                    else
                    {
                        Logger.Warn("Could not get Watchdog from OpenVRModule!");
                    }
                }
                catch(Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        /// <summary>
        /// Evaluates if specific monitoring options are enabled and creates subscriptions
        /// to listen to this specific events 
        /// </summary>
        /// <param name="hmdActivityWatchdog">The HMD activity watchdog.</param>
        /// <param name="isVrResetEnabled">if set to <c>true</c> resetting the VR Driver/Environment by a special button combo is enabled.</param>
        /// <param name="vrResetConditions">The configured Button Combination/Condition that needs to be meet to trigger the VR Reset actionS.</param>
        /// <param name="isQuitGameShortcutEnabled">if set to <c>true</c> a buttom combo can be used to quite a VR Game.</param>
        /// <param name="quitGameConditions">The configured Button Combination/Condition that needs to be meet to trigger the quit game action.</param>
        private void MonitorShortcuts(
                IHmdActivityWatchdog hmdActivityWatchdog,
                bool isVrResetEnabled,
                ControllerShortcutCondition[] vrResetConditions,
                bool isQuitGameShortcutEnabled,
                ControllerShortcutCondition[] quitGameConditions)
        {
            Logger.Trace(
                    $"MonitorShortcuts with isVrResetEnabled={isVrResetEnabled}, isQuitGameShortcutEnabled={isQuitGameShortcutEnabled}");
            QuickLeap.AssertNotNull(hmdActivityWatchdog);

            foreach(var subscription in _shortcutWatchesSubscriptions ?? new IDisposable[0])
            {
                subscription.Dispose();
            }

            foreach(var controllerShortcutWatch in _shortcutWatches ?? new ControllerShortcutWatch[0])
            {
                controllerShortcutWatch.Dispose();
            }

            var shortcutWatchesList = new List<ControllerShortcutWatch>();
            var shortcutWatchesSubscriptionsList = new List<IDisposable>();

            //When Option VRReset is Enabled, we will subscribe to Event's that will fire when
            //this condition is reached
            if(isVrResetEnabled)
            {
                var vrResetShortcutWatches = vrResetConditions.Select(
                                                                       q => new ControllerShortcutWatch(
                                                                               _stationController,
                                                                               _remoteServiceController,
                                                                               hmdActivityWatchdog,
                                                                               q.Scope,
                                                                               q.KeyActions)).
                                                               ToArray();
                shortcutWatchesList.AddRange(vrResetShortcutWatches);

                foreach(var vrResetShortcutWatch in vrResetShortcutWatches)
                {
                    //Subscribe to Event
                    var subscription =
                            vrResetShortcutWatch.WhenIsSatisfiedChanged.Where(q => q).
                                                 Subscribe(q => OnVrResetConditionSatisfied());
                    //Remember subscription
                    shortcutWatchesSubscriptionsList.Add(subscription);
                }
            }

            //When Quit Game Option is enabled
            if(isQuitGameShortcutEnabled)
            {
                var quitGameShortcutWatches = quitGameConditions.Select(
                                                                         q => new ControllerShortcutWatch(
                                                                                 _stationController,
                                                                                 _remoteServiceController,
                                                                                 hmdActivityWatchdog,
                                                                                 q.Scope,
                                                                                 q.KeyActions)).
                                                                 ToArray();
                shortcutWatchesList.AddRange(quitGameShortcutWatches);

                foreach(var quitGameShortcutWatch in quitGameShortcutWatches)
                {
                    //Subscribe to Event
                    var subscription =
                            quitGameShortcutWatch.WhenIsSatisfiedChanged.Where(q => q).
                                                  Subscribe(q => OnQuitGameConditionSatisfied());

                    shortcutWatchesSubscriptionsList.Add(subscription);
                }
            }

            //Base Instances
            _shortcutWatches = shortcutWatchesList.ToArray();

            //Subscriptions to base Instances
            _shortcutWatchesSubscriptions = shortcutWatchesSubscriptionsList.ToArray();
        }

        /// <summary>
        /// Evaluates the inactivity time and triggers an Session Logout if conditions are meet.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">ShouldLogout is set to true, but session is null.</exception>
        private async Task AutoLogOutCheckAsync()
        {
            while(true)
            {
                bool shouldLogout;
                IUISession session;
                TimeSpan autoLogoutNeededNotEarlierThanIn;
                lock(_lock)
                {
                    var now = DateTime.UtcNow;
                    session = _currentSession;
                    shouldLogout = _autoLogoutAt != null && now >= _autoLogoutAt;

                    if(shouldLogout)
                    {
                        // Reset _autoLogoutAt when decided to perform auto-logout.
                        _autoLogoutAt = null;
                    }

                    autoLogoutNeededNotEarlierThanIn = _autoLogoutAt != null
                            ? _autoLogoutAt.Value - now
                            : _behaviorConfig.HmdInactivityToLogout;
                }

                if(shouldLogout)
                {
                    Logger.Debug("BehaviorController decided to AUTO LOGOUT user due to inactivity.");
                    if(session == null)
                    {
                        throw new InvalidOperationException("ShouldLogout is set to true, but session is null.");
                    }

                    Logger.Info("Requesting Session Stop due to Inactivity detected by VR-HMD");
                    session.RequestStopSession(SessionStopReason.StationInactivity);
                }

                // Since we know for sure that we won't need auto-logout earlier than in X time, we can safely wait that long.
                Logger.Debug($"AutoLogout Check Done, going to wait until {autoLogoutNeededNotEarlierThanIn}");
                await Task.Delay(autoLogoutNeededNotEarlierThanIn);
            }
        }

        /// <summary>
        /// Called when [quit game condition satisfied] to quit a game.
        /// </summary>
        private void OnQuitGameConditionSatisfied()
        {
            // Schedule stop game, but don't wait until it is done.
            Task.Run(
                         () =>
                         {
                             var wasQuitGameInProgress = QuickLeap.OperateInterlockedFlag(
                                     ref _isQuitGameInProgressFlag,
                                     true);
                             if(wasQuitGameInProgress)
                             {
                                 // If game is quitting currently don't try to stop it again.
                                 return;
                             }

                             try
                             {
                                 Logger.Info(
                                         "Quit Game request detected trying to request Termination of all application");
                                 _stationController.TerminateApps();
                             }
                             finally
                             {
                                 QuickLeap.OperateInterlockedFlag(ref _isQuitGameInProgressFlag, false);
                             }
                         }).
                 Forget();
        }

        /// <summary>
        /// Called when [vr reset condition satisfied] to reset VR Station State.
        /// </summary>
        private void OnVrResetConditionSatisfied()
        {
            // Schedule restart, but don't wait until it is done.
            Task.Run(
                         () =>
                         {
                             var wasOpenVrRestartInProgress = QuickLeap.OperateInterlockedFlag(
                                     ref _isOpenVrRestartInProgressFlag,
                                     true);
                             if(wasOpenVrRestartInProgress)
                             {
                                 // If OpenVR is restarting currently don't try to restart it again.
                                 return;
                             }

                             try
                             {
                                 Logger.Info("Requesting Reset Station State");
                                 _stationController.ResetStationState();
                             }
                             finally
                             {
                                 // Notify that restarting has stoped.
                                 QuickLeap.OperateInterlockedFlag(ref _isOpenVrRestartInProgressFlag, false);
                             }
                         }).
                 Forget();
        }

        /// <summary>
        /// Called when [open vr event occured].
        /// </summary>
        /// <param name="openVrEvent">The open vr event.</param>
        private void OnOpenVrEventOccured(IOpenVrEvent openVrEvent)
        {
            switch(openVrEvent)
            {
                case IHmdActivityEvent hmdActivityEvent:
                    OnHmdActivityChanged(hmdActivityEvent.Status);
                    break;

                case IControllerButtonActionEvent controllerButtonActionEvent:
                    OnControllerButtonActionEvent(controllerButtonActionEvent);
                    break;
                default:
                    Logger.Trace(
                            $"Unsupported openVrEvent type/value: openVrEvent = `{openVrEvent}`, openVrEvent?.GetType() = `{openVrEvent?.GetType()}`.");
                    break;
            }
        }

        /// <summary>
        /// Called when [HMD activity changed].
        /// </summary>
        /// <param name="newState">The new state.</param>
        private void OnHmdActivityChanged(HmdActivityStatus newState)
        {
            Logger.Debug($"HMD activity status changed, NewState = `{newState}`.");
            lock(_lock)
            {
                _hmdActivityStatus = newState;
                RecalculateAutoLogoutAt();
            }
        }

        private void OnControllerButtonActionEvent(IControllerButtonActionEvent controllerButtonActionEvent)
        {
            //
        }

        /// <summary>
        /// Called when [session started].
        /// </summary>
        /// <param name="newSession">The new session.</param>
        private void OnSessionStarted(IUISession newSession)
        {
            Logger.Debug("New Session start detected");
            lock(_lock)
            {
                Logger.Info($"Setting Session to Session with LoginIntentionId = {newSession.SessionId}");
                _currentSession = newSession;
                newSession.WhenSessionUpdated.Where(x => x.StopReason != null).Subscribe(OnCurrentSessionStopped);
                RecalculateAutoLogoutAt();
            }
        }

        /// <summary>
        /// Called when [current session stopped].
        /// </summary>
        /// <param name="session">The session.</param>
        private void OnCurrentSessionStopped(IUISession session)
        {
            Logger.Info($"Session Stop detected of Session with  with LoginIntentionId = {session.SessionId}");
            lock(_lock)
            {
                _currentSession = null;
                RecalculateAutoLogoutAt();
            }
        }

        /// <summary>
        /// Recalculates the automatic logout Time.
        /// </summary>
        private void RecalculateAutoLogoutAt()
        {
            // must be executed in lock(_lock) context

            if(!_behaviorConfig.IsHmdInactivityLogoutEnabled)
            {
                _autoLogoutAt = null;
                Logger.Debug(
                        $"NoAutoLogout required, its not enabled! IsHmdInactivityLogoutEnabled = {_behaviorConfig.IsHmdInactivityLogoutEnabled}.");
                return;
            }

            // If no session running or HMD is not inactive then no auto-logout needed.
            if(_currentSession == null || _hmdActivityStatus != HmdActivityStatus.Inactive)
            {
                _autoLogoutAt = null;
                Logger.Debug(
                        $"No AutoLogout Required, no session or improper status! _currentSession = `{_currentSession}`, _hmdActivityStatus = `{_hmdActivityStatus}`.");
                return;
            }

            // If _autoLogoutAt is already set then just keep that value.
            if(_autoLogoutAt != null)
            {
                Logger.Debug(
                        $"AutoLogout already set, not adjustment needed! _currentSession = `{_currentSession}`, _hmdActivityStatus = `{_hmdActivityStatus}`.");
                return;
            }

            // Set _autoLogoutAt at configured time from now.
            _autoLogoutAt = DateTime.UtcNow + _behaviorConfig.HmdInactivityToLogout;
            Logger.Info(
                    $"AutoLogout calculated for `{_autoLogoutAt}`; _currentSession = `{_currentSession}`, _hmdActivityStatus = `{_hmdActivityStatus}`.");
        }
    }
}