#region Licence
/****************************************************************
 *  Filename: BaseApplicationExecution.cs
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
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Utilities.Windows;
using LeapVR.Utilities.Windows.Processes;
using NLog;

namespace LeapVR.Shell.Modules.Platform
{
    public abstract class BaseApplicationExecution : IApplicationExecution
    {
        private const int WaitForStartIntervalMs = 3000;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Subject<AppExecutionMessage> _whenExecutionPhaseChangeSubject = new Subject<AppExecutionMessage>();
        private readonly object _stateLock = new object();
        private readonly IAppPlatformInfo _platformAppInfo;
        private readonly ExecutableLibrary _executableLibrary;
        private volatile bool _isStarted = false;
        private volatile bool _hasError = false;
        private volatile bool _hasRun = false;
        private volatile bool _isTerminationInProcess = false;
        private volatile bool _isSystemShutDown = false;
        private volatile bool _isSystemShutDownAddedToReasons = false;
        private IAccountAccess _accountAccess = null;
        private List<TerminationReason> _terminationReasons = null;

        public IAppPlatformInfo DisplayInfo => _platformAppInfo;
        public IProcessExecutionLogic LogicToExecute { get; }
        public IObservable<AppExecutionMessage> WhenExecutionPhaseChange { get; }
        public DateTime? Started { get; private set; }
        public DateTime? Stopped { get; private set; }
        public bool IsStarted => _isStarted;
        public bool HasRun => _hasRun;
        public bool HasError => _hasError;
        public bool WasCanceled => _isTerminationInProcess;
        public bool IsLicenseRequired => _platformAppInfo.IsLicenseRequired;
        protected IPlatformStateProvider StateProvider { get; }
        private readonly IUIMessageBroker _messageBroker;

        protected BaseApplicationExecution(
            IAppPlatformInfo platformAppInfo,
            IProcessExecutionLogic logicToExecute,
            IUIMessageBroker messageBroker
            )
        {
            QuickLeap.AssertNotNull(logicToExecute, platformAppInfo, messageBroker);
            _executableLibrary = new ExecutableLibrary();
            LogicToExecute = logicToExecute;
            Started = DateTime.UtcNow;
            WhenExecutionPhaseChange = _whenExecutionPhaseChangeSubject.AsObservable();
            _messageBroker = messageBroker;
            _platformAppInfo = platformAppInfo;
            StateProvider = new PlatformStateProvider(_platformAppInfo,_messageBroker);
        }

        public void Run()
        {
            if (IsStarted || HasError || HasRun) return;
            lock (_stateLock)
            {
                if (IsStarted || HasError || HasRun) return;
                Logger.Debug("Run was called and is about to be executing");
                _isStarted = true;
                Task.Factory.StartNew(DoWork, TaskCreationOptions.LongRunning);
            }
        }
        public void Terminate(bool isSystemShutDown = false)
        {
            if (!IsStarted || WasCanceled) return;
            lock (_stateLock)
            {
                if (!IsStarted || WasCanceled) return;
                Logger.Debug("Terminate was called and is executing");
                _isSystemShutDown = isSystemShutDown;
                _isTerminationInProcess = true;
                Task.Factory.StartNew(DoTerminate, TaskCreationOptions.PreferFairness);
            }
        }

        public long ExecutionDurationTicks()
        {
            if (Started == null)
            {
                Logger.Warn("It's not yet started is null.");
                return 0;
            }
            var stoppedTime = Stopped ?? DateTime.UtcNow;
            return (stoppedTime - Started).Value.Ticks;
        }

        /// <summary>
        /// Tries to get an Account and AccountAccess for the Application to Execute
        /// </summary>
        /// <param name="account">The Account with the License</param>
        /// <param name="accountAccess">The Access Token</param>
        /// <returns>true if account was found false if no account could be allocated</returns>
        protected bool GetAccount(out IPlatformAccount account,out IAccountAccess accountAccess)
        {
            var isSuccess = _platformAppInfo.TryGetAccountAccessForApp(out account, out accountAccess);
            if(isSuccess) _accountAccess = accountAccess;
            return isSuccess;
        }

        /// <summary>
        /// Called by Base to verify if all data and requirements are present for the platform to start app
        /// </summary>
        /// <returns>true if startable, false if requirements not meet</returns>
        protected abstract bool IsStartable();

        /// <summary>
        /// Called by Base to perform Platform Start by PlatformModule
        /// Exceptions thrown inside of the Methods will mark start as failed
        /// </summary>
        protected abstract void OnPlatformStart();

        /// <summary>
        /// Called by Base to perform Platform Stop by PlatformModule
        /// Call is ensured when OnPlatformStart was perviously called
        /// </summary>
        protected abstract void OnPlatformStop();

        /// <summary>
        /// Base Class Work Routine to be executed in seperate thread as this method is blocking
        /// </summary>
        private void DoWork()
        {
            Logger.Debug("Working Routine started");
            try
            {
                _hasRun = true;
                try
                {
                    //Verifiy if the Platform App is Startable
                    if (!IsStartable())
                    {
                        try
                        {
                            Logger.Warn("Application is not Startable!");
                            _hasError = true;
                            //Inform about finished application execution
                            _whenExecutionPhaseChangeSubject.OnNext(new AppExecutionMessage(this, ExecutionPhase.OnFinished));
                            return;
                        }
                        catch (Exception exception)
                        {
                            Logger.Fatal(exception, "An Subscriber has thrown on AppExecution Message");
                            return;
                        }
                    }
                    //Set next Phase and Continue to execution Loop
                    ExecutionPhase nextPhase = ExecutionPhase.BeforeStart;

                    //Never skip Before Exit after OnPlatformStart was performed!
                    //Never skip further then After Exit as Modules might need to deinitialize themselfs or cleanup!
                    do
                    {
                        Logger.Debug($"Performing Phase = {nextPhase}");
                        switch (nextPhase)
                        {                                
                            //Publish UI Event to create Screen response for User
                            case ExecutionPhase.BeforeStart:
                                _messageBroker.Publish(new UIAppExecutionEvent(LogicToExecute.ApplicationGuid, _platformAppInfo, UIApplicationExecutionPhase.BeginnExecution));
                                nextPhase = DoNotification(ExecutionPhase.BeforeStart, ExecutionPhase.OnPlatformStart, ExecutionPhase.AfterExit);
                                break;
                            //Start Execution by Platform
                            case ExecutionPhase.OnPlatformStart:
                                bool platformStartSuccess = true;
                                try
                                {
                                    OnPlatformStart();
                                    Started = DateTime.UtcNow;
                                }
                                catch (Exception e)
                                {
                                    platformStartSuccess = false;
                                    Logger.Error(e, "OnPlatformStart has thrown an exception!");
                                }
                                //Handle Execution Result
                                if (platformStartSuccess)
                                {
                                    nextPhase = DoNotification(ExecutionPhase.OnPlatformStart,
                                        ExecutionPhase.AfterStart, ExecutionPhase.BeforeExit);
                                }
                                else
                                {
                                    nextPhase = DoNotification(ExecutionPhase.OnPlatformStart, ExecutionPhase.BeforeExit, ExecutionPhase.BeforeExit);
                                }
                                break;
                            //Start Watch Executed Application and Start Monitoring / Waiting for Exit
                            case ExecutionPhase.AfterStart:
                                nextPhase = DoNotification(ExecutionPhase.AfterStart, ExecutionPhase.BeforeExit, ExecutionPhase.BeforeExit);

                                //Monitor and Block Chain After Start and Wait until Application Exists
                                DoWatchDog();
                                break;
                            //Inform about Start of Exit Sequence
                            case ExecutionPhase.BeforeExit:
                                nextPhase = DoNotification(ExecutionPhase.BeforeExit, ExecutionPhase.OnPlatformEnd, ExecutionPhase.OnPlatformEnd);

                                //Do the Termination Job after the Before Exit State was Signalized
                                DoTerminate();
                                break;
                            case ExecutionPhase.OnPlatformEnd:
                                OnPlatformStop();
                                nextPhase = DoNotification(ExecutionPhase.OnPlatformEnd, ExecutionPhase.AfterExit, ExecutionPhase.AfterExit);
                                break;
                            case ExecutionPhase.AfterExit:
                                Stopped = DateTime.UtcNow;
                                nextPhase = DoNotification(ExecutionPhase.AfterExit, ExecutionPhase.OnFinished, ExecutionPhase.OnFinished);
                                break;
                            case ExecutionPhase.OnFinished:
                                nextPhase = DoNotification(ExecutionPhase.OnFinished, ExecutionPhase.OnDone, ExecutionPhase.OnDone);
                                _messageBroker.Publish(new UIAppExecutionEvent(LogicToExecute.ApplicationGuid, _platformAppInfo, UIApplicationExecutionPhase.EndedSuccefully));
                                break;
                        }
                    } while (nextPhase != ExecutionPhase.OnDone);
                }
                finally
                {
                    if(_accountAccess != null && !_accountAccess.IsReleased)
                    {
                        _accountAccess.Release();
                    }
                    Logger.Debug("Work Routine Ended!");
                    
                    _isStarted = false;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Was thrown out of the Work Loop!");
            }
        }

        private ExecutionPhase DoNotification(ExecutionPhase current, ExecutionPhase next, ExecutionPhase onTermination)
        {
            AppExecutionMessage message;
            bool hasTerminationReasonHistory = false;
            //Provide Termination Reasons to subscribers if there are any
            if (_terminationReasons == null) message = new AppExecutionMessage(this, current);
            else
            {
                hasTerminationReasonHistory = true;
                message = new AppExecutionMessage(this, current, _terminationReasons);
            }
            try
            {
                //If a System Shutdown was requested we need to add it to the Termination Reasons to inform every subscriber
                if (_isSystemShutDown && !_isSystemShutDownAddedToReasons)
                {
                    Logger.Debug("Recognized that a SystemShutdown was requested and not yet added as shutdown reason");
                    message.RequestTermination(TerminationReason.SystemShutdown);
                    hasTerminationReasonHistory = true;
                    _isSystemShutDownAddedToReasons = true;
                }
                //We inform everyone with the current state
                //Each informed subscriber can set an Termination Flag to request Termination
                _whenExecutionPhaseChangeSubject.OnNext(message);
            }
            catch (Exception e)
            {
                Logger.Error(e, "A subscriber has thrown and broke the Notification chain! Termination will be initialized");
                message.RequestTermination(TerminationReason.ExceptionOnNext);
            }

            //We need to evaluate if we are already in an Termination Sequence or any request was set during last Publishing to Subscribers
            //Return the onTermination Phase as nextphase if an termination was requested from outside
            //Outside can be an Module or an Call to Terminate the app through a public method
            if (_isTerminationInProcess || message.TerminationRequested)
            {
                Logger.Warn($"A Termination sequence about to exceute with _isTerminationInProcess={_isTerminationInProcess},TerminationRequested={message.TerminationRequested}, TerminationReasons={QuickLeap.EnumerableToString(message.TerminationReasons)} ! Termination will be initialized");
                //Publish a Message to the UI if a component has requested a shutdown, but only in case it was a UI Component
                if (message.TerminationRequested && !_isSystemShutDown)
                {
                    Logger.Debug($"Publishing Message to UI for ComponentTerminationRequest for ApplicationGuid={LogicToExecute.ApplicationGuid}");
                    _messageBroker.Publish(new UIAppExecutionEvent(LogicToExecute.ApplicationGuid, _platformAppInfo, UIApplicationExecutionPhase.ComponentTerminationRequested, message.TerminationReasons.ToArray()));
                }

                //If we got a NEW Reason added, we set our list to the latest state
                if (!hasTerminationReasonHistory && message.TerminationRequested) _terminationReasons = message.TerminationReasons;
                return onTermination;
            }
            return next;
        }

        /// <summary>
        /// Does watch over the processes to dectect if the application is still running
        /// </summary>
        private void DoWatchDog()
        {
            Logger.Debug($"Application WatchDog started for ApplicationGuid={LogicToExecute.ApplicationGuid}");
            var processSearchStopWatch = new Stopwatch();
            var mainProcesses = LogicToExecute.MonitorInstructions.Where(instruction => instruction.Instruction.HasFlag(ProcessMonitorOption.IsMainExecutable)).ToList();
            bool isSingleMainProcess = mainProcesses.Count == 1;
            Logger.Debug($"Application has single MainProcess={isSingleMainProcess}");
            while (!_isTerminationInProcess && processSearchStopWatch.ElapsedMilliseconds < WaitForStartIntervalMs)
            {
                processSearchStopWatch.Start();
                try
                {
                    bool foundMainProcess = false;
                    Process main = null;
                    IProcessMonitorInstruction mainProcessInstruction = null;
                    while (!foundMainProcess && !_isTerminationInProcess && processSearchStopWatch.ElapsedMilliseconds < WaitForStartIntervalMs)
                    {
                        List<Process> mainProcess = new List<Process>();
                        foreach (var process in mainProcesses)
                        {
                            mainProcessInstruction = process;
                            mainProcess = _executableLibrary.FindProcess(
                                _executableLibrary.GetProcessNameFromExePath(process.ExecutableRelativePathFileName));
                            if (mainProcesses.Any()) break;
                        }
                        if (!mainProcess.Any()) continue;
                        main = mainProcess.First();
                        foundMainProcess = true;
                        processSearchStopWatch.Reset();
                    }
                    //Continue and run onto the timeout condition
                    if (!foundMainProcess)
                    {
                        Logger.Debug($"No MainProcess found for {mainProcessInstruction?.ExecutableRelativePathFileName}, continue");
                        continue;
                    }


                    //Check and Wait until Process will exit
                    WaitForProcessEnd(main, mainProcessInstruction.Instruction.HasFlag(ProcessMonitorOption.KillProcessOnNotResponding));

                    //If it is a single Mainprocess, we do not need to check for other spawned mainprocesses if we once found one and he exited
                    if (isSingleMainProcess) break;
                    processSearchStopWatch.Stop();
                    processSearchStopWatch.Reset();
                }
                catch
                { }
            }
            Logger.Debug($"Watchdog finished work for ApplicationGuid={LogicToExecute.ApplicationGuid}");
        }


        private void WaitForProcessEnd(Process process, bool checkAndKillNotResponding = false)
        {
            try
            {
                Logger.Debug("Waiting for Process to end started");
                if (process == null)
                {
                    Logger.Warn("Process is null, but should not be");
                    return;
                }
                if (checkAndKillNotResponding)
                {
                    var notRespondingProcess = new NotRespondingProcess(process,_executableLibrary.KillProcess);
                    notRespondingProcess.WaitForExit();
                }
                else
                {
                    Logger.Debug($"Start Waiting for Process with Name ={process.ProcessName}");
                    process.WaitForExit();
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Kills Application specified by MonitorInstructions
        /// </summary>
        private void DoTerminate()
        {
            foreach (var instruction in LogicToExecute.MonitorInstructions)
            {
                if (instruction.Instruction.HasFlag(ProcessMonitorOption.KillOnExit)) _executableLibrary.KillProcess(_executableLibrary.GetProcessNameFromExePath(instruction.ExecutableRelativePathFileName));
            }
        }

        public override string ToString()
        {
            if (LogicToExecute != null) return $" ApplicationID={LogicToExecute.ApplicationGuid}, ApplicationName={_platformAppInfo.Name} Logic to Execute={LogicToExecute.DisplayName}";
            return base.ToString();
        }

    }
}
