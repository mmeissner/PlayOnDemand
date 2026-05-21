#region Licence
/****************************************************************
 *  Filename: VrDesktopModule.cs
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
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Execution;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Utilities.Windows;
using NLog;

namespace LeapVR.Shell.Modules.Vr
{
    public class VrDesktopModule : IVrDesktopModule
    {
        #region Properties & Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly object _vrDesktopLock = new object();
        private readonly ExecutableLibrary _executableLibrary;
        private readonly VrDesktopModuleConfig _vrDesktopModuleConfig;
        private Process _vrGuiProcessExecution;

        public bool ShouldBeRunning { get; private set; }
        #endregion Properties & Fields


        #region Constructors
        public VrDesktopModule(IConfigFileRepository<VrDesktopModuleConfig> vrDesktopModuleConfigRepository)
        {
            QuickLeap.AssertNotNull(vrDesktopModuleConfigRepository);
            _vrDesktopModuleConfig = vrDesktopModuleConfigRepository.Get();
            _executableLibrary = new ExecutableLibrary();
        }
        #endregion Constructors


        #region Methods        
        /// <summary>
        /// Changes <see cref="P:LeapVR.Shell.Modules.Interfaces.Vr.IVrDesktopModule.ShouldBeRunning" />.
        /// </summary>
        /// <param name="newValue">New value</param>
        public void ChangeShouldBeRunning(bool newValue)
        {
            lock (_vrDesktopLock)
            {
                ShouldBeRunning = newValue;
                EnsureState(newValue,ref _vrGuiProcessExecution,_vrDesktopModuleConfig.VrDesktopProcessName);
            }
        }

        /// <summary>
        /// Kills any known or unknown running VR Desktop process, and restarts it
        /// Sets ShouldBeRunning to true
        /// </summary>
        public void RestartVrDesktopModule()
        {
            lock(_vrDesktopLock)
            {
                //Ensure no process is running
                EnsureState(false,ref _vrGuiProcessExecution,_vrDesktopModuleConfig.VrDesktopProcessName);
                //Ensure new process
                ShouldBeRunning = true;
                EnsureState(true,ref _vrGuiProcessExecution,_vrDesktopModuleConfig.VrDesktopProcessName);
            }
        }

        /// <summary>
        /// Evaluates the required state and the current state of the VR Desktop Process
        /// and starts or stops the process based on the current conditions
        /// </summary>
        private void EnsureState(bool shouldBeRunning,ref Process currentWatchedProcess, string processName)
        {
            lock (_vrDesktopLock)
            {
                if(shouldBeRunning)
                {
                    // Check if we have a current process and its running
                    if(currentWatchedProcess != null)
                    {
                        //We have a process but its not running anymore
                        if(currentWatchedProcess.HasExited)
                        {
                            //We unsubscribe from ours
                            currentWatchedProcess.Exited -= VRDesktopProcessOnExited;
                            currentWatchedProcess.Dispose();
                            currentWatchedProcess = null;

                            //Handle situation of not knowning any running process but one needs to run
                            HandleNoKnownRunningProcess(ref currentWatchedProcess,processName);
                            return;
                        }
                        //We have a process and its running, nice nothing to do
                        return;
                    }
                    //We have no process and need to handle situation of not knowning any running process but one needs to run
                    HandleNoKnownRunningProcess(ref currentWatchedProcess,processName);
                    return;
                }
                //Should not run
                //We have a process
                if(currentWatchedProcess != null)
                {
                    //Unregister the process to prevent notification
                    currentWatchedProcess.Exited -= VRDesktopProcessOnExited;
                    currentWatchedProcess.Dispose();
                    currentWatchedProcess = null;
                }
                    
                //Kill any process that could be in existence
                _executableLibrary.KillProcesses(new []{processName});
            }
        }

        /// <summary>
        /// Execute inside of an lock statement
        /// Starts the VR Desktop process.
        /// </summary>
        private void StartLogic()
        {
            _vrGuiProcessExecution = StartProcess(_vrDesktopModuleConfig);
        }

        /// <summary>
        /// Execute inside of an lock statement
        /// Creates the VRDesktop process and starts it
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns></returns>
        private Process StartProcess(VrDesktopModuleConfig config)
        {
            string appBaseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if(String.IsNullOrEmpty(appBaseDir))
            {
                Logger.Error("Could not get Application Base Directory!");
                return null;
            }

            var vrDesktopExecutablePath = Path.Combine(appBaseDir, _vrDesktopModuleConfig.VrDesktopExecutableRelativeFilePath);
            if(!File.Exists(vrDesktopExecutablePath))
            {
                Logger.Error($"VR_Desktop Executable does not exist in Path={vrDesktopExecutablePath}");
                return null;
            }

            var startInfo = new ProcessStartInfo
                            {
                                    Arguments = config.VrDesktopExecutableParameters,
                                    FileName = vrDesktopExecutablePath
                            };
            var vrDesktopProcess = new Process();
            vrDesktopProcess.Exited += VRDesktopProcessOnExited;
            vrDesktopProcess.EnableRaisingEvents = true;
            vrDesktopProcess.StartInfo = startInfo;
            vrDesktopProcess.Start();
            return vrDesktopProcess;
        }

        /// <summary>
        /// Process notification to detect unintended close of VRDesktop
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VRDesktopProcessOnExited(object sender, EventArgs e)
        {
            lock(_vrDesktopLock)
            {
                //Check if the sender is our current process
                if(sender != null && sender is Process sendingProcess && sendingProcess.Equals(_vrGuiProcessExecution))
                {
                    //Its our process
                    _vrGuiProcessExecution.Exited -= VRDesktopProcessOnExited;
                    _vrGuiProcessExecution.Dispose();
                    _vrGuiProcessExecution = null;
                    EnsureState(ShouldBeRunning, ref _vrGuiProcessExecution,_vrDesktopModuleConfig.VrDesktopProcessName);
                }
            }
        }

        private void HandleNoKnownRunningProcess(ref Process currentProcess, string processName)
        {
            //Is there any other process running ?
            var anyRunningVrDesktopProcess =_executableLibrary.FindProcess(processName).FindAll(x=> !x.HasExited);

            //We do have one running
            if(anyRunningVrDesktopProcess.Count == 1)
            {
                //All good we have one running process, however its not ours
                //Assign the new one
                currentProcess = anyRunningVrDesktopProcess.First();
                //Enable events and subscribe
                currentProcess.Exited += VRDesktopProcessOnExited;
                currentProcess.EnableRaisingEvents = true;
                return;
            }

            if(anyRunningVrDesktopProcess.Count > 1)
            {
                //We have multiple processes, that should not have happened
                //We kill each of them
                foreach(var process in anyRunningVrDesktopProcess)
                {
                    _executableLibrary.KillProcess(process);
                }
            }
            //We had either no or multiple processes that were killed
            //Now start a new process
            //We Start a new one and leave
            StartLogic();
            return;
        }
        #endregion Methods
    }
}
