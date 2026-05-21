#region Licence
/****************************************************************
 *  Filename: ExecutableUtil.cs
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
using System.Management;
using System.Threading;
using LeapVR.Shared.Lib.Win.WinApi;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using LeapVR.Utilities.Windows.Executable;
using NLog;
using Process = System.Diagnostics.Process;

namespace LeapVR.Utilities.Windows
{
    public class ExecutableLibrary
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Executes a file
        /// </summary>
        /// <param name="filepath">The file path to the file to execute</param>
        /// <param name="workingPath">The working directory, if null then it's the same with filepath directory</param>
        /// <param name="args">Arguments to execute the file with</param>
        /// <param name="hidden">Start as hidden without window</param>
        /// <param name="noWindow">Execute without showing a window</param>
        /// <returns>The started Process</returns>
        public Process Execute(string filepath, string workingPath = null, string args = null,
            bool hidden = false, bool noWindow = false)
        {
            try
            {
                Logger.Debug("Starting Process with filepath {0}, working Path {1} and args {2}", filepath, workingPath,
                    args);
                if (!File.Exists(filepath))
                {
                    Logger.Error($"File with Filepath: {filepath} does not exist!");
                    return null;
                }
                var file = new FileInfo(filepath);
                var type = StartableType(file);
                if (type == FileType.Shortcut ||
                    type == FileType.Unset)
                {
                    return Process.Start(filepath);
                }
                var start = new ProcessStartInfo { FileName = file.FullName };

                //Set default or explicit working Path
                if (String.IsNullOrWhiteSpace(workingPath) && !String.IsNullOrWhiteSpace(file.DirectoryName))start.WorkingDirectory = file.DirectoryName;
                else if (!String.IsNullOrWhiteSpace(workingPath)) start.WorkingDirectory = workingPath;

                //Set Command line arguments
                if (!String.IsNullOrWhiteSpace(args)) start.Arguments = args;
                start.UseShellExecute = false;
                start.WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
                start.CreateNoWindow = noWindow;
                return Process.Start(start);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"Exception on exection start of {filepath}");
                throw;
            }
        }
        public Process Execute(ProcessStartInfo startInfo)
        {
            return Process.Start(startInfo);
        }
        public void KillProcess(string processName)
        {
            KillProcesses(new[] { processName });
        }
        public void KillProcesses(string[] processNames)
        {
            //Get Exe name
            var ownExe = GetOwnProcessName();
            foreach (Process process in Process.GetProcesses())
            {
                foreach (string processName in processNames)
                {
                    if (String.IsNullOrEmpty(processName) ||
                        process.ProcessName.ToLowerInvariant().Contains(ownExe.ToLowerInvariant())) continue;
                    if (process.ProcessName.ToLowerInvariant().Contains(processName.ToLowerInvariant()))
                    {
                        Logger.Warn("Killing Proccess {1} that contains {0} in Processname", processName,
                            process.ProcessName);
                        try
                        {
                            KillProcess(process);
                        }
                        catch (Exception exception)
                        {
                            var processes = processName.Aggregate("", (current, name) => current + name + ", ");
                            Logger.Error(exception, "Exception occured during try to kill processes with name: {0}",
                                processes);
                        }
                    }
                }
            }
        }
        public void WaitForProcessesExit(string[] processNames)
        {
            //Get Exe name
            var ownExe = GetOwnProcessName();
            //If its a multi process program that terminates one process and starts another and vice versa, so we have to check again
            bool run;
            var anyProcessFound = false;
            do
            {
                run = true;
                foreach (Process process in Process.GetProcesses())
                {
                    foreach (string processName in processNames)
                    {
                        if (String.IsNullOrEmpty(processName) ||
                            process.ProcessName.ToLowerInvariant().Contains(ownExe.ToLowerInvariant())) continue;
                        if (!process.ProcessName.ToLowerInvariant().Contains(processName.ToLowerInvariant())) continue;
                        Logger.Debug("Found Proccess {1} that contains {0} in Processname and Wait now for Exit",
                            processName, process.ProcessName);
                        anyProcessFound = true;
                        process.WaitForExit();
                    }
                }
                //Loop until no processes are found
                if (anyProcessFound) anyProcessFound = false;
                else run = false;
            } while (run);
        }

        /// <summary>
        /// Searches for a prcoesses name that contains a specific string (case insensitive)
        /// </summary>
        /// <param name="processName">cointaining string</param>
        /// <param name="logging"></param>
        /// <returns>List with found processes</returns>
        public List<Process> FindProcess(string processName, bool logging = true, bool logfound = true)
        {
            if (String.IsNullOrEmpty(processName))
            {
                Logger.Warn("Provided processName is null or empty!");
                return new List<Process>();
            }
            var processes =
                Process.GetProcesses()
                    .Where(
                        process =>
                            process.ProcessName.ToLower()
                                .Equals(processName.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            if (logging) Logger.Debug("Found {0} Processes with name {1}", processes.Count, processName);
            if (processes.Any() && logfound)
                Logger.Debug("Found {0} Processes with name {1}", processes.Count, processName);
            return processes;
        }

        /// <summary>
        /// Finds the processes with the specified name.
        /// </summary>
        /// <param name="processNames">The process names.</param>
        /// <param name="logging">if set to <c>true</c> [logging].</param>
        /// <param name="logfound">if set to <c>true</c> [logfound].</param>
        /// <returns></returns>
        public List<Process> FindProcesses(List<string> processNames, bool logging = true,
            bool logfound = true)
        {
            var retval = new List<Process>();
            if (!processNames.Any())
            {
                Logger.Warn("There are no Provided processNames!");
                return new List<Process>();
            }

            var stemp = processNames.Aggregate("", (current, name) => current + name + ",");
            Logger.Debug($"Searching for Processes:{stemp}");
            var processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                foreach (string name in processNames)
                {
                    if (string.Equals(process.ProcessName, name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        retval.Add(process);
                        if (logfound || logging)
                            Logger.Debug("Found Process with name {0}", process.ProcessName);
                    }
                }
            }
            if (logging) Logger.Debug("Found {0} Processes", retval.Count);
            return retval;
        }

        public void KillProcess(Process process)
        {
            if (process == null)
            {
                Logger.Error("Process provided is null!");
                return;
            }
            if (process.HasExited)
            {
                Logger.Debug("Process already exited!");
                return;
            }
            var processId = process.Id;
            try
            {
                Logger.Warn("Trying to closs Process by CloseMainWindow");
                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    // Try closing application by sending WM_CLOSE to all child windows in all threads.
                    foreach (ProcessThread pt in process.Threads)
                    {
                        User32.EnumThreadWindows((uint)pt.Id, EnumThreadCallback, IntPtr.Zero);
                    }
                }
                else
                {
                    // Try to close main window.
                    if (process.CloseMainWindow())
                    {
                        // Free resources used by this Process object.
                        process.Close();
                    }
                }
                Thread.Sleep(1000);
                //Check if it worked and process is gone
                if (process.HasExited) return;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Proccess.HasExited throw after CloseMainWindow");
            }
            try
            {
                //Try to Close
                Logger.Warn("Trying to closs Process by Close");
                var close = Process.GetProcessById(processId);
                close.Close();
                Thread.Sleep(500);
                if (close.HasExited) return;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Proccess.HasExited throw after Close");
            }
            try
            {
                var kill = Process.GetProcessById(processId);
                Logger.Warn("Trying to closs Process by Kill");
                kill.Kill();
                Thread.Sleep(300);
                if (kill.HasExited) return;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Proccess.HasExited throw after Kill");
            }

            try
            {
                var killTree = Process.GetProcessById(processId);
                Logger.Warn("Trying to closs Process by KillProcessTree");
                KillProcessTree(killTree);
                if (killTree.HasExited) return;
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "KillProcessTree throw");
            }
        }

        public string GetProcessNameFromExePath(string filePathName)
        {
            return Path.GetFileNameWithoutExtension(filePathName);
        }

        private bool EnumThreadCallback(IntPtr hWnd, IntPtr lParam)
        {
            // Close the enumerated window.
            UIntPtr lRes;
            User32.SendMessageTimeout(hWnd, (int)User32.Wm.Close, UIntPtr.Zero, IntPtr.Zero,
                User32.SendMessageTimeoutFlags.SmtoAbortifhung, 1000, out lRes);
            return true;
        }

        private FileType StartableType(FileInfo fileInfo)
        {
            switch (fileInfo.Extension.ToLowerInvariant())
            {
                case ".exe":
                    return FileType.Executable;
                case ".bat":
                    return FileType.BatchFile;
                case ".lnk":
                    return FileType.Shortcut;
                default:
                    return FileType.Unset;
            }
        }

        public void KillProcessTree(Process root)
        {
            if (root != null)
            {
                var list = new List<Process>();
                GetProcessAndChildren(Process.GetProcesses(), root, list, 1);

                foreach (Process p in list)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch (Exception exception)
                    {
                        //Log error?
                        Logger.Warn(exception, "KillProcessTree throw");
                    }
                }
            }
        }

        private int GetParentProcessId(Process p)
        {
            int parentId = 0;
            try
            {
                var mo = new ManagementObject("win32_process.handle='" + p.Id + "'");
                mo.Get();
                parentId = Convert.ToInt32(mo["ParentProcessId"]);
            }
            catch
            {
                parentId = 0;
            }
            return parentId;
        }

        private void GetProcessAndChildren(Process[] plist, Process parent, List<Process> output, int indent)
        {
            foreach (Process p in plist)
            {
                if (GetParentProcessId(p) == parent.Id)
                {
                    GetProcessAndChildren(plist, p, output, indent + 1);
                }
            }
            output.Add(parent);
        }

        private string GetOwnProcessName()
        {
            var ownExe = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            if (String.IsNullOrEmpty(ownExe)) ownExe = "vrlaunchkit";
            return ownExe;
        }
    }
}
