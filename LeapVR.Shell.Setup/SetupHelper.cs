#region Licence
/****************************************************************
 *  Filename: SetupHelper.cs
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
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Utilities.Windows;
using LeapVR.Utilities.Windows.TaskScheduler;
using Microsoft.Win32;
using NLog;

namespace LeapVR.Shell.Setup
{
    public class SetupHelper
    {       
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IConfigFileRepository<DiskConfig> _diskConfigFileRepository;
        private readonly IConfigFileRepository<SystemConfig> _systemConfigFileRepository;
        private bool _powerShellInstalled;

        public SetupHelper(IConfigFileRepository<DiskConfig> diskConfigFileRepository, IConfigFileRepository<SystemConfig> systemConfigFileRepository)
        {
            _diskConfigFileRepository = diskConfigFileRepository;
            _systemConfigFileRepository = systemConfigFileRepository;
            _powerShellInstalled = IsPowerShellInstalled();
        }
        public bool IsPowerShellInstalled()
        {
            string regval = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\3", "Install", null).ToString();
            if(regval.Equals("1"))
            {
                _powerShellInstalled = true;
                return true;
            }
            else
            {
                _powerShellInstalled = false;
                return false;
            }
        }
        public bool IsWerEnabled()
        {
            var disabledWer = RegistryUtil.GetValueData(
                    $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting",
                    "Disabled");
            var dontShowUi = RegistryUtil.GetValueData(
                    $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting",
                    "DontShowUI");
            if(String.IsNullOrEmpty(disabledWer) || String.IsNullOrEmpty(dontShowUi)) return true;
            return disabledWer.Equals("0") && dontShowUi.Equals("0");
        }
        public bool GetLeapVRTaskInfo(out ITaskInfo foundTask)
        {
            return TaskSchedulerUtil.GetTaskInfo(_systemConfigFileRepository.Get().LeapVRTaskName, out foundTask) &&
                   foundTask.Enabled;
        }
        public bool GetExcludedFoldersFromWinDefender(out List<string> excludedFolders)
        {
            excludedFolders = new List<string>();
            if(!_powerShellInstalled) return false;
            try
            {
                using (PowerShell powerShellInstance = PowerShell.Create(RunspaceMode.NewRunspace))
                {
                    powerShellInstance.AddScript("$test = Get-MpPreference" + Environment.NewLine + "$test.ExclusionPath");
                    var psOutput = powerShellInstance.Invoke();
                    foreach(var psObject in psOutput)
                    {
                        if(psObject != null)excludedFolders.Add(psObject.ToString());
                    }
                    return true;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Exception GetExcludedFoldersFromWinDefender");
                return false;
            }
        }

        public Task<bool> ChangeAutoStart(bool autostartWithWindows)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        var taskFound = GetLeapVRTaskInfo(out var foundTaskInfo);
                        var taskName = _systemConfigFileRepository.Get().LeapVRTaskName;
                        //We delete also a old task
                        bool retval = true;
                        if(taskFound)
                        {
                            retval= TaskSchedulerUtil.DeleteTask(foundTaskInfo);
                        }
                        //Create Task
                        if(autostartWithWindows)
                        {
                            retval= TaskSchedulerUtil.CreateUserLogOnTask(
                                    new TaskSchedulerUtil.ExecuteAppAction(GlobalConfig.GetGlobalConfiguration().ShellExecutablePath),taskName);
                        }

                        return retval;
                    }
                    catch(Exception exception)
                    {
                        Logger.Error(exception, "Exception during AutoStart Task Creation");
                        return false;
                    }
                });
        }
        public Task<bool> SaveDiskValues(string gameStorageFilePath, int percent, int baseDivision)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        var config = _diskConfigFileRepository.Get();
                        config.StorageBaseDir = gameStorageFilePath;
                        config.SystemDrives =
                                new[] {Directory.CreateDirectory(gameStorageFilePath).Root.Name};
                        config.ReservedDiskSpaceRatio = ConvertFromIntBasedPercentageValue(percent,baseDivision);
                        _diskConfigFileRepository.Store(config);
                        return true;
                    }
                    catch(Exception exception)
                    {
                        Logger.Error(exception, "Exception during AutoStart Task Creation");
                        return false;
                    }
                });
        }
        public Task<bool> ExcludeStorageFromWinDefender(bool excludeDirective, string gameStorageFilePath)
        {
            return Task.Run(
                () =>
                {
                    if(!_powerShellInstalled) return false;
                    var verb = excludeDirective ? "Add-MpPreference" : "Remove-MpPreference";
                    try
                    {
                        using (PowerShell powerShellInstance = PowerShell.Create())
                        {
                            powerShellInstance.AddScript($"{verb} -ExclusionPath \"{gameStorageFilePath}\"");
                            var psOutput = powerShellInstance.Invoke();
                            if(psOutput.Any())
                            {
                                Logger.Error($"Error during ExcludeStorageFromWinDefender: {psOutput.FirstOrDefault()}");
                                return false;
                            }
                            return true;
                        }
                    }
                    catch(Exception exception)
                    {
                        Logger.Error(exception, "Exception during ExcludeStorageFromWinDefender");
                        return false;
                    }
                });
        }
        public Task<bool> SetWer(bool isEnabled)
        {
            return Task.Run(
                    () =>
                    {
                        try
                        {
                            int value = isEnabled ? 0 : 1;
                            //RegistryUtil.
                            if(RegistryUtil.SetValue(
                                       RegistryHive.LocalMachine,
                                       "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting",
                                       "Disabled",
                                       value,
                                       RegistryValueKind.DWord) &&
                               RegistryUtil.SetValue(
                                       RegistryHive.LocalMachine,
                                       "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting",
                                       "DontShowUI",
                                       value,
                                       RegistryValueKind.DWord)) return true;
                            return false;
                        }
                        catch(Exception e)
                        {
                            Logger.Error(e, "Error during Disabling WER");
                            return false;
                        }
                    });
        }
        /// <summary>
        /// Converts from int based value a double based percentage value.
        /// Were double value 1 is 100% 
        /// </summary>
        /// <param name="value">The int Value</param>
        /// <param name="baseDivision">The division of one percent.</param>
        /// <returns></returns>
        private double ConvertFromIntBasedPercentageValue(int value, int baseDivision)
        {
            if(value == 0) return 0;
            var sliderValDouble = Convert.ToDouble(value);
            return sliderValDouble / baseDivision;
        }
        private int ConvertToSliderValue(double ratioValue,int baseDivision)
        {
            return Convert.ToInt32(ratioValue * baseDivision);
        }
    }
}
