#region Licence
/****************************************************************
 *  Filename: ProcessCpuWatchdog.cs
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
using NLog;

namespace LeapVR.Utilities.Windows.Processes
{
    public class ProcessCpuWatchdog
    {
        private static readonly Logger CpuWatchdogLogger = LogManager.GetCurrentClassLogger();
        private DateTime _lastTime;
        private TimeSpan _lastTotalProcessorTime;
        private DateTime _curTime;
        private TimeSpan _curTotalProcessorTime;
        private readonly Process _processToMonitor;
        private bool _isFunctional;

        public ProcessCpuWatchdog(Process processToMonitor)
        {
            _processToMonitor = processToMonitor;
            Initialize();
        }

        public double CpuUsage => CalculateCpuUsage();


        private void Initialize()
        {
            try
            {
                if (_processToMonitor == null || _processToMonitor.HasExited) _isFunctional = false;
                else
                {
                    _lastTime = DateTime.Now;
                    _lastTotalProcessorTime = _processToMonitor.TotalProcessorTime;
                    _isFunctional = true;
                    CpuWatchdogLogger.Info($"ProcessCpuWatchdog initialized for process = {_processToMonitor.ProcessName}!");
                }
            }
            catch (Exception exception)
            {
                CpuWatchdogLogger.Error(exception, "Could not initialize ProcessCpuWatchdog!");
                _isFunctional = false;
            }
        }

        /// <summary>
        /// Calculates the cpu usage from object creation to call or between calls
        /// </summary>
        /// <returns></returns>
        private double CalculateCpuUsage()
        {
            if (_isFunctional == false) return 0;
            try
            {
                _curTime = DateTime.Now;
                _curTotalProcessorTime = _processToMonitor.TotalProcessorTime;

                double cpuUsage = (_curTotalProcessorTime.TotalMilliseconds - _lastTotalProcessorTime.TotalMilliseconds) / _curTime.Subtract(_lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);

                _lastTime = _curTime;
                _lastTotalProcessorTime = _curTotalProcessorTime;
                return cpuUsage;
            }
            catch (Exception exception)
            {
                CpuWatchdogLogger.Error(exception, "Error during calculation of CPU-Usage!");
                throw;
            }
        }

        public override string ToString()
        {
            return String.Format($"{0} CPU: {1:0.0}%", CalculateCpuUsage(), CpuUsage * 100);
        }
    }
}