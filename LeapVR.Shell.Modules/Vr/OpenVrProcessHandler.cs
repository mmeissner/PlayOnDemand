#region Licence
/****************************************************************
 *  Filename: OpenVrProcessHandler.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Repositories;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Utilities.Steam.Steam;
using LeapVR.Utilities.Windows;
using LeapVR.Utilities.Windows.FileProcessor;
using Newtonsoft.Json;
using NLog;
using Polly;

namespace LeapVR.Shell.Modules.Vr
{
    internal class OpenVrProcessHandler : IDisposable
    {
        #region Private Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string LogFileReadyTransitionTrigger = "VR_Init successful";
        private const string LogFileFailedTransitionTrigger = "VR_Init failed";
        private const int MaxMoveWindowAttempts = 5;
        private const int MoveWindowAttemptsWaitMs = 400;
        private const int MaxRestartAttempts = 3;
        private const int MaxDisableWindowAttempts = 3;
        private const int DisableWindowAttempFailedWaitTimeMs = 200;
        private const int DetectOpenVrStartMaxLoopCount = 10;
        private const int DetectOpenVrStartRetryDelayMs = 500;
        private readonly TimeSpan _minWaitBetweenKillAttempts = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan _createProcessTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _steamVrReadyTimeout = TimeSpan.FromSeconds(10);

        private readonly OpenVRFilesHandler _openVRFilesHandler;
        private readonly OpenVrModuleConfig _openVrModuleConfig;
        private readonly object _openVrLock = new object();

        private readonly string _logFilePath;
        private Process _vrMonitorProcess = null;
        private HmdActivityWatchdog _activityWatchdog;
        private volatile bool _isOpenVrRunning;
        private volatile bool _isWatchDogCreated;
        private volatile bool _hasError;
        private IntPtr _eventHook = IntPtr.Zero;
        private readonly TransparencyAreaCallBack _transparencyAreaCallback;
        private readonly Action _restartVrGui;
        private readonly WinEventDelegate _mWinEventDelegate;
        #endregion

        #region Public Properties
        public bool IsRunning
        {
            get
            {
                lock(_openVrLock)
                {
                    return !_hasError && _isOpenVrRunning;
                }
            }
        }
        public bool HasError
        {
            get
            {
                lock(_openVrLock)
                {
                    return _hasError;
                }
            }
        }
        public bool DisableInteraction { get; }
        public Size OpenVrWindowSize { get; private set; }
        #endregion

        public OpenVrProcessHandler(
                IConfigFileRepository<OpenVrModuleConfig> openVrModuleConfigRepository,
                IOpenVrSettingsSetRepository openVrSettingsSetRepository,
                SteamLib steamLib,
                TransparencyAreaCallBack transparencyAreaCallback,
                bool disableInteraction,
                Action restartVrGui)
        {
            _openVrModuleConfig = openVrModuleConfigRepository.Get();
            _openVRFilesHandler = new OpenVRFilesHandler(
                    openVrModuleConfigRepository,
                    openVrSettingsSetRepository,
                    steamLib);
            _logFilePath = GetLogFilePath(
                    _openVrModuleConfig.VrMonitorConfigFilePath,
                    _openVrModuleConfig.VrMonitorLogFileName);

            _mWinEventDelegate = WhenWindowChanges;
            _transparencyAreaCallback = transparencyAreaCallback;
            _restartVrGui = restartVrGui;
            DisableInteraction = disableInteraction;
        }

        public bool StartVrDriver()
        {
            try
            {
                Logger.Debug("Start OpenVR requested.");
                if(_isOpenVrRunning)
                {
                    Logger.Debug("Start OpenVR requested, but is already running");
                    return true;
                }
                if(_hasError)
                {
                    Logger.Warn("Start OpenVR requested, but it has already an error, please create a new ProcessHandler");
                    return false;
                }

                var retval = StartProcess();
                Logger.Debug($"StartProcess returned with Result={retval}");
                return retval;
            }
            catch(Exception e)
            {
                Logger.Error(e, "Error occured during Start of VR Driver");
                _hasError = true;
                return false;
            }
            finally
            {
                if(_hasError)
                {
                    Logger.Warn("Start OpenVR ended with Error");
                }
                else Logger.Debug("Start OpenVR ended");
            }
        }

        private bool StartProcess()
        {
            lock(_openVrLock)
            {
                if(_isOpenVrRunning)
                {
                    Logger.Debug("Start OpenVR requested, but is already running");
                    return true;
                }

                if(_hasError)
                {
                    Logger.Warn("Start OpenVR requested, but it has already an error, please create a new ProcessHandler");
                    return false;
                }

                //Ensure it's stopped as we switch config files
                if(!StopVrMonitorProcess())
                {
                    Logger.Error("OpenVR Stop before start finished with error!");
                    {
                        return false;
                    }
                }

                //Switch Files
                _openVRFilesHandler.ReplaceOpenVrConfig();

                //Internal Start
                if(GetOrCreateVrMonitorProcess(out var vrMonitorProcess))
                {
                    
                    //Process Started
                    _isOpenVrRunning = true;
                    _vrMonitorProcess = vrMonitorProcess;
                    _vrMonitorProcess.Exited += _vrMonitorProcess_Exited;
                    _vrMonitorProcess.EnableRaisingEvents = true;


                    //No Window Management needed
                    if(_transparencyAreaCallback == null)
                    {
                        return false;
                    }

                    //Prepare Window Management
                    Logger.Debug("Registered WindowsEventHook for OpenVR!");

                    //Scan for Windows and identify them
                    ScanVrMonitorWindows(_vrMonitorProcess);

                    //Position the Window for Initialization
                    if(_transparencyAreaCallback == null)
                    {
                        Logger.Info("No TransparencyArea Callback was provided, Ui Interaction is Off");
                        {
                            return false;
                        }
                    }


                    var uiStartUpProcedure = Application.Current.Dispatcher.CheckAccess() ? UserInterfaceProcedure() :
                                Application.Current.Dispatcher.Invoke(UserInterfaceProcedure);
                    return uiStartUpProcedure;
                }

                //Error
                Logger.Error("Could not Get or Create VRMonitor Process");
                Unsubscribe();
                StopVrMonitorProcess(_vrMonitorProcess);
                _openVRFilesHandler.RestoreOpenVrConfig();
                _hasError = true;
                return false;
            }
        }

        public void StopVrDriver()
        {
            try
            {
                Logger.Debug("Stop OpenVR requested.");
                if(!_isOpenVrRunning || _hasError)
                {
                    Logger.Debug("Stop OpenVR requested, but is already running or start/stop request in process.");
                    return;
                }

                lock(_openVrLock)
                {
                    if(!_isOpenVrRunning || _hasError) return;
                    //Internal Stop
                    if(Unsubscribe())
                    {
                        Logger.Debug("Event Hook for OpenVr unsubscribed successfully");
                    }
                    else
                    {
                        Logger.Warn("Event Hook for Open VR could unsubscribed unsuccessfully");
                    }

                    _vrMonitorProcess.EnableRaisingEvents = false;
                    _vrMonitorProcess.Exited -= _vrMonitorProcess_Exited;


                    _activityWatchdog?.Dispose();
                    _activityWatchdog = null;
                    StopVrMonitorProcess(_vrMonitorProcess);
                    _openVRFilesHandler.RestoreOpenVrConfig();
                }

                Logger.Debug("Stop OpenVR ended.");
            }
            catch(Exception e)
            {
                Logger.Error(e, "Error occured during Stop of VR Driver");
                _hasError = true;
            }
        }
        public bool GetWatchdog(out IHmdActivityWatchdog watchdog)
        {
            watchdog = null;
            if(!_isOpenVrRunning || _hasError) return false;
            lock(_openVrLock)
            {
                if(!_isOpenVrRunning || _hasError)
                {
                    return false;
                }

                if(_isWatchDogCreated)
                {
                    watchdog = _activityWatchdog;
                    return true;
                }

                _activityWatchdog = new HmdActivityWatchdog();
                _isWatchDogCreated = true;
                watchdog = _activityWatchdog;
                return true;
            }
        }

        #region CallBack
        private readonly HashSet<IntPtr> _windowHandleToIgnore = new HashSet<IntPtr> {IntPtr.Zero};
        private readonly HashSet<IntPtr> _windowHandleToHide = new HashSet<IntPtr>();
        private readonly HashSet<string> _classNamesToHide = new HashSet<string>()
                                                             {
                                                                     "Qt5QWindowPopupDropShadowSaveBits",
                                                                     "Qt5QWindowToolSaveBits",
                                                                     "SysShadow"
                                                             };
        private const int MinHeightMainWindow = 264;
        private const string ClassNameMainWindow = "Qt5QWindowIcon";
        private const string WindowTextMainWindow = "SteamVR";

        private IntPtr _windowHandleToPosition = IntPtr.Zero;

        private void WhenWindowChanges(
                IntPtr hWinEventHook,
                uint eventType,
                IntPtr hwnd,
                int idObject,
                int idChild,
                uint dwEventThread,
                uint dwmsEventTime)
        {
            if(_windowHandleToIgnore.Contains(hwnd)) return;
            if(_windowHandleToHide.Contains(hwnd))
            {
                User32.ShowWindow(hwnd, User32.WindowState.Hide);
                return;
            }

            if(_windowHandleToPosition.Equals(hwnd))
            {
                if(User32.GetWindowRect(hwnd, out var rectangle))
                {
                    var pos = _transparencyAreaCallback?.Invoke(rectangle.Width, rectangle.Height)?.CalcScreenPos();
                    if(pos == null) return;
                    if(rectangle.Left.Equals(pos.Value.Left) &&
                       rectangle.Top.Equals(pos.Value.Top)) return;
                    MoveOpenVrWindow(hwnd, pos.Value.X, pos.Value.Y);
                }
                else
                {
                    Logger.Warn("Could not Receive Window Rectangle for Window to Position");
                }

                return;
            }

            AddToFilter(hwnd);
            WhenWindowChanges(hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwmsEventTime);
        }
        #endregion

        #region Private Methods

        private void _vrMonitorProcess_Exited(object sender, EventArgs e)
        {
            lock(_openVrLock)
            {                
                Logger.Warn($"VR Monitor Process was closed from outside: VrMonitorProcess.HasExited={_vrMonitorProcess.HasExited}, IsOpenVrRunning={_isOpenVrRunning}, HasError = {_hasError}");
                if(_vrMonitorProcess.HasExited && _isOpenVrRunning && !_hasError)
                {
                    Logger.Info("Trying to Restart VrMonitorProcess");
                    Unsubscribe();
                    StopVrMonitorProcess(_vrMonitorProcess);
                    if(StartProcess())
                    {
                        _restartVrGui?.Invoke();
                    }
                    else
                    {
                        Logger.Error("SteamVR Exit Detected but could not Restart Process, Setting Error");
                        _hasError = true;
                    }
                }
                else
                {
                    Logger.Info("Did not restarted VrMonitor Process as the prerequirements werent fullfilled");
                }
            }
        }

        private bool UserInterfaceProcedure()
        {
            if(GetWindowLocation(_vrMonitorProcess.MainWindowHandle, out var openVrMonitorWindow))
            {
                Logger.Debug("Received Window Location for OpenVR");
                OpenVrWindowSize = openVrMonitorWindow.ToWindowsSize();
                var transparencyArea = _transparencyAreaCallback.Invoke(
                        OpenVrWindowSize.Width,
                        OpenVrWindowSize.Height);

                if(transparencyArea != null)
                {
                    var openVrScreenPos = transparencyArea.CalcScreenPos();
                    Logger.Debug("Calculated Window Location for OpenVR");

                    //Do this only when Interaction is Disabled
                    //When Interaction is Enabled and someone minimize the Window
                    //the whole Application will freeze User Input!
                    if(DisableInteraction)
                    {
                        //Do Subscription for WindowsEvents
                        if(!SubscribeEvents((uint)_vrMonitorProcess.Id, out _eventHook))
                        {
                            Logger.Error("Subscription for Window Events failed");
                        }
                        else
                        {
                            Logger.Info("Subscribtion for  Window Events succeded");
                        }
                    }
     

                    if(_windowHandleToPosition != IntPtr.Zero && !openVrScreenPos.IsEmpty)
                    {
                        //Initial Positioning
                        Logger.Info("Positioning of Window started");
                        int attempts = 0;
                        do
                        {
                            Thread.Sleep(MoveWindowAttemptsWaitMs);
                            attempts++;
                            MoveOpenVrWindow(
                                    _windowHandleToPosition,
                                    openVrScreenPos.X,
                                    openVrScreenPos.Y);
                        } while(!GetWindowLocation(_windowHandleToPosition,out var position) && !User32.Rectangle.ToRectangleDrawing(openVrScreenPos).Equals(position) && attempts < MaxMoveWindowAttempts);
                    }
                    else
                    {
                        Logger.Warn("No Window Handle to Position found on StartVR Driver");
                    }

                    Logger.Info("Open VR Start successfull!");
                    return true;
                }
                Logger.Error("Could not Calculate OpenVRWindow Position!");
            }
            else
            {
                Logger.Error("Could not Get or Create VRMonitor Process");
            }

            return false;
        }

        private void AddToFilter(IntPtr hwnd)
        {
            //GetWindow ClassName
            var sbClassName = new StringBuilder(64);
            User32.GetClassName(hwnd, sbClassName, sbClassName.Capacity);
            var className = sbClassName.ToString();

            //GetWindow Text
            var sbWindowText = new StringBuilder(64);
            User32.GetWindowText(hwnd, sbWindowText, sbWindowText.Capacity);
            var windowText = sbWindowText.ToString();

            //GetWindow Location
            //Is it Window to Hide
            var hasLocation = GetWindowLocation(hwnd, out var location);
            if(_classNamesToHide.Contains(className))
            {
                _windowHandleToHide.Add(hwnd);
                return;
            }

            //Is it Window to Position
            if(className.Equals(ClassNameMainWindow) &&
               hasLocation &&
               location.Width >= MinHeightMainWindow &&
               windowText.Contains(WindowTextMainWindow))
            {
                _windowHandleToPosition = hwnd;
                return;
            }

            //Ignore Window
            _windowHandleToIgnore.Add(hwnd);
        }

        private Process StartVrMonitorProcess(string command)
        {
            try
            {
                Process vrMonitorProcess = null;
                bool startIsSuccess = false;
                int attempts = 0;
                do
                {
                    DeleteLogFile();
                    Process.Start(command);
                    attempts++;
                    if(WaitForFullStart())startIsSuccess = true;
                    vrMonitorProcess= GetVrProcess(_openVrModuleConfig.VrMonitorProcessName);


                    if(startIsSuccess)
                    {
                        if(DisableInteraction)
                        {
                            int disableWindowAttempt = 1;
                            while(!User32.EnableWindow(vrMonitorProcess.MainWindowHandle, false) || disableWindowAttempt >= MaxDisableWindowAttempts)
                            {
                                disableWindowAttempt++;
                                Thread.Sleep(DisableWindowAttempFailedWaitTimeMs);
                            }
                        }
                        continue;
                    }
                    if(vrMonitorProcess != null) StopVrMonitorProcess(vrMonitorProcess);
                } while(!startIsSuccess && attempts < MaxRestartAttempts);

                return vrMonitorProcess;
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Exception during ExecuteShellCommand");
                throw;
            }
        }
        private bool StopVrMonitorProcess() { return StopVrMonitorProcess(GetVrProcess(_openVrModuleConfig.VrMonitorProcessName)); }

        private bool StopVrMonitorProcess(Process vrMonitorProcess)
        {
            try
            {
                //Check if in between monitoring a change happend and it was started
                bool checkForHangingSubprocesses = false;
                bool sendQuitCommand = false;
                if(!_isOpenVrRunning)
                {
                    checkForHangingSubprocesses = true;
                }

                if(vrMonitorProcess != null && !vrMonitorProcess.HasExited)
                {
                    Logger.Debug("OpenVR Monitor seems to run");
                    sendQuitCommand = true;
                }

                if(sendQuitCommand)
                {
                    Logger.Debug($"OpenVR Quit Command Send = {_openVrModuleConfig.VrMonitorStopCommand}");
                    Process.Start(_openVrModuleConfig.VrMonitorStopCommand);
                }

                if(_isOpenVrRunning || checkForHangingSubprocesses)
                {
                    var processNamesToExit = new List<string>(_openVrModuleConfig.VrMonitorProcessNamesToExit);
                    var exeLib = new ExecutableLibrary();
                    var processesRunning = exeLib.FindProcesses(processNamesToExit);
                    if(!processesRunning.Any())
                    {
                        Logger.Debug("No Steam VR Process to quit");
                        return true;
                    }

                    //Check if it is still running
                    int rounds = 0;
                    while((processesRunning = exeLib.FindProcesses(processNamesToExit)).Any())
                    {
                        foreach(Process process in processesRunning)
                        {
                            Logger.Info(
                                    $"Found Steam VR Process with: Id {process.Id}, Name: {process.ProcessName} , StartTime: {process.StartTime}");
                        }

                        //If after 15 rounds steam VR still didn't closed
                        //we will force it
                        if(rounds == 15)
                        {
                            Logger.Warn($"Killing steam VR Processes after {rounds} rounds Steam VR");
                            foreach(Process process in processesRunning)
                            {
                                exeLib.KillProcess(process);
                            }

                            return true;
                        }

                        //Wait a second and then check again
                        Thread.Sleep(_minWaitBetweenKillAttempts);
                        Logger.Debug("SteamVR seemed to shutdown by shutdown command!");
                        rounds++;
                    }
                }

                _isOpenVrRunning = false;
                return true;
            }
            catch(Exception e)
            {
                Logger.Error(e, "Error occured during StopVrMonitor");
                _hasError = true;
                return false;
            }
        }

        private Process GetVrProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            var vrMonitorProcessCount = processes.Length;
            if(vrMonitorProcessCount > 1)
            {
                throw new NotImplementedException(
                        $"Detected multiple processes with Name = {processName}, this case is not supported");
            }

            if(vrMonitorProcessCount == 0) return null;
            return processes.First();
        }

        private void ScanVrMonitorWindows(Process vrMonitorProcess)
        {
            bool CheckEnumThreadWindow(IntPtr hwnd, IntPtr lParam)
            {
                AddToFilter(hwnd);
                return true;
            }

            foreach(ProcessThread thread in vrMonitorProcess.Threads)
            {
                User32.EnumThreadWindows((uint)thread.Id, CheckEnumThreadWindow, IntPtr.Zero);
            }
        }

        private bool GetOrCreateVrMonitorProcess(out Process vrMonitorProcess)
        {
            vrMonitorProcess = GetVrProcess(_openVrModuleConfig.VrMonitorProcessName);
            var timeoutSw = Stopwatch.StartNew();
            while(vrMonitorProcess == null)
            {
                if(timeoutSw.Elapsed > _createProcessTimeout)
                {
                    Logger.Error($"Timeout during start of vrMonitor Process, timeout = {_createProcessTimeout}");
                    return false;
                }
                vrMonitorProcess = StartVrMonitorProcess(_openVrModuleConfig.VrMonitorStartCommand);
            }
            return true;
        }

        private string GetLogFilePath(string vrMonitorConfigFilePath, string vrMonitorLogFileName)
        {
            Logger.Info(
                    $"Resolving VrMonitor LogFilePath: ConfigFilePath = {vrMonitorConfigFilePath}, LogFileName={vrMonitorLogFileName}");
            var resolvedConfigFilePath =
                    Environment.ExpandEnvironmentVariables(vrMonitorConfigFilePath);
            var steamConfigFile =
                    JsonConvert.DeserializeObject<SteamVrConfigFile>(File.ReadAllText(resolvedConfigFilePath));
            var logFilePath = Path.Combine(steamConfigFile.log.First(), vrMonitorLogFileName);
            Logger.Info($"Resolved LogFilePath={logFilePath}");
            return logFilePath;
        }

        private void DeleteLogFile()
        {
            try
            {
                Logger.Info("Deleting VrMonitor Log file");
                if(File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
                else
                {
                    Logger.Debug($"Logfile does not exist at LogfilePath = {_logFilePath}");
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, $"Error during delete of VrMonitor Log file at Path ={_logFilePath}");
            }
        }

        private bool WaitForFullStart()
        {      
            try
            {
                ManualResetEvent signalVrDriverStartedEvent = new ManualResetEvent(false);
                Logger.Info("Waiting for full START of OpenVR...");
                bool initializationSuccess = false;
                var monitor = new LogFileMonitor(_logFilePath, "\r\n");
                monitor.OnNewline += (s, e) =>
                                     {
                                         // will be a different thread...
                                         var line = e.Line;
                                         if(line.Contains(LogFileReadyTransitionTrigger))
                                         {
                                             Logger.Info("Transition recognized from Not Ready to Ready, Success Trigger detected");
                                             initializationSuccess = true;
                                             signalVrDriverStartedEvent.Set();
                                         }

                                         if(line.Contains(LogFileFailedTransitionTrigger))
                                         {
                                             Logger.Info( "SteamVR Failed to Initialized to Ready, Fail Trigger detected");
                                             initializationSuccess = false;
                                         }
                                     };
                var policy = Policy.HandleResult(false).
                                    WaitAndRetry(DetectOpenVrStartMaxLoopCount, (index) => TimeSpan.FromMilliseconds(DetectOpenVrStartRetryDelayMs));

                if(policy.Execute(() => File.Exists(_logFilePath)))
                {
                    Logger.Info("Logfile Monitor Started");
                    monitor.Start();
                    signalVrDriverStartedEvent.WaitOne(_steamVrReadyTimeout);
                    signalVrDriverStartedEvent.Reset();
                    monitor.Stop();
                }

                if(initializationSuccess) Logger.Info("OpenVR Initialization detected");
                else Logger.Warn("OpenVR Initialization could not be detected");
                return initializationSuccess;
            }
            catch(Exception e)
            {
                Logger.Error(e, "Exception Occured");
                return false;
            }
        }

        #region Win32 Calls and Window Operations
        private bool GetWindowLocation(IntPtr whnd, out User32.Rectangle rectangle)
        {
            if(User32.GetWindowRect(whnd, out rectangle)) return true;
            var errorCode = Marshal.GetLastWin32Error();
            if(errorCode == 0) return true;
            Logger.Error($"GetWindowLocation received Win32 ErrorCode= {errorCode}");
            return false;
        }

        private void MoveOpenVrWindow(IntPtr hwnd, int moveWindowPosX, int moveWindowPosY)
        {
            Logger.Debug("Try to SetWindow Position and TopMost");
            User32.SetWindowPos(
                    hwnd,
                    User32.HWND.TopMost, 
                    moveWindowPosX,
                    moveWindowPosY,
                    0,
                    0,
                    User32.SetWindowPosFlags.SWP_NOACTIVATE | User32.SetWindowPosFlags.SWP_NOSIZE);
        }

        private bool SubscribeEvents(uint processId, out IntPtr hookIntPtr)
        {
            hookIntPtr = User32.SetWinEventHook(
                    User32.WinEventHook.EVENT_OBJECT_LOCATIONCHANGE,
                    User32.WinEventHook.EVENT_OBJECT_LOCATIONCHANGE,
                    IntPtr.Zero,
                    _mWinEventDelegate,
                    processId,
                    0,
                    0);
            if(hookIntPtr.Equals(IntPtr.Zero))
            {
                Logger.Warn("Event Hook for Open VR could not subscribe");
                return false;
            }

            Logger.Info("Event Hook for Open VR subscribed succesfully");
            return true;

        }

        private bool Unsubscribe()
        {
            if(_eventHook != IntPtr.Zero)
            {
                var retval = User32.UnhookWinEvent(_eventHook);
                if(!retval)
                {
                    Logger.Warn("Event Hook for Open VR could not unsubscribe");
                }
                else
                {
                    Logger.Info("Event Hook for Open VR unsubscribed succesfully");
                    _eventHook = IntPtr.Zero;
                }
            }
            return true;
        }
        private bool IsWindowAbove(IntPtr a, IntPtr b)
        {
            try
            {
                while (a != IntPtr.Zero)
                {
                    a = User32.GetWindow(a, User32.Gw.Hwndnext);
                    if (a == b)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch(Exception e)
            {
                Logger.Debug(e, "IsWindowAbove encountered Exception");
                throw;
            }
        }
        #endregion
        #endregion

        private void ReleaseUnmanagedResources() { Unsubscribe(); }
        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if(disposing)
            {
                _vrMonitorProcess?.Dispose();
                _activityWatchdog?.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~OpenVrProcessHandler() { Dispose(false); }
    }
}