#region Licence
/****************************************************************
 *  Filename: GlobalConfiguration.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-25
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
using System.IO;
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Customization
{
    
    public interface IGlobalConfiguration {
        /// <summary>
        /// Gets the resolved persistent directory. Example (unresolved:) "%APPDATA%\LeapVR"
        /// </summary>
        /// <value>
        /// The persistent directory.
        /// </value>
        string PersistentDirectory { get; }

        /// <summary>
        /// Gets the resolved ShellExecutablePath. Example (unresolved:) "%LEAPVRINSTALLBASEDIRECTORY%\LeapVR.Shell.exe"
        /// </summary>
        /// <value>
        /// The shell executable path.
        /// </value>
        string ShellExecutablePath { get; }

        /// <summary>
        /// Gets the shell binary path.  Example (unresolved:) "%LEAPVRINSTALLBASEDIRECTORY%"
        /// </summary>
        /// <value>
        /// The shell binary path.
        /// </value>
        string ShellBinaryPath { get; } 

        /// <summary>
        /// Gets the configuration files directory.Example (unresolved:) "%APPDATA%\LeapVR\Config"
        /// </summary>
        /// <value>
        /// The configuration files directory.
        /// </value>
        string ConfigFilesDirectory { get; }

        /// <summary>
        /// Gets the database file path. Example (unresolved:) "%APPDATA%\LeapVR\VRBox.db"
        /// </summary>
        /// <value>
        /// The database file path.
        /// </value>
        string DatabaseFilePath { get; }

        /// <summary>
        /// Gets the database backup directory. Example (unresolved:) "%APPDATA%\LeapVR\DatabaseBackups"
        /// </summary>
        /// <value>
        /// The database backup directory.
        /// </value>
        string DatabaseBackupDirectory { get; }
        
        /// <summary>
        /// Gets the name of the database file.
        /// </summary>
        /// <value>
        /// The name of the database file.
        /// </value>
        string DatabaseFileName { get; }

        /// <summary>
        /// The Media Directory that gets scanned for files after an installation, during the setup of the Database
        /// </summary>
        string MediaDirectory { get; }

        /// <summary>
        /// The Id the Media Player should use that is running in the Background during no Sessions
        /// </summary>
        string BackgroundPlayerId { get; }
    }

    /// <summary>
    /// Global configuration file that points at <see cref="PersistentDirectory"/> with more detailed configurations,
    /// and other global to the whole PC settings.
    /// Is located in main Shell executable directory, serialized as JSON.
    /// </summary>
    
    internal class GlobalConfiguration : IGlobalConfiguration
    {
        public string DatabaseFileName => GlobalConfig.DbFileName;
        public string PersistentDirectory { get; } = Environment.ExpandEnvironmentVariables(GlobalConfig.PersistentDirectory);
        public string ShellBinaryPath { get; } =  Environment.ExpandEnvironmentVariables(GlobalConfig.ShellBinaryPath);
        public string ShellExecutablePath { get; } = Environment.ExpandEnvironmentVariables(GlobalConfig.ShellExecutablePath);
        public string ConfigFilesDirectory { get; } = Path.Combine(Environment.ExpandEnvironmentVariables(GlobalConfig.PersistentDirectory), GlobalConfig.ModuleConfigurationDirectory);
        public string DatabaseFilePath { get; } =Path.Combine(Environment.ExpandEnvironmentVariables(GlobalConfig.PersistentDirectory), GlobalConfig.DbFileName);
        public string DatabaseBackupDirectory { get; } =Path.Combine(Environment.ExpandEnvironmentVariables(GlobalConfig.PersistentDirectory), GlobalConfig.DatabaseBackupDir);
        public string MediaDirectory { get; } = Path.Combine(Environment.ExpandEnvironmentVariables(GlobalConfig.ShellBinaryPath), GlobalConfig.DefaultMediaDirectory);
        public string BackgroundPlayerId { get; } = GlobalConfig.BackgroundPlayerIdentifier;
    }
}
