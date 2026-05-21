#region Licence
/****************************************************************
 *  Filename: SteamApplicationExecution.cs
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
using System.Diagnostics;
using System.Threading;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Utilities.Steam.Steam;
using NLog;

// ReSharper disable ExplicitCallerInfoArgument

namespace LeapVR.Shell.Modules.Platform.Steam
{
    /// <summary>
    /// Application Execution Class for Steam Platform
    /// </summary>
    public class SteamApplicationExecution : BaseApplicationExecution
    {
        private const int TimeoutSecToStartSteam = 60;
        private const int TimeoutSecToStartApp = 60;
        private const int RetryExecutionAfterUpdateSec = 10;
        private const int TimeoutSecToQuitSteam = 10;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAppPlatformData _appPlatformData;
        private readonly SteamLib _steamLib;
        private SteamSelf _steamSelf;

        #region Constructors
        public SteamApplicationExecution(
                IAppPlatformInfo platformAppInfo,
                IAppPlatformData platformData,
                IProcessExecutionLogic logicToExecute,
                SteamLib steamLib,
                IUIMessageBroker messageBroker
        )
                : base(
                        platformAppInfo,
                        logicToExecute,
                        messageBroker
                )
        {
            _appPlatformData = platformData;
            _steamLib = steamLib;
        }
        #endregion Methods

        protected override bool IsStartable()
        {
            Logger.Debug(
                    $"Evaluating if Steam Game is Startable with Guid={DisplayInfo.ApplicationGuid}, Platform Id ={DisplayInfo.PlatformAppId}");
            //Get License
            if(IsLicenseRequired)
            {
                if(!GetAccount(out _, out var accountAccess))
                {
                    Logger.Warn(
                            $"Could not receive Account for Steam App with Guid={DisplayInfo.ApplicationGuid}, Platform Id ={DisplayInfo.PlatformAppId}");
                    return false;
                }

                //Prepare AppStart
                _steamSelf = new SteamSelf(_steamLib, accountAccess, DisplayInfo.PlatformAppId);
                var isGameInstalled = _steamSelf.IsGameInstalled();
                if(!isGameInstalled)
                {
                    Logger.Error(
                            $" SteamGame with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId} seems to be not installed");
                }

                return _steamSelf.IsGameInstalled();
            }

            Logger.Error("Steam Applications need all time an License but IsLicenseRequired returned false!");
            return false;
        }

        protected override void OnPlatformStart()
        {

            bool startSteamResult = false;
            bool startGameResult= false;
            try
            {
                //Do Publishing
                Logger.Debug(
                        $"Starting with OnPlatformStart with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                StateProvider.Publish(PlatformState.StartingClient);

                //Start Steam with Account
                if(!_steamSelf.StartApp(LogicToExecute))
                {
                    StateProvider.Publish(PlatformState.StartingClientError);
                    Logger.Error(
                            $"Error during Starting of Steam with Account for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                    throw new OnPlatformStartException("Could not receive Steam-Account to Start Client!");
                }

                //Watch Steam
                startSteamResult = WaitForSteamStart(
                        _steamSelf,
                        StateProvider,
                        TimeSpan.FromSeconds(TimeoutSecToStartSteam));

                if(!startSteamResult)
                {
                    StateProvider.Publish(PlatformState.StartingClientError);
                    throw new OnPlatformStartException("Steam-Client could not be started");
                }

                //Watch Application Start
                startGameResult = WaitForSteamGameStart(
                        _steamSelf,
                        StateProvider,
                        TimeSpan.FromSeconds(TimeoutSecToStartApp));

                if(!startGameResult)
                {
                    StateProvider.Publish(PlatformState.StartingApplicationError);
                    throw new OnPlatformStartException("Start of Steam Application aborted!");
                }
            }
            finally
            {
                Logger.Debug($"Finished OnPlatformStart with startSteamResult={startSteamResult}, startGameResult={startGameResult}, WasCanceled ={WasCanceled}");
            }
        }

        protected override void OnPlatformStop()
        {
            Logger.Debug("Call to OnPlatformStop");
            StateProvider.Publish(PlatformState.StoppingClient);
            _steamSelf?.QuitSteam(TimeoutSecToQuitSteam);
        }

        /// <summary>
        /// Waits until the Steam Client is successfully started and provides notifications through <see cref="IPlatformStateProvider"/>
        /// </summary>
        /// <param name="steamWrapper">The steam wrapper.</param>
        /// <param name="stateProvider">The state provider.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        private bool WaitForSteamStart(
                SteamSelf steamWrapper, IPlatformStateProvider stateProvider, TimeSpan timeout)
        {
            //Watch Steam
            bool hasTimeout;
            var stopwatch = Stopwatch.StartNew();
            AppState steamState;
            AppState lastSteamState = AppState.Unknown;
            Logger.Debug(
                    $"Starting to watch Steam to detect when Client is ready with Timeout={timeout} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
            
            //CancellationToken that can be used when Updates are required to cancel a long running Update
            CancellationToken? tokenUpdate = null;

            do
            {
                //Check Timeout
                hasTimeout = timeout < stopwatch.Elapsed;
                if(hasTimeout)
                {
                    Logger.Debug("Loop break due to Timeout!");
                    break;
                }

                //Cancellation Request Detection
                if(tokenUpdate != null && tokenUpdate.Value.IsCancellationRequested)
                {
                    Logger.Info("Cancelation of Client Update requested!");
                    return false;
                }

                steamState = steamWrapper.SteamState;
                switch(steamState)
                {
                    case AppState.Ready:
                        Logger.Debug(
                                $"Watch Steam detected State={AppState.Ready} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                        break;

                    case AppState.Launching:
                        if(lastSteamState != AppState.Launching)
                        {
                            Logger.Debug(
                                    $"Watch Steam detected State={AppState.Launching} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                            stateProvider.Publish(PlatformState.StartingClient);
                        }
                        break;

                    case AppState.Updating:
                        if(lastSteamState != AppState.Updating)
                        {
                            //Increase TimeOut
                            Logger.Debug(
                                    $"Watch Steam detected State={AppState.Updating} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}, Timeout will have no effect during Update");
                            stopwatch.Restart();
                            tokenUpdate = stateProvider.PublishCancelable(PlatformState.UpdateingClient);
                        }
                        else
                        {
                            //Don't occupy the CPU
                            Thread.Sleep(1000);
                            //As long we update Steam we don't Timeout
                            stopwatch.Restart();
                        }
                        break;

                    case AppState.Running:
                        if(lastSteamState != AppState.Running)
                        {
                            Logger.Debug(
                                    $"Watch Steam detected State={AppState.Running} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                            stateProvider.Publish(PlatformState.LoggingIn);
                        }
                        break;

                    case AppState.Exited:
                        Logger.Debug(
                                $"Watch Steam detected State={AppState.Exited} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                        break;
                }

                lastSteamState = steamState;
            } while(!WasCanceled &&
                    steamState != AppState.Running &&
                    steamState != AppState.Exited);

            //Check for Timeout
            if(hasTimeout)
            {
                Logger.Error(
                        $"Start of Steam Client has Timeout with last AppState ={lastSteamState} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                return false;
            }

            //Check for success
            if(lastSteamState != AppState.Running)
            {
                Logger.Warn($"Steam State={lastSteamState} but expecting Running! Starting Client Error Detected!");
                return false;
            }
            Logger.Debug("Sucessfully detected Steam Start!");
            return true;
        }

        /// <summary>
        /// Waits for a Steam Game to Start and provides notifications through <see cref="IPlatformStateProvider"/>
        /// </summary>
        /// <param name="steamWrapper">The steam wrapper.</param>
        /// <param name="stateProvider">The state provider.</param>
        /// <param name="timeout">The timeout for the startup procedure.</param>
        /// <returns></returns>
        private bool WaitForSteamGameStart(
                SteamSelf steamWrapper, IPlatformStateProvider stateProvider, TimeSpan timeout)
        {
            bool hasTimeout;
            bool wasExecutedAfterUpdate = false;
            var stopwatchExecuteAfterUpdateRetry = new Stopwatch();
            AppState gameState;
            AppState lastGameState = AppState.Unknown;
            Logger.Debug(
                    $"About to start Application with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}, setting Timout to {timeout}");

            //Publish that we are about to Start the Application
            stateProvider.Publish(PlatformState.StartingApplication);

            //StopWatch to detect general Timeout of an Operation
            var timeoutStopwatch = Stopwatch.StartNew();

            //CancellationToken that can be used when Updates are required to cancel a long running Update
            CancellationToken? tokenUpdate = null;

            //A Flag indicating if an Update was detected
            bool wasUpdateDetected = false;

            //Handling Loop and Detection
            do
            {
                //Set Current Game State for Evaluation
                gameState = steamWrapper.GameState;

                //Timeout Detection
                hasTimeout = timeout < timeoutStopwatch.Elapsed;
                if(hasTimeout)
                {
                    Logger.Debug("Loop break due to Timeout!");
                    break;
                }

                //Cancellation Request Detection
                if(tokenUpdate != null && tokenUpdate.Value.IsCancellationRequested)
                {
                    Logger.Info("Cancelation of Application Update requested!");
                    return false;
                }

                //Retry Execution after Update Detection
                if(wasUpdateDetected && wasExecutedAfterUpdate)
                {
                    if(stopwatchExecuteAfterUpdateRetry.Elapsed.TotalSeconds >= RetryExecutionAfterUpdateSec)
                    {
                        wasExecutedAfterUpdate = false;
                        Logger.Debug("Resetting Flag for Execution After Update to retry Execution!");
                    }
                }

                //Log only State Changes
                if(lastGameState != gameState)
                {
                    Logger.Debug(
                            $"Detected GameState = {gameState} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                }

                //Handle States
                switch(gameState)
                {
                    //This State is Received on Start, after an Finished Update
                    //or if the Running State from Running to not Running
                    case AppState.Ready:
                        if(wasUpdateDetected)
                        {
                            //We try to catch false positives were Update is reported with AppReady state but then switch to Launching
                            //This Sleep tries to wait for the time in that the GameState would change
                            Thread.Sleep(1000);
                            if(steamWrapper.GameState == AppState.Ready && !wasExecutedAfterUpdate)
                            {
                                Logger.Debug("Detected Ready State after Update State, trying to execute app after update");
                                //State after an Update, we need now to initialize close of an Dialog and Start of the App
                                wasExecutedAfterUpdate = steamWrapper.ExecuteStartAfterUpdate(_appPlatformData.ApplicationName);
                                stopwatchExecuteAfterUpdateRetry.Restart();
                                Thread.Sleep(150);
                                continue;
                            }
                            Logger.Warn("Detected Update false positive");
                        }
                        break;

                    case AppState.Launching:
                        if(lastGameState != AppState.Launching)
                        {
                            stateProvider.Publish(PlatformState.StartingApplication);
                        }

                        break;

                    case AppState.Running:
                        if(lastGameState != AppState.Running)
                        {
                            stateProvider.Publish(PlatformState.ApplicationRunning);
                        }

                        break;

                    case AppState.Updating:
                        if(!wasUpdateDetected)
                        {
                            tokenUpdate = stateProvider.PublishCancelable(PlatformState.ApplicationUpdateRequired);
                            wasUpdateDetected = true;
                        }
                        //Dont occupy the CPU during an Update
                        else
                        {
                            Thread.Sleep(1000);
                            //Reset StopWatch to prevent Timeout
                        }
                        timeoutStopwatch.Reset();
                        break;
                    case AppState.Exited:
                        break;
                }

                //Preserve the last provided Game State for Evaluation in Next Loop
                lastGameState = gameState;

            } while(!WasCanceled &&
                    gameState != AppState.Running &&
                    gameState != AppState.Exited);

            if(hasTimeout)
            {
                Logger.Error(
                        $"Start of Steam Application has Timeout with last AppState ={lastGameState} for App with Guid={DisplayInfo.ApplicationGuid},  Platform Id ={DisplayInfo.PlatformAppId}");
                return false;
            }

            //Check for success
            if(lastGameState == AppState.Running)
            {
                Logger.Debug("Sucessfully detected Application Start!");
                return true;
            }
            Logger.Warn($"Steam State={lastGameState} but expecting Running! Starting Client Error Detected!");
            return false;
        }
    }
}