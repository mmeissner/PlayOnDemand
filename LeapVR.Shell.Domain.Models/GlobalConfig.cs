#region Licence
/****************************************************************
 *  Filename: GlobalConfig.cs
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
using System.IO;
using LeapVR.Shell.Domain.Models.Customization;
using NLog;
using System.Reflection;
using static System.String;

namespace LeapVR.Shell.Domain.Models
{
    public static class GlobalConfig
    {
        public const string ConfigParameter = "-config";
        public const string UninstallParameter = "-uninstall";
        public const string RemoteDebuggerParameter = "-rdbg";
        public const string DebugParameterShort = "-d";
        public const string DebugParameterLong = "-debug";
        public const string FirewallGroupName = "LeapPlay";

        private static readonly object WriteLock = new object();
        private static GlobalConfiguration _globalConfiguration = null;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string InstallRootDirectoryVariable = "LEAPVRINSTALLBASEDIRECTORY";


        internal const string DbFileName = "LeapPlay.db";
        internal const string DatabaseBackupDir = "DatabaseBackups";
        internal const string ModuleConfigurationDirectory = "Config";
        internal const string BackgroundPlayerIdentifier = "LoginView_Background_Player";
        internal const string DefaultMediaDirectory = "Media";

        internal const string PersistentDirectory = @"%APPDATA%\LeapPlay";
        internal static readonly string ShellBinaryPath = $"%{InstallRootDirectoryVariable}%";
        internal static readonly string ShellExecutablePath = $"%{InstallRootDirectoryVariable}%\\LeapPlay.Shell.exe";


        static GlobalConfig()
        {
            //Set the Install Base Directory Variable if not set in system
            if(Environment.GetEnvironmentVariable(InstallRootDirectoryVariable) == null)
            {
                var executableAssembly = Assembly.GetEntryAssembly();
                var directoryName = Path.GetDirectoryName(executableAssembly.Location);
                if(IsNullOrWhiteSpace(directoryName))
                {
                    Logger.Fatal("Could not get own executable path");
                    return;
                }
                var directory = new DirectoryInfo(directoryName);
                var rootInstallDirectory = directory.FullName;
                if(IsNullOrWhiteSpace(rootInstallDirectory))
                {
                    Logger.Fatal(
                            "Could not get parent directory of executing directory, is software installed properly ?");
                    return;
                }
                Logger.Info($"No LeapVrInstallBaseDirectory Variable found, setting it to:{rootInstallDirectory}");
                Environment.SetEnvironmentVariable(InstallRootDirectoryVariable, rootInstallDirectory);
            }
            else
            {
                Logger.Info($"Found LeapVrInstallBaseDirectory variable already set, using={Environment.GetEnvironmentVariable(InstallRootDirectoryVariable)}");
            }
        }
        public static IGlobalConfiguration GetGlobalConfiguration()
        {
            if(_globalConfiguration == null)
            {
                lock(WriteLock)
                {
                    if(_globalConfiguration != null) return _globalConfiguration;
                    var globalConfiguration = new GlobalConfiguration();
                    if(!Directory.Exists(globalConfiguration.PersistentDirectory))
                    {
                        Directory.CreateDirectory(globalConfiguration.PersistentDirectory);
                    }

                    if(!Directory.Exists(globalConfiguration.ConfigFilesDirectory))
                    {
                        Directory.CreateDirectory(globalConfiguration.ConfigFilesDirectory);
                    }

                    Logger.Info(
                            $"Set PersistentDirectory={globalConfiguration.PersistentDirectory}, " +
                            $"ShellLauncherExecutablePath={globalConfiguration.ShellExecutablePath}, " +
                            $"ConfigurationFileDirectory={globalConfiguration.ConfigFilesDirectory}");
                    _globalConfiguration = globalConfiguration;
                    return _globalConfiguration;
                }
            }

            return _globalConfiguration;
        }
    }
}