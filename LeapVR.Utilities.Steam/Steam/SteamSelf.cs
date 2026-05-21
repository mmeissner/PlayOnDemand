#region Licence
/****************************************************************
 *  Filename: SteamSelf.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LeapVR.Shared.Lib.Win.VirtualKeyboard;
using LeapVR.Shared.Lib.Win.WinApi;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Utilities.Windows;
using LeapVR.Utilities.Windows.Processes;
using NLog;
using Timer = System.Timers.Timer;

namespace LeapVR.Utilities.Steam.Steam
{
    /// <summary>
    /// Class can be used for starting steam. However if steam was once in the Running State and Exited,
    /// so it cant be restarted again.
    /// </summary>
    public class SteamSelf
    {
        #region Private Fields
        private const string ClientUpdaterClass = "BootstrapUpdateUIClass";
        private const string AppUpdaterClass = "vguiPopupWindow";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static string sQuitSteamParam = @"-shutdown";
        private volatile bool _loginConfirmed;
        private volatile bool _steamExited;
        private volatile bool _quitRequested;
        private readonly string _username;
        private readonly string _password;
        private readonly ulong _appId;
        private readonly string _exeFilePath;
        private readonly SteamLib _steamLib;
        private readonly ExecutableLibrary _executableLibrary;
        private IProcessExecutionLogic _processExecutionLogic;
        private bool _unregisteredProcess = true;
        private bool _firstGameRegistryChange = true;
        private bool _isGameStart = false;
        private Process _steamProcess;
        private RegistryWatcher _watcherSteamUser;
        private RegistryWatcherMultiValue _watcherApp;
        private volatile AppState _steamAppState;
        private volatile AppState _gameAppState;
        private readonly object _eventLock = new object();
        private Timer _steamClientTimer = null;
        #endregion

        #region Public Properties
        public Process SteamProcess
        {
            get
            {
                lock(_eventLock)
                {
                    return _steamProcess;
                }
            }
        }

        public AppState SteamState => _steamAppState;

        public AppState GameState => _gameAppState;
        #endregion

        #region Constructor
        public SteamSelf(SteamLib steamLib, IAccountAccess accountAccess, ulong appId)
        {
            _username = accountAccess.Username;
            _password = accountAccess.Password;
            _appId = appId;
            _steamLib = steamLib;
            _executableLibrary = new ExecutableLibrary();
            _exeFilePath = _steamLib.SteamExeFilePathName;
            _steamAppState = string.IsNullOrEmpty(_exeFilePath) ? AppState.NotInstalled : AppState.Ready;
            Logger.Debug($"Initialized with AppState = {_steamAppState}");
        }
        #endregion

        #region Public Methods
        public bool IsGameInstalled()
        {
            Logger.Debug($"InitializeGameAppState for Id: {_appId}, setting GameState to Unknown");
            if(_appId == 0)
            {
                _gameAppState = AppState.Unknown;
                return false;
            }

            //Is the Game Installed ?
            var installed = RegistryUtil.GetValueData(
                    $"HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam\\Apps\\{_appId}",
                    "Installed");
            if(installed == null || installed == "0")
            {
                Logger.Debug($"Game seems to be not installed! Received data = {installed}");
                _gameAppState = AppState.NotInstalled;
                return false;
            }

            Logger.Debug($"Setting GameState to Ready");
            _gameAppState = AppState.Ready;
            return true;
        }

        public bool StartApp(IProcessExecutionLogic processExecution)
        {
            Logger.Debug("Starting Steam");
            _processExecutionLogic = processExecution;
            //Allow Start only if ready and not already started
            if(_steamAppState != AppState.Ready && _steamAppState != AppState.Exited) return false;

            //Unregister if we were before once starting
            UnregisterWatcher();

            //Unregister if we were before once starting
            UnregisterProcess();

            //Unregister the Timer if we were before once starting
            UnregisterTimer();

            //Reset Flags
            _loginConfirmed = false;
            _steamExited = false;

            //Get new Steam Process
            NewSteamProcess(GetStartInfo(processExecution));
            _isGameStart = true;

            //Getnew Watcher
            NewSteamWatcher();

            //Prepare the Timer to get an Tick every second
            NewTimer();

            //Start the Watcher
            _watcherSteamUser.Start();

            //Start The Timer
            _steamAppState = AppState.Launching;
            _steamClientTimer.Start();

            //Start the Process
            if(_steamProcess.Start())
            {
                return true;
            }

            //Something went wrong
            UnregisterWatcher();
            UnregisterProcess();
            UnregisterTimer();
            _steamAppState = AppState.Unknown;
            return false;
        }

        public bool ExecuteStartAfterUpdate(string appOriginalName)
        {
            var data = User32WindowUtil.GetWindowData(SteamProcess, new WindowClassNameFilter {ClassName = AppUpdaterClass});
            var myWindow = data?.FirstOrDefault(x => x.WindowTitle.Contains(appOriginalName));
            if(myWindow != null && myWindow.HWnd != IntPtr.Zero)
            {
                if(User32.SetForegroundWindow(myWindow.HWnd))
                {
                    var simulator = new InputSimulator();
                    simulator.Keyboard.KeyPress(VirtualKeyCode.Menu, VirtualKeyCode.F4);
                    Thread.Sleep(150);
                    Process.Start(GetStartInfo(_processExecutionLogic));
                    return true;
                }
            }
            return false;
        }

        public void CheckAndHandleUpdatedDialog()
        {
            var mainWindowHandle = _steamProcess.MainWindowHandle;
            if(mainWindowHandle  != IntPtr.Zero)
            {
                var sb = new StringBuilder(64);
                User32.GetClassName(mainWindowHandle, sb, sb.Capacity);
                var className = sb.ToString();
                if(!string.IsNullOrEmpty(className))
                {
                    var strClassName = className;
                    if(strClassName.Contains(AppUpdaterClass))
                    {
                        _steamAppState = AppState.Updating;
                        Logger.Debug("Detected that Steam is updateing!");
                        return;
                    }
                    else
                    {
                        Logger.Debug($"Received ClassName but its not the Updater Class={AppUpdaterClass}, its {strClassName}!");
                        return;
                    }
                }
                else
                {
                    Logger.Warn("Could not get ClassName, its IsNullOrEmpty!");
                    return;
                }
            }
            Logger.Warn("Could not get Steam MainWindowHandle!");
        }

        /// <summary>
        /// Quits Steam.
        /// </summary>
        public void QuitSteam(int secondsTimeout)
        {
            Logger.Debug($"Start to quit Steam with secondsTimeout={secondsTimeout}");
            _quitRequested = true;
            List<Process> steamProcess;
            //Try to Stop SteamVR if running
            if(!_executableLibrary.FindProcess(_steamLib.SteamProcessName).Any())
            {
                Logger.Debug("No Steam Process to quit");
                return;
            }

            ;
            Process.Start(_steamLib.SteamExeFilePathName, sQuitSteamParam);
            //Check if it is still running
            var stopWatchTimeout = Stopwatch.StartNew();
            while((steamProcess = Process.GetProcessesByName(_steamLib.SteamProcessName).ToList()).Any())
            {
                //If after the Timeout Threshhold steam still didn't closed
                //We will force it
                if(stopWatchTimeout.Elapsed.TotalSeconds >= secondsTimeout)
                {
                    foreach(Process process in steamProcess)
                    {
                        Logger.Warn($"Have to kill process with name {process} as it does not seem to shutdown in the specified time");
                        _executableLibrary.KillProcess(process);
                    }

                    return;
                }
                //Wait a second and then check again
                Thread.Sleep(1000);
            }

            Logger.Debug("Steam seemed to shutdown by shutdown command!");
        }
                
        #endregion

        #region Private Methods
        private void SteamClientTimerCallback(object sender, ElapsedEventArgs e)
        {
            //------Steam Startup Procedure-----
            //-> Steam Starts
            //-> Steam Gets an MainWindowHandle
            //-> Steam might perform an UpdateCheck (each time Steam was killed before, not shutdown properly)
            //-> Steam logs User in

            bool keepTimerAlive = true;
            try
            {
                Logger.Debug("Watchdog Timer: triggered!");
                lock(_eventLock)
                {
                    //Control the Time
                    Logger.Debug($"Watchdog Timer: entered lock with _steamExited={_steamExited},_steamProcess.HasExited={_steamProcess.HasExited}");

                    //Check if Steam is still running
                    //Should catch abnormal Process termination
                    var steamProcess = _steamProcess;
                    if(_steamExited || steamProcess.HasExited)
                    {
                        // The launcher we spawned can exit for benign reasons:
                        //   1. Steam was already running on the box, so steam.exe
                        //      with -applaunch delegates to the running instance
                        //      and the launcher immediately quits.
                        //   2. Steam updated itself and restarted its own process.
                        // In both cases an actual Steam.exe is still running. Only
                        // treat this as a real exit if no Steam process is found.
                        if(!_quitRequested)
                        {
                            bool refreshSuccess;
                            var timeoutWatch = Stopwatch.StartNew();
                            do
                            {
                                refreshSuccess = RefreshSteamProcess();
                            } while(!refreshSuccess && timeoutWatch.Elapsed.TotalSeconds < 15 && !_quitRequested);

                            if(refreshSuccess)
                            {
                                // When adopting an already-running Steam, the
                                // ActiveUser registry value won't change, so the
                                // watcher would never fire _loginConfirmed.
                                // Read it directly to detect a pre-existing
                                // logged-in session.
                                if(!_loginConfirmed)
                                {
                                    var activeUser = RegistryUtil.GetValueData(
                                            @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam\ActiveProcess",
                                            "ActiveUser");
                                    if(!string.IsNullOrEmpty(activeUser) && activeUser != "0")
                                    {
                                        Logger.Info($"Adopted Steam already has user logged in (ActiveUser={activeUser})");
                                        _loginConfirmed = true;
                                    }
                                }
                                _steamAppState = AppState.Launching;
                                Logger.Info("Watchdog Timer: launcher exited but Steam.exe still running - adopted existing process");
                                return;
                            }

                            _steamExited = true;
                            _steamAppState = AppState.Exited;
                            keepTimerAlive = false;
                            Logger.Info("Watchdog Timer: launcher exited and no Steam.exe found - treating as real exit");
                            return;
                        }
                        else
                        {
                            //If it's detected through the process
                            _steamExited = true;
                            _steamAppState = AppState.Exited;
                            keepTimerAlive = false;
                            Logger.Debug("Watchdog Timer: setting KeepAliveTimer to false and _steamExited to true");
                            return;
                        }
                    }
                    else
                    {
                        Logger.Debug("Watchdog Timer: Steam seems still to run");
                    }

                    //Check if Steam is Updating
                    Logger.Debug($"Watchdog Timer: Checking if Steam is updating with current _steamAppState ={_steamAppState}");
                    if(_steamAppState != AppState.Updating)
                    {
                        if(IsSteamUpdating(_steamProcess))
                        {
                            _steamAppState = AppState.Updating;
                            Logger.Info("Watchdog Timer: Detected that Steam is updateing!");
                            return;
                        }
                    }

                    //Check if Steam Update is finished
                    else if(_steamAppState == AppState.Updating)
                    {
                        //Still Updating
                        if(IsSteamUpdating(_steamProcess))return;
                        else
                        {                            
                            Logger.Debug("Steam update seems to be finished!");
                        }
                    }

                    //Check if Steam Logged in User
                    if(!_loginConfirmed)
                    {
                        // The RegistryWatcher misses changes that happen before
                        // it finishes registering its kernel notification, and
                        // also misses changes when Steam writes ActiveUser
                        // before the kiosk's first tick. Poll directly as a
                        // fallback - we already paid the registry-read cost in
                        // IsSteamUpdating above.
                        var activeUser = RegistryUtil.GetValueData(
                                @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam\ActiveProcess",
                                "ActiveUser");
                        if(!string.IsNullOrEmpty(activeUser) && activeUser != "0")
                        {
                            Logger.Info($"Watchdog Timer: Login confirmed via direct registry poll (ActiveUser={activeUser})");
                            _loginConfirmed = true;
                        }
                        else
                        {
                            Logger.Debug("Watchdog Timer: Could not yet confirm that User is logged in, returning and waiting for next cycle!");
                            return;
                        }
                    }
                    else
                    {
                        Logger.Debug($"Watchdog Timer: User Loggin Confirmed! Setting _steamAppState = AppState.Running and setting keepTimerAlive=false");
                    }


                    //Might ensure that Steam is at that time able to 
                    _steamAppState = AppState.Running;
                    keepTimerAlive = false;
                    if(_isGameStart)
                    {
                        CreateAppWatcher();
                    }
                }
            }
            finally
            {
                Logger.Debug($"Watchdog Timer: Finalization of cycle with keepTimerAlive={keepTimerAlive}");
                if(keepTimerAlive)
                {
                    _steamClientTimer?.Start();
                }
                else
                {
                    UnregisterWatcher();
                    UnregisterProcess();
                    UnregisterTimer();
                }
            }
        }

        private bool IsSteamUpdating(Process steamProcess)
        {
            var mainWindowHandle = steamProcess.MainWindowHandle;
            if(mainWindowHandle  != IntPtr.Zero)
            {
                var sb = new StringBuilder(64);
                User32.GetClassName(mainWindowHandle, sb, sb.Capacity);
                var className = sb.ToString();
                if(!string.IsNullOrEmpty(className))
                {
                    var strClassName = className;
                    if(strClassName.Contains(ClientUpdaterClass))
                    {
                        _steamAppState = AppState.Updating;
                        Logger.Debug("Detected that Steam is updateing!");
                        return true;
                    }
                    else
                    {
                        Logger.Debug($"Received ClassName but its not the Updater Class={ClientUpdaterClass}, its {strClassName}!");
                        return false;
                    }
                }
                else
                {
                    Logger.Warn("Could not get ClassName, its IsNullOrEmpty!");
                    return false;
                }
            }
            Logger.Warn("Could not get Steam MainWindowHandle!");
            return false;
        }

        private void _steamProcess_Exited(object sender, EventArgs e)
        {
            //Does not work for abnormal process termination e.g. Task Kill in TaskManager
            Logger.Debug("Event: SteamProcess exited event raised!");
            lock(_eventLock)
            {
                if(!_steamExited)
                {
                    _steamExited = true;
                    Logger.Debug($"Event: Setting  _steamExited = true;");
                }
            }
        }

        private void WatcherSteamUser_OnRegistryEntryChanged(string value, string newdata)
        {
            try
            {
                lock(_eventLock)
                {
                    //If steam did not close properly before, it might first zero the value
                    if(newdata != "0" && !_loginConfirmed)
                    {
                        Logger.Info($"Login confirmed with User Login = {newdata}");
                        _loginConfirmed = true;
                    }
                }
            }
            finally { }
        }

        private void WatcherApp_OnRegistryEntryChanged(string value, string oldData, string newData)
        {

            //Background Information
            //Launching and Running can be for some time 1 at the same time (observed with HordeZ)!
            Logger.Debug($"Receiving for Value: {value}, OldState:{oldData} , NewState{newData}");

            //if its the first registry change then we do know that steam touched the game and we set it to
            //Launching as it might be an cleanup of old values
            if(_firstGameRegistryChange)
            {
                Logger.Debug("First Time Registry change detected, Setting _gameAppState = AppState.Launching");
                _gameAppState = AppState.Launching;
                _firstGameRegistryChange = false;
            }
            //Attention, the values also move back to 0 if steam is closed
            //Its currently unclear how to handle/detect this when close command was caused from outside

            switch(value)
            {
                case "Launching":
                    //Is it a switch from 0 -> 1 then the Game is Launching
                    if(oldData == "0" && newData == "1")
                    {
                        Logger.Debug($"Setting GameState to :{AppState.Launching}");
                        _gameAppState = AppState.Launching;
                    }
                    else if(oldData == "1" && newData == "0")
                    {
                        Logger.Debug($"No GameState Change");
                    }

                    break;
                case "Running":
                    if(oldData == "0" && newData == "1")
                    {
                        _gameAppState = AppState.Running;
                        Logger.Debug($"Setting GameState to :{AppState.Running}");
                    }
                    else if(oldData == "1" && newData == "0")
                    {
                        Logger.Debug($"Setting GameState to :{AppState.Ready}");
                        _gameAppState = AppState.Ready;
                    }

                    break;
                case "Updating":
                    if(oldData == "0" && newData == "1")
                    {
                        Logger.Debug($"Setting GameState to :{AppState.Updating}");
                        _gameAppState = AppState.Updating;
                    }
                    else if(oldData == "1" && newData == "0")
                    {
                        Logger.Debug($"Setting GameState to :{AppState.Ready}");
                        _gameAppState = AppState.Ready;
                    }

                    break;
            }
        }

        private void NewTimer()
        {
            lock(_eventLock)
            {
                Logger.Debug("Creating new Timer!");
                if(_steamClientTimer != null)
                {
                    _steamClientTimer.Stop();
                    _steamClientTimer.Elapsed -= SteamClientTimerCallback;
                    _steamClientTimer.Dispose();
                }

                //Prepare the Timer to get an Tick every second
                _steamClientTimer = new Timer(1000);
                _steamClientTimer.Elapsed += SteamClientTimerCallback;
                _steamClientTimer.AutoReset = false;
            }
        }

        private void NewSteamWatcher()
        {
            lock(_eventLock)
            {
                Logger.Debug("Creating new Steam Watcher!");
                if(_watcherSteamUser != null)
                {
                    _watcherSteamUser.OnRegistryEntryChanged -= WatcherSteamUser_OnRegistryEntryChanged;
                    _watcherSteamUser.Stop();
                }

                //Create a new Watcher
                _watcherSteamUser = new RegistryWatcher(
                        @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam\ActiveProcess",
                        "ActiveUser");
                //Register Events
                _watcherSteamUser.OnRegistryEntryChanged += WatcherSteamUser_OnRegistryEntryChanged;
            }
        }

        private bool CreateAppWatcher()
        {
            lock(_eventLock)
            {
                Logger.Debug("Creating new App Watcher!");
                _watcherApp = new RegistryWatcherMultiValue(
                        $"HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam\\Apps\\{_appId}",
                        new[] {"Launching", "Running", "Updating"});
                if(!_watcherApp.IsObserving)
                {
                    Logger.Warn("Watcher has not found any values for observation!");
                    _watcherApp.Dispose();
                    return false;
                }

                _watcherApp.OnRegistryEntryChanged += WatcherApp_OnRegistryEntryChanged;
                return true;
            }
        }

        private void NewSteamProcess(ProcessStartInfo startInfo)
        {
            lock(_eventLock)
            {
                Logger.Debug("Creating new Steam Process!");
                if(!_unregisteredProcess)
                {
                    _steamProcess.Exited -= _steamProcess_Exited;
                    _steamProcess.Dispose();
                }

                _steamProcess = new Process
                                {
                                        StartInfo = startInfo
                                };
                _unregisteredProcess = false;
                _steamProcess.Exited += _steamProcess_Exited;
            }
        }

        private bool RefreshSteamProcess()
        {
            lock(_eventLock)
            {
                try
                {
                    var processes = _executableLibrary.FindProcess(_steamLib.SteamProcessName);
                    if(processes.Any())
                    {
                        UnregisterProcess();
                        _steamExited = false;
                        _steamProcess = processes.First();
                        _unregisteredProcess = false;
                        _steamProcess.Exited += _steamProcess_Exited;
                        _steamProcess.EnableRaisingEvents = true;
                        return true;
                    }
                    return false;
                }
                catch(Exception e)
                {
                    Logger.Warn(e);
                    return false;
                }
            }
        }

        private ProcessStartInfo GetStartInfo(IProcessExecutionLogic executionLogic)
        {
            var args = $"-login {_username} {_password} -silent -novid -nochatui -nofriendsui -applaunch {_appId}";
            if(!String.IsNullOrEmpty(executionLogic.ExecutionParameters))
                args = args + $" {executionLogic.ExecutionParameters}";
            return new ProcessStartInfo(_exeFilePath, args);
        }

        private void UnregisterWatcher()
        {
            lock(_eventLock)
            {
                if(_watcherSteamUser == null) return;
                Logger.Debug("Unregistering Watcher!");
                _watcherSteamUser.Stop();
                _watcherSteamUser.OnRegistryEntryChanged -= WatcherSteamUser_OnRegistryEntryChanged;
                _watcherSteamUser.Dispose();
                _watcherSteamUser = null;
            }
        }

        private void UnregisterAppWatcher()
        {
            lock(_eventLock)
            {
                if(_watcherApp == null) return;
                Logger.Debug("Unregistering Watcher!");
                _watcherApp.Stop();
                _watcherApp.OnRegistryEntryChanged -= WatcherApp_OnRegistryEntryChanged;
                _watcherApp.Dispose();
                _watcherApp = null;
            }
        }

        private void UnregisterProcess()
        {
            lock(_eventLock)
            {
                if(_unregisteredProcess) return;
                Logger.Debug("Unregistering Process!");
                _steamProcess.Exited -= _steamProcess_Exited;
                _steamProcess = null;
                _unregisteredProcess = true;
            }
        }

        private void UnregisterTimer()
        {
            lock(_eventLock)
            {
                if(_steamClientTimer == null) return;
                Logger.Debug("Unregistering Timer!");
                _steamClientTimer.Stop();
                _steamClientTimer.Elapsed -= SteamClientTimerCallback;
                _steamClientTimer.Dispose();
                _steamClientTimer = null;
            }
        }
        #endregion
    }
}