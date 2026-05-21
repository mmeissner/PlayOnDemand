#region Licence
/****************************************************************
 *  Filename: Uninstaller.cs
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
using System.IO;
using System.Linq;
using System.Threading;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Repository.Database;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace LeapVR.Shell.Setup
{
    public class Uninstaller
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IPlatformController _platformController;
        private readonly SetupHelper _setupHelper;
        private readonly IDiskController _diskController;
        public Uninstaller(IPlatformController platformController,IDiskController diskController, SetupHelper setupHelper)
        {
            _diskController = diskController;
            _setupHelper = setupHelper;
            _platformController = platformController;
        }

        public int StartUninstall()
        {
            var options = new UninstallOptions();
            bool success = true;

            //Remove Startup Task
            if(options.RemoveStartupTask)
            {
                Logger.Debug("Starting to Remove Startup Task");
                var autoStartResult = _setupHelper.ChangeAutoStart(false).Result;
                success = autoStartResult;
            }

            //Enable Wer
            if(options.EnableWer)
            {
                Logger.Debug("Starting to Enable WER");
                var enableWerResult = _setupHelper.SetWer(true).Result;
                if(success) success = enableWerResult;
            }
            //Delete Games
            if(options.DeleteGames)
            {
                Logger.Debug("Starting to Delete Games");
                try
                {
                    var installedApps =_platformController.GetApplicationInstallationData(AppInstallationType.Container).ToList();
                    installedApps.AddRange(_platformController.GetApplicationInstallationData(AppInstallationType.Platform));
                    foreach(IAppInstallationData app in installedApps)
                    {
                        Logger.Debug($"Delete of Game with Guid={app.ApplicationGuid} requested");
                        _platformController.Uninstall(app.ApplicationGuid,app.PlatformPluginGuid,true);
                    }
                    while(_platformController.GetLockedApplications().Any())
                    {
                        Logger.Debug("Waiting as Uninstallation Directory is not empty");
                        Thread.Sleep(500);
                    }
                    Logger.Debug("Delete of Games finished");
                }
                catch(Exception e)
                {
                    success = false;
                    Logger.Error(e,"Error during attempt to Delete Games");
                }
            }
            //Remove Windows Defender Exclusion
            if(options.RemoveWindowsDefenderExclusion && !String.IsNullOrEmpty(_diskController.StorageDirectory))
            {
                Logger.Debug("Starting to Remove Windows Defender Exclusion");
                try
                {
                    if(_setupHelper.GetExcludedFoldersFromWinDefender(out var excludedFolders))
                    {
                        if(excludedFolders.Any(x=> x.ToLowerInvariant().Equals(_diskController.StorageDirectory.ToLowerInvariant())))
                        {
                            var excludeDirResult = _setupHelper.ExcludeStorageFromWinDefender(false,_diskController.StorageDirectory).Result;
                            if(!excludeDirResult)
                            {
                                Logger.Warn("Exclusion from Windows Defender failed");
                                success = false;
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    success = false;
                    Logger.Error(e,"Error during attempt to Remove Windows Defender Exclusion");
                }
            }
            //Delete Custom Files
            if(options.DeleteCustomConfig)
            {
                Logger.Debug("Starting to Delete Config Files");
                var customConfigDir = new DirectoryInfo(GlobalConfig.GetGlobalConfiguration().ConfigFilesDirectory);
                if(customConfigDir.Exists)
                {
                    try
                    {
                        customConfigDir.Delete(true);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e,$"Error during attempt to delete Custom Config Dir={customConfigDir.FullName}");
                        success = false;
                    }
                }
            }
            //Delete Database if Games are deleted
            if(options.DeleteGames)
            {

                Logger.Debug("Starting to Remove Databasebackups");
                var databaseBackupDir = new DirectoryInfo(GlobalConfig.GetGlobalConfiguration().DatabaseBackupDirectory);
                if(databaseBackupDir.Exists)
                {
                    try
                    {
                        databaseBackupDir.Delete(true);
                    }
                    catch(Exception e)
                    {
                        Logger.Error(e,"Error during attempt to delete Database Backups");
                        success = false;
                    }
                }
                Database.CloseDb();
                if(File.Exists(GlobalConfig.GetGlobalConfiguration().DatabaseFilePath))
                {                
                    Logger.Debug("Starting to Remove Database");
                    //It takes some time until the Database File will be released
                    var timeout = TimeSpan.FromSeconds(15);
                    var stopWatch = Stopwatch.StartNew();
                    do
                    {
                        try
                        {
                            File.Delete(GlobalConfig.GetGlobalConfiguration().DatabaseFilePath);
                        }
                        catch(Exception exception)
                        {
                            Logger.Error(exception, "Error during attempt to delete Database");
                        }
                    } while(File.Exists(GlobalConfig.GetGlobalConfiguration().DatabaseFilePath) && stopWatch.Elapsed < timeout);

                    if(File.Exists(GlobalConfig.GetGlobalConfiguration().DatabaseFilePath))
                    {
                        Logger.Warn("Remove Database failed");
                        success = false;
                    }
                }
            }
            return success ? 0 : 1;
        }
    }
}
