#region Licence
/****************************************************************
 *  Filename: ConsoleProcess.cs
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace LeapVR.Shared.Lib.Win
{
    public static class ConsoleProcess
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static int Run(string exeFile, string arguments, out List<string> output, out List<string> error, string workingDirectory = null)
        {
            ProcessStartInfo processStartInfo = GetProcessStartInfo(exeFile, arguments);
            if (!String.IsNullOrEmpty(workingDirectory)) processStartInfo.WorkingDirectory = workingDirectory;
            Logger.Debug($"Calling |Exe: {exeFile} |Arguments: {arguments}");

            using (var process = Process.Start(processStartInfo))
            {
                // Read stderr synchronously (on another thread)
                var internalOutput = new StringBuilder();
                var errorOutput = new StringBuilder();
                var stderrThread = new Thread(
                    () =>
                    {
                        while (true)
                        {
                            var line = process.StandardError.ReadLine();
                            if (line == null)
                                break;
                            errorOutput.AppendLine(line);
                        }
                    }
                );
                stderrThread.Start();

                // Read stdout synchronously (on this thread)

                while (true)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line == null)
                        break;
                    internalOutput.AppendLine(line);
                    // ... Do something with the line here ...
                }

                process.WaitForExit();
                stderrThread.Join();

                // ... Here you can do something with errorText ...
                error = errorOutput.ToList();
                output = internalOutput.ToList();
                return process.ExitCode;
            }
        }

        private static ProcessStartInfo GetProcessStartInfo(string exeFile,string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = exeFile,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };
            return processStartInfo;
        }
    }
    public class ConsoleProcessEventOutput
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public object Tag { get; set; }
        public Process Process { get; private set; }
        public void Run(ProcessStartInfo processStartInfo)
        {
            Task.Run(delegate { RunProcess(processStartInfo); });
        }
        public void Run(string exeFile, string arguments, string workingDirectory = null)
        {
            Task.Run(delegate { RunProcess(exeFile, arguments, workingDirectory); });
        }

        private void RunProcess(string exeFile, string arguments, string workingDirectory = null)
        {
            ProcessStartInfo processStartInfo = GetProcessStartInfo(exeFile, arguments);
            if (!String.IsNullOrEmpty(workingDirectory)) processStartInfo.WorkingDirectory = workingDirectory;
            Logger.Debug($"Calling |Exe: {exeFile} |Arguments: {arguments}");
        }

        private void RunProcess(ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            using (Process = Process.Start(processStartInfo))
            {
                // Read stderr synchronously (on another thread)
                var internalOutput = new StringBuilder();
                var errorOutput = new StringBuilder();
                var stderrThread = new Thread(
                    () =>
                    {
                        while (true)
                        {
                            var line = Process.StandardError.ReadLine();
                            if (line == null)
                                break;
                            ErrOutput?.Invoke(this,line);
                            errorOutput.AppendLine(line);
                        }
                    }
                );
                stderrThread.Start();

                // Read stdout synchronously (on this thread)

                while (true)
                {
                    var line = Process.StandardOutput.ReadLine();
                    if (line == null)
                        break;
                    StdOutput?.Invoke(this,line);
                    internalOutput.AppendLine(line);
                    // ... Do something with the line here ...
                }

                Process?.WaitForExit();
                stderrThread.Join();
                ProcessExited?.Invoke(this,Process.ExitCode);
            }
        }

        public event Action<object,int> ProcessExited;
        public event Action<object,string> StdOutput;
        public event Action<object,string> ErrOutput;

        private ProcessStartInfo GetProcessStartInfo(string exeFile, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = exeFile,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };
            return processStartInfo;
        }
    }

    internal static class ListExtensions
    {
        public static List<string> ToList(this StringBuilder stringBuilder)
        {
            return stringBuilder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }
    }
}
