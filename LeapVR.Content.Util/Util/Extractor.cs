#region Licence
/****************************************************************
 *  Filename: Extractor.cs
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
using LeapVR.Content.Util.Archive;
using LeapVR.Content.Util.Game;
using NLog;

namespace LeapVR.Content.Util.Util
{
    public class Extractor
    {
        #region Private Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ArchiveReport _report;
        private Archive.Archive _archive;
        private string _targetDir;
        private string _password;

        private DirectoryInfo _finalDirectory;
        #endregion

        #region Public Properties
        public Archive.Archive Archive => _archive;
        public string TargetDirectory => _targetDir;
        public DirectoryInfo ExtractionDirectory => _finalDirectory;
        #endregion

        #region Constructor
        public Extractor(Archive.Archive archive, string targetDir, string password ="")
        {
            _report = null;
            _archive = archive;
            _targetDir = targetDir;
            _password = password;
        }
        public Extractor(ArchiveReport report, string targetDir, string password = "")
        {
            _report = report;
            _archive = report.Archive;
            _targetDir = targetDir;
            _password = password;
        }
        #endregion

        #region Methods
        public bool Extract(bool rebaseRoot = true)
        {
            string extractDir;

            //No Rebase
            if (!rebaseRoot)
            {
                if (ExtractArchive(_targetDir, _password))
                {
                    _finalDirectory = new DirectoryInfo(Path.Combine(_targetDir));
                    return true;
                }
                return false;
            }

            //Rebase for No Game Archive
            if (_report?.GameInfo?.Root == null)
            {
                extractDir = Path.Combine(_targetDir, $"_NoGame_{_archive.ArchiveFile.Name}");
                if (ExtractArchive(extractDir, _password))
                {
                    _finalDirectory = new DirectoryInfo(Path.Combine(extractDir));
                    return true;
                }
                return false;
            }

            //Rebase for Game
            if (_report.GameRootDirectoryDepth == 0)
            {
                //Add Directory with game name
                //Get GameName from Exe File
                extractDir = Path.Combine(_targetDir, GameNameFromExe(_report.GameInfo));
                if (ExtractArchive(extractDir, _password))
                {
                    _finalDirectory = new DirectoryInfo(Path.Combine(extractDir));
                    return true;
                }
                return false;
            }

            bool blacklistedGameRootName = false;
            bool extractionSuccess = false;
            var filter = $"{_report.GameInfo.Root.RelativeFullName}\\*";
            string targetName = null;

            extractDir = Path.Combine(_targetDir, "tmp");
            //Needs to be unpacked into temp target dir and then processed/moved/renamed
            if (_report.GameRootDirectoryDepth >= 1)
            {
                //Normally the right naming, but need to check against blacklist
                foreach (string directoryExclusion in Library.DirectoryExclusions)
                {
                    if (_report.GameInfo.Root.RelativeFullName.EndsWith(directoryExclusion,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        blacklistedGameRootName = true;
                        targetName = _report.ArchiveRoot.Directories[0].Name;
                    }
                }
                if (!blacklistedGameRootName)
                {
                    var parts = _report.GameInfo.Root.RelativeFullName.Split(new[] {'\\'}, StringSplitOptions.RemoveEmptyEntries);
                    targetName = parts[parts.Length-1];
                }
            }
            if (_report.GameRootDirectoryDepth >= 1)
            {
                extractionSuccess = ExtractArchive(extractDir, _password, filter);
            }
            if (extractionSuccess)
            {
                if (String.IsNullOrEmpty(targetName))targetName = GameNameFromExe(_report.GameInfo);
                var gamePath = new DirectoryInfo(Path.Combine(extractDir, _report.GameInfo.Root.RelativeFullName));
                if (gamePath.Exists)
                {
                    _finalDirectory = new DirectoryInfo(Path.Combine(_targetDir, targetName));
                    if(!_finalDirectory.Exists) gamePath.MoveTo(_finalDirectory.FullName);
                    else Debug.WriteLine($@"Cant move ""{gamePath.FullName}"" to ""{_finalDirectory.FullName}""!");
                }
                else extractionSuccess = false;

            }
            if(Directory.Exists(extractDir)) DeleteDirectory(extractDir);
            return extractionSuccess;
        }
        #endregion

        #region Private Methods
        private string GameNameFromExe(GameInfo info)
        {
            //string retval;
            string nameFromExe = null;
            var contentfile = Enumerable.FirstOrDefault<ContentFile>((from file in info.GameExes where !String.IsNullOrEmpty(file.FileName) select file));
            nameFromExe = contentfile == null ? $"UnnamedTitle_{DateTime.Now:yyyyMMddTHHmmss}" : contentfile.FileName.Substring(0, contentfile.FileName.Length - contentfile.Extension.Length);
            return nameFromExe;
        }   
        private bool ExtractArchive(string target,  string password = "", string filter = null)
        {
            return _archive.Extract(target, password, filter);
        }
        private bool DeleteDirectory(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath)) Directory.Delete(directoryPath, true);
            }
            catch (IOException)
            {
                if (Directory.Exists(directoryPath)) Directory.Delete(directoryPath, true);
            }
            catch (UnauthorizedAccessException)
            {
                if (Directory.Exists(directoryPath)) Directory.Delete(directoryPath, true);
            }
            if (Directory.Exists(directoryPath)) DeleteDirectory(directoryPath);
            return !Directory.Exists(directoryPath);
        }
        #endregion
    }
}
