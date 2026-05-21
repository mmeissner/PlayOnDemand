#region Licence
/****************************************************************
 *  Filename: ApplicationExecutionBase.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-7-14
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
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared;
using LeapVR.Shared.NetStandard;
using LeapVR.Shared.NetStandard.Extensions;
using LeapVR.VBox.DataModel.Interfaces.App;
using LeapVR.VBox.DataModel.Interfaces.Execution;
using LeapVR.VBox.Modules.Interfaces.Execution;
using NLog;

// ReSharper disable ExplicitCallerInfoArgument

namespace LeapVR.VBox.Modules.Platform
{
    public abstract class ApplicationExecutionBase : IApplicationExecution
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region Properties & Fields
        public IAppPlatformData AppPlatformData { get; }
        public DateTime? Started { get; private set; }
        public DateTime? Stopped { get; private set; }

        private readonly ReplaySubject<ExecutionPhase> _whenExecutionPhaseChangeSubject;
        public IObservable<ExecutionPhase> WhenExecutionPhaseChange { get; }

        private readonly object _executionPhaseLock = new object();
        private readonly List<ExecutionPhase> _executionPhaseHistory = new List<ExecutionPhase>();

        public IEnumerable<IOptionalBehavior> BehaviorsToExecute { get; }

        #endregion Properties & Fields

        #region Constructors

        protected ApplicationExecutionBase(IAppPlatformData appPlatformData)
        {
            QuickLeap.AssertNotNull(appPlatformData);

            AppPlatformData = appPlatformData;

            BehaviorsToExecute = new List<IOptionalBehavior>(); // TODO [RM]: Empty for now; To be assigned somewhere.
            _whenExecutionPhaseChangeSubject = new ReplaySubject<ExecutionPhase>();
            WhenExecutionPhaseChange = _whenExecutionPhaseChangeSubject.AsObservable();

            Started = DateTime.UtcNow;
        }

        #endregion Constructors

        #region Methods

        //protected (bool, IProcessExecution) StartExecution(IProcessExecutionLogic processExecutionLogic)
        //{
        //    // should only be called in lock(_processLock) context

        //    this.LogInfo("Execution logic started.", QuickLeap.GetCallerName(), () => processExecutionLogic.ExecutionFile.ApplicationGuid);

        //    var fileInfo = VBoxPlatformModule.DiskController.GetAppFile(processExecutionLogic.ExecutionFile);
        //    var workingDirectory = Path.GetDirectoryName(fileInfo.AbsolutePath);

        //    this.LogInfo("Process is about to be started.", QuickLeap.GetCallerName(), () => fileInfo.AbsolutePath);

        //    var startProcessExecution = VBoxPlatformModule.ProcessExecutionManager.CreateProcessExecution(fileInfo.AbsolutePath, processExecutionLogic.ExecutionParameters, workingDirectory);

        //    OnExecutionPhaseChanging(ExecutionPhase.BeforeStart);

        //    startProcessExecution.Start();


        //    // TODO [RM]: improve this waiting code; now temp only

        //    var loopCountLeft = _waitForStartMaxLoopCount;
        //    var mainprocessName = processExecutionLogic.MainProcessName;
        //    var processes = VBoxPlatformModule.ProcessExecutionManager.AttachToProcesses(mainprocessName).ToArray();

        //    while (!processes.Any())
        //    {
        //        loopCountLeft--;

        //        // ReSharper disable once AccessToModifiedClosure
        //        this.LogDebug("In loop waiting for process to start...", QuickLeap.GetCallerName(), () => loopCountLeft);

        //        Thread.Sleep(_waitForStartLoopintervalMs);

        //        if (loopCountLeft <= 0)
        //        {
        //            this.LogWarn("Loop counter limit exceeded; Canceling and terminating.");

        //            OnExecutionPhaseChanging(ExecutionPhase.BeforeExit);
        //            startProcessExecution.Kill(); // TODO [RM]: Kill(killChildProcesses: true) ?
        //            OnExecutionPhaseChanging(ExecutionPhase.AfterExit); // TODo [RM]: ExecutionPhase.Errored?
        //            OnExecutionPhaseChanging(ExecutionPhase.Ended);

        //            return (false, null);
        //        }

        //        processes = VBoxPlatformModule.ProcessExecutionManager.AttachToProcesses(mainprocessName).ToArray();
        //    }

        //    this.LogDebug("Application properly started.");

        //    OnExecutionPhaseChanging(ExecutionPhase.AfterStart);

        //    // TODO [RM]: how to handle multiple processes with same name?
        //    var targetProcess = processes.Single();

        //    return (true, targetProcess);

        //}
        public abstract void Execute();
        public virtual async Task ExecuteAsync()
        {
            await Task.Run(() => Execute());
        }
        public abstract void TerminateExecution();
        public virtual async Task TerminateExecutionAsync()
        {
            await Task.Run(() => TerminateExecution());
        }

        protected void KillProcessByIdLogic(ProcessIdentifier processIdentifier)
        {
            var process = Process.GetProcessById(processIdentifier.Id);
            if (process.StartTime == processIdentifier.StartTime)
            {
                KillProcessOrIgnore(process);
            }

        }
        protected void KillProcessByNameLogic(string processName)
        {
            Process.GetProcessesByName(processName).ToList().ForEach(KillProcessOrIgnore);

        }
        protected void KillProcessOrIgnore(Process process)
        {
            try
            {
                process?.Kill();
            }
            catch (Exception exception)
            {
                Logger.Log(LogLevel.Warn, exception,$"failed to kill process for application {AppPlatformData.ApplicationGuid} and will ignore this issue.");
            }
        }
        private void CleanupBehaviors()
        {
            Exception lastException = null;
            foreach (var behavior in BehaviorsToExecute)
            {
                try
                {
                    behavior.Cleanup();
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }

            if (lastException != null)
            {
                throw new InvalidOperationException("Fatal error in process of cleanup of OptionalBehaviors.", lastException);
            }
        }
        public void OnExecutionPhaseChanging(ExecutionPhase newPhase)
        {
            // TODO [RM]: call (by PlatformModule) before change phase, wait till finishes, then continue
            // TODO [RM]: throw if failure? Or return some status enum?

            Logger.Log(LogLevel.Debug,$"Execution phase changed. newPhase={newPhase}");

            lock (_executionPhaseLock)
            {
                var lastPhase = _executionPhaseHistory.LastOrDefault();
                if (lastPhase != default(ExecutionPhase) && (int)newPhase <= (int)lastPhase)
                {
                    throw new InvalidOperationException("Detected ExecutionPhase order inconsistency.");
                }
                _executionPhaseHistory.Add(newPhase);

                if (Started == null)
                {
                    Started = DateTime.UtcNow; // TODO [RM]: decide if assign like this
                }

                if (newPhase == ExecutionPhase.AfterExit)
                {
                    Stopped = DateTime.UtcNow;
                }

                try
                {
                    foreach (var behavior in BehaviorsToExecute)
                    {
                        behavior.OnPhaseChanged(newPhase);
                    }
                }
                catch
                {
                    CleanupBehaviors();
                    // TODO [RM]: terminate execution? return some value indicating error?
                    throw;
                }

                _whenExecutionPhaseChangeSubject.OnNext(newPhase);
                if (newPhase == ExecutionPhase.Ended)
                {
                    _whenExecutionPhaseChangeSubject.OnCompleted();
                }
            }
        }

        #endregion Methods
    }
}
