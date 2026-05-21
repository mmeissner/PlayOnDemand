#region Licence
/****************************************************************
 *  Filename: SteamGame.cs
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
#region Using Directives
using System;
using System.Diagnostics;
using System.Threading;
using LeapVR.Utilities.Windows;
using NLog;
#endregion

namespace LeapVR.Utilities.Steam.Steam
{
    public class SteamGame : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private volatile AppState _appState;
        private Process _gameProcess;
        private readonly object _lockStateObj = new object();
        private readonly int _appId;
        private readonly string _steamExeFilePathName;
        private readonly string _exeParameter;
        private bool _firstRegistryChange = true;
        private RegistryWatcherMultiValue _watcher;

        public SteamGame(int appId, string steamExeFilePathName, string exeParameter)
        {
            _steamExeFilePathName = steamExeFilePathName;
            _appId = appId;
            _exeParameter = CreateExeParameter(appId,exeParameter);
            if (_appId == 0)
            {
                Logger.Debug($"AppId could not be resolved from parameter: {exeParameter}, setting GameState to Unknown");
                _appState = AppState.Unknown;
                return;
            }
            //Is the Game Installed ?
            var installed = RegistryUtil.GetValueData(
                $"HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam\\Apps\\{_appId}", "Installed");
            if (installed == null || installed == "0")
            {
                Logger.Debug($"Game seems to be not installed! Received data = {installed}");
                _appState = AppState.NotInstalled;
                return;
            }
            Logger.Debug($"Setting GameState to Ready");
            _appState = AppState.Ready;
        }

        public AppState State => _appState;
        public Process GameProcess => _gameProcess;
        public bool PrepareGameStart()
        {
            if (_appState != AppState.Ready)
            {
                Logger.Debug($"Returning false as GameState Not Ready");
                return false;
            }
            _watcher = new RegistryWatcherMultiValue($"HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam\\Apps\\{_appId}",
                new[] { "Launching", "Running", "Updating" });
            if (!_watcher.IsObserving)
            {
                Logger.Warn($"Watcher has not found any values for observation!");
                _watcher.Dispose();
                return false;
            }
            _watcher.OnRegistryEntryChanged += RegistryWatcherOnOnRegistryEntryChanged;
            return true;
        }

        public void Dispose()
        {
            _watcher?.Stop();
        }

        public StartState GameStart(double timeoutInSec)
        {
            var stopwatch = new Stopwatch();
            AppState appState;
            bool timeOut = false;
            Logger.Debug("Started to Watching Game State");

            //One round takes at least one second and we want to ensure at least one round
            if (timeoutInSec <= 0) timeoutInSec = 1;

            stopwatch.Start();
            _gameProcess  = Process.Start(_steamExeFilePathName, _exeParameter);
            while ((appState = State) != AppState.Running && !(timeOut = stopwatch.Elapsed.TotalSeconds >= timeoutInSec))
            {
                Logger.Debug($"Gamestate is:{appState}");
                Thread.Sleep(1000);
                switch (appState)
                {
                    case AppState.Updating:
                        Logger.Debug("Game seems to need RestartByLauncher!");
                        return StartState.NeedsUpdate;
                    case AppState.Launching:
                        Logger.Debug("Game seems to be launching!");
                        continue;
                    case AppState.Running:
                        break;
                }
            }
            if (timeOut && appState != AppState.Running)
            {
                Logger.Debug("Timeout during StartState");
                return StartState.StartFailed;
            }
            Logger.Debug("Returing Successful Start!");
            return StartState.StartSuccessful;
        }

        private string CreateExeParameter(int id,string exeParameter)
        {
            //if (!exeParameter.ToLowerInvariant().Contains("-novid")) exeParameter = $"-novid {exeParameter}";
            //if (!exeParameter.ToLowerInvariant().Contains("-silent")) exeParameter = $"-silent {exeParameter}";
            //C:\Steam\Steam.exe -applaunch 253030 vr

            var param= $"-applaunch {id}";
            if(!String.IsNullOrEmpty(exeParameter)) param = param + $" {exeParameter}";
            return param;
        }

        private void RegistryWatcherOnOnRegistryEntryChanged(string value, string oldData, string newData)
        {
            //Handle State changes
            HandleStates(value, oldData, newData);
        }

        private void HandleStates(string value, string oldData, string newData)
        {
            lock (_lockStateObj)
            {
                //Background Information
                //Launching and Running can be for some time 1 at the same time (observed with HordeZ)!
                Logger.Debug($"Receiving for Value: {value}, OldState:{oldData} , NewState{newData}");

                //if its the first registry change then we do know that steam touched the game and we set it to
                //Launching as it might be an cleanup of old values
                if (_firstRegistryChange)
                {
                    _appState = AppState.Launching;
                    _firstRegistryChange = false;
                }
                switch (value)
                {
                    case "Launching":
                        //Is it a switch from 0 -> 1 then the Game is Launching
                        if (oldData == "0" && newData == "1")
                        {
                            Logger.Debug($"Setting GameState to :{AppState.Launching}");
                            _appState = AppState.Launching;
                        }
                        else if (oldData == "1" && newData == "0")
                        {
                            Logger.Debug($"No GameState Change");
                        }
                        break;
                    case "Running":
                        if (oldData == "0" && newData == "1")
                        {
                            _appState = AppState.Running;
                            Logger.Debug($"Setting GameState to :{AppState.Running}");
                        }
                        else if (oldData == "1" && newData == "0")
                        {
                            Logger.Debug($"Setting GameState to :{AppState.Ready}");
                            _appState = AppState.Ready;
                        }
                        break;
                    case "Updating":
                        if (oldData == "0" && newData == "1")
                        {
                            Logger.Debug($"Setting GameState to :{AppState.Updating}");
                            _appState = AppState.Updating;
                        }
                        else if (oldData == "1" && newData == "0")
                        {
                            Logger.Debug($"Setting GameState to :{AppState.Ready}");
                            _appState = AppState.Ready;
                        }
                        break;
                }
            }
        }
    }
}