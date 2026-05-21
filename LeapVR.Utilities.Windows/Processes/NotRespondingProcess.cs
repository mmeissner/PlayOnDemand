#region Licence
/****************************************************************
 *  Filename: NotRespondingProcess.cs
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
using System.Diagnostics;
using System.Threading;
using NLog;

namespace LeapVR.Utilities.Windows.Processes
{
    /// <summary>
    /// Provides a way to recognize and interpret behavior of processes that tend to not respond
    /// Will monitor if the process is not responding and if it is using cpu time
    /// A Process that is for too long time not responding will be killed
    /// A Process will be monitored for longer if he shows CPU activity to prevent falase positives
    /// during loading of games.
    /// </summary>
    public class NotRespondingProcess
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// This is the maximum time in ms to wait for a process that is not responding and does not show a cpu usage
        /// </summary>
        private const int WaitForResumingFromNotRespondingTimeout = 6000;
        /// <summary>
        /// The time interval in ms in that a process needs to have cpu usage to be seen as cpu active
        /// </summary>
        private const int NotRespondingMaxTimeNoCpuUsage = 1500;
        /// <summary>
        /// The maximum total time in ms when a process is not responding but still shows cpu usage
        /// </summary>
        private const int NotRespondingCpuUsageTimeout = 10000;
        /// <summary>
        /// The time between measure/probe cycles in ms
        /// </summary>
        private const int SleepBetweenCyclesMs = 1000;
        private readonly Process _process;
        private readonly Action<Process> _killAction;


        /// <summary>
        /// Initializes a new instance of the <see cref="NotRespondingProcess"/> class.
        /// </summary>
        /// <param name="process">The process to monitor.</param>
        /// <param name="killAction">The kill action to invoke if the process is declared dead.</param>
        public NotRespondingProcess(Process process, Action<Process> killAction)
        {
            _process = process;
            _killAction = killAction;
        }

        /// <summary>
        /// Waits for exit of a process
        /// Can also kill an hanging process if its not responding
        /// </summary>
        public void WaitForExit()
        {
            Logger.Debug("Process is requires not Responding Monitoring!");
            //Loop
            bool isAlive = true;
            var notRespondingStopWatch = new Stopwatch();
            bool stopwatchRunning = false;
            while (isAlive)
            {
                //Process is definitly gone
                if (_process.HasExited) return;

                //Process seem to be not Responding
                if (!_process.Responding)
                {
                    //Start Stopwatch to measure max time it is allowed to not respond
                    if (!stopwatchRunning)
                    {
                        stopwatchRunning = true;
                        notRespondingStopWatch.Start();
                    }
                }
                //Proces is responding!
                else
                {
                    //We need to stop the Watch if we had it started
                    if (stopwatchRunning)
                    {
                        stopwatchRunning = false;
                        notRespondingStopWatch.Reset();
                    }
                    //We continue Monitoring
                    Thread.Sleep(SleepBetweenCyclesMs);
                    continue;
                }

                //Condition if Process in for to long time not responding
                if (notRespondingStopWatch.ElapsedMilliseconds > WaitForResumingFromNotRespondingTimeout)
                {
                    //Check now if process has activity, some applications are not responding during loading
                    var cpuWatchdog = new ProcessCpuWatchdog(_process);
                    var cpuWatchdogStopWatch = new Stopwatch();
                    var cpuWatchdogMaxTimeNoCpuUsage = new Stopwatch();
                    bool maxTimeNoCpuUsageStopwatchActive = false;
                    cpuWatchdogStopWatch.Start();

                    bool runWatchDog = true;

                    //Start Cpu Monitoring
                    do
                    {
                        //Sleep awhile between cpu measure cycles
                        Thread.Sleep(200);

                        //No Measurable CPU usage
                        if (Math.Abs(cpuWatchdog.CpuUsage) <= 0.01)
                        {
                            //We already measuring the time for no CPU Activity
                            if (maxTimeNoCpuUsageStopwatchActive)
                            {
                                //Timeout Condition for No Cpu Activity to declare proces dead
                                if (NotRespondingMaxTimeNoCpuUsage >=
                                    cpuWatchdogMaxTimeNoCpuUsage.ElapsedMilliseconds)
                                {
                                    isAlive = false;
                                    runWatchDog = false;
                                    Logger.Warn("Process that is not responding, has no cpu activity and is declared death");
                                }
                                //Timeout still not reached
                                continue;
                            }
                            //We do not Meause the process yet
                            cpuWatchdogMaxTimeNoCpuUsage.Start();
                            maxTimeNoCpuUsageStopwatchActive = true;
                        }
                        //Cpu Activity detected
                        else
                        {
                            //If the temporary stopwatch is active we need to reset it as we now have activity
                            if (maxTimeNoCpuUsageStopwatchActive)
                            {
                                cpuWatchdogMaxTimeNoCpuUsage.Reset();
                                maxTimeNoCpuUsageStopwatchActive = false;
                            }
                            //If the Process is responding again all will be good
                            //and we can break out here
                            if (_process.Responding)
                            {
                                Logger.Info($"Monitored Process Name={_process.ProcessName}, recovered from not responding!");
                                break;
                            }
                            //If its still not responding we need to check the maximal Timeout
                            if (cpuWatchdogStopWatch.ElapsedMilliseconds >= NotRespondingCpuUsageTimeout)
                            {
                                //We have to give up its fubar
                                runWatchDog = false;
                                isAlive = false;
                                Logger.Warn("Process that is not responding has cpu activity but exeded the timout for not responding with cpu activity");
                            }
                        }


                    } while (runWatchDog);

                    if (!isAlive)
                    {
                        Logger.Warn($"Monitored Process Name={_process.ProcessName}, with Not Responding seems to be dead, and we going to kill it");
                        _killAction(_process);
                    }
                }
                Thread.Sleep(SleepBetweenCyclesMs);
            }
        }
    }
}