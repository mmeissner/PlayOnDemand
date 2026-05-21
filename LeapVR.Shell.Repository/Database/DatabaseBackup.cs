#region Licence
/****************************************************************
 *  Filename: DatabaseBackup.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2018-1-19
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
using System.Globalization;
using System.IO;
using System.Linq;
using NLog;

namespace LeapVR.Shell.Repository.Database
{
    [Obsolete("This class will be removed in the next versions, do not use!")]
    public static class DatabaseBackup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string _dateTimeFormat = "yyyyMMdd_hhmmss";

        public static string DatabaseFileName { get; private set; }
        public static string DatabaseFilePath { get; private set; }
        public static string BackupDirectory { get; private set; }

        public static int CleanupKeepNewestCount { get; } = 5;

        private static bool _isInitialized;
        private static readonly object _lock = new object();

        internal static void Initialize(string databaseFilePath, string databaseFileName,string databaseBackupDir)
        {
            lock (_lock)
            {
                DatabaseFileName = databaseFileName;
                DatabaseFilePath =databaseFilePath;

                BackupDirectory = databaseBackupDir;
                Directory.CreateDirectory(BackupDirectory);

                _isInitialized = true;
            }
        }

        public static string Backup()
        {
            var backupFileName = $"{DateTime.Now.ToString(_dateTimeFormat, CultureInfo.InvariantCulture)}_{DatabaseFileName}";
            Logger.Info($"{nameof(DatabaseBackup)}.{nameof(Backup)} called to create new backupFileName = `{backupFileName}`.");

            lock (_lock)
            {
                EnsureInitialized();

                if (!File.Exists(DatabaseFilePath))
                {
                    throw new InvalidOperationException("Cannot Backup database; Database file doesn't exists.");
                }

                var backupFilePath = Path.Combine(BackupDirectory, backupFileName);
                File.Copy(DatabaseFilePath, backupFilePath);
                return backupFileName;
            }
        }

        public static bool TryRestore(string backupFileName)
        {
            Logger.Info( $"{nameof(DatabaseBackup)}.{nameof(TryRestore)} called to restore backupFileName = `{backupFileName}`.");
            lock (_lock)
            {
                EnsureInitialized();

                string tempFilePath = Path.GetTempFileName();
                bool isCopiedToTemp = false;
                try
                {
                    var backupFilePath = Path.Combine(BackupDirectory, backupFileName);
                    if (!File.Exists(backupFilePath))
                    {
                        Logger.Info($"{nameof(DatabaseBackup)}.{nameof(TryRestore)} failed for backupFileName = `{backupFileName}`; Backup file does not exists.");
                        return false;
                    }

                    File.Copy(DatabaseFilePath, tempFilePath, true);
                    isCopiedToTemp = true;
                    File.Delete(DatabaseFilePath);
                    File.Copy(backupFilePath, DatabaseFilePath);
                    return true;
                }
                catch (System.Exception e)
                {
                    Logger.Warn(e, $"{nameof(DatabaseBackup)}.{nameof(TryRestore)} failed for backupFileName = `{backupFileName}`");

                    try
                    {
                        if (isCopiedToTemp)
                        {
                            File.Copy(tempFilePath, DatabaseFileName, true);
                        }
                    }
                    catch (System.Exception e2)
                    {
                        Logger.Fatal(e2, $"{nameof(DatabaseBackup)}.{nameof(TryRestore)} FATAL - RESTORE TEMP FAILED WITH EXCEPTION {e2}.");
                    }

                    return false;
                }
            }
        }

        public static void DeleteBackup(string backupFileName)
        {
            Logger.Info($"{nameof(DatabaseBackup)}.{nameof(DeleteBackup)} called to delete backupFileName = `{backupFileName}`.");
            lock (_lock)
            {
                EnsureInitialized();

                var backupFilePath = Path.Combine(BackupDirectory, backupFileName);
                File.Delete(backupFilePath);
            }
        }

        public static void CleanupBackups()
        {
            Logger.Info($"{nameof(DatabaseBackup)}.{nameof(CleanupBackups)} called to cleanup old not-needed backups.");
            lock (_lock)
            {
                EnsureInitialized();

                var backupFilePaths = Directory.GetFiles(BackupDirectory, "*", SearchOption.TopDirectoryOnly);
                if (backupFilePaths.Length <= CleanupKeepNewestCount)
                {
                    Logger.Info($"{nameof(DatabaseBackup)}.{nameof(CleanupBackups)} found no old backups that needs to be deleted.");
                    return;
                }

                var oldBackupsToDelete = backupFilePaths
                    .Select(q => new
                    {
                        FilePath = q,
                        FileName = Path.GetFileName(q),
                        DateTime = DateTime.ParseExact(Path.GetFileName(q).Substring(0, _dateTimeFormat.Length), _dateTimeFormat, CultureInfo.InvariantCulture),
                    })
                    .OrderBy(q => q.DateTime)
                    .Take(backupFilePaths.Length - CleanupKeepNewestCount);

                foreach (var oldBackup in oldBackupsToDelete)
                {
                    Logger.Info($"{nameof(DatabaseBackup)}.{nameof(CleanupBackups)} is deleting old backup file `{oldBackup.FileName}`.");
                    File.Delete(oldBackup.FilePath);
                }
            }
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(DatabaseBackup)} is not initialized; Call {nameof(Initialize)}(...) method to initialize first.");
            }
        }
    }
}
