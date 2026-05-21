#region Licence
/****************************************************************
 *  Filename: DirectoryAnalyzer.cs
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
using LeapVR.Content.Util.Archive;
using LeapVR.Content.Util.Game;
using NLog;

namespace LeapVR.Content.Util.Util
{
    public class DirectoryAnalyzer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly FileInfo _sevenZipExeFile;

        private HashSet<string> _directoryBlacklist;
        private DirectoryInfo _directoryInfo;
        private int _scanDepth;
        private bool _lookForGames;
        private bool _lookForArchives;

        #region Public Properties
        public FolderType FolderType { get; private set; }
        public List<Archive.Archive> Archives { get; private set; }
        public Dictionary<DirectoryInfo,GameInfo> GameDirectories { get; private set; }
        #endregion
        
        #region Constructor

        /// <summary>
        /// Creates an Analyzer for archives and game directory to provide inside information
        /// </summary>
        /// <param name="sevenZipExe">Path to 7Zip command line executable</param>
        /// <param name="rootDirectory">Directory to Analyze</param>
        /// <param name="scanDepth">Max subdirectory depth to scan </param>
        /// <param name="lookForGames">If game directories should be included into the search</param>
        /// <param name="lookForArchives">If archives should be included into the search</param>
        /// <param name="directoryBlacklist">Directories that should not be scanned, all entries needs to be lowercase invariant</param>
        public DirectoryAnalyzer(FileInfo sevenZipExe,string rootDirectory,int scanDepth = 1, bool lookForGames = true, bool lookForArchives = true, HashSet<string> directoryBlacklist = null)
        {
            if (directoryBlacklist == null) _directoryBlacklist = new HashSet<string>();
            else _directoryBlacklist = directoryBlacklist;
            _sevenZipExeFile = sevenZipExe;
            var directoryInfo = new DirectoryInfo(rootDirectory);
            _directoryInfo = directoryInfo;
            _scanDepth = scanDepth;
            _lookForArchives = lookForArchives;
            _lookForGames = lookForGames;
            FolderType = AnalyzeRootFolderType(directoryInfo, scanDepth, lookForGames, lookForArchives);
        }
        #endregion

        #region Public Events

        public event EventHandler<FoundEventArgs> FoundEvent;

        #endregion

        #region Public Methods

        public void AnalyzeFolderType()
        {
            FolderType = AnalyzeRootFolderType(_directoryInfo, _scanDepth, _lookForGames, _lookForArchives);
        }
        /// <summary>
        /// Provides Information about the executables in an content directory
        /// </summary>
        /// <param name="rootDirectory">The root directory</param>
        /// <param name="includeSubDirectories">Allows to include subdirectories to search in</param>
        /// <param name="filterLauncherLoader">Excludes known launcher executables</param>
        /// <returns>A list with Executables</returns>
        public static List<ContentFile> GetAllExe(ContentDirectory rootDirectory, bool includeSubDirectories = true, bool filterLauncherLoader = true, bool filterInstallExes = true)
        {
            var retval = new List<ContentFile>();
            foreach (ContentFile file in rootDirectory.Files)
            {
                if (file.Extension == ".exe")
                {
                    retval.Add(file);
                }
            }
            if (includeSubDirectories)
            {
                foreach (ContentDirectory directory in rootDirectory.Directories)
                {
                    retval.AddRange(GetAllExe(directory));
                }
            }
            if (filterLauncherLoader)
            {
                var filteredRetval = new List<ContentFile>(retval);
                foreach (ContentFile file in retval)
                {
                    if (file.FileName.ToLowerInvariant().Contains("launcher_") ||
                        file.FileName.ToLowerInvariant().Contains("_launcher.exe") ||
                        file.FileName.ToLowerInvariant().Contains("loader.exe") ||
                        file.FileName.ToLowerInvariant().Contains("sselauncher.exe"))
                    {
                        filteredRetval.Remove(file);
                    }
                }
                retval = filteredRetval;
            }
            if (filterInstallExes)
            {
                var filteredRetval = new List<ContentFile>(retval);
                foreach (ContentFile file in retval)
                {
                    if (file.FileName.ToLowerInvariant().Contains("uninstall.exe") ||
                        file.FileName.ToLowerInvariant().Contains("setup.exe") ||
                        file.FileName.ToLowerInvariant().Contains("install.exe") ||
                        file.FileName.ToLowerInvariant().Contains("remove.exe"))
                    {
                        filteredRetval.Remove(file);
                    }
                }
                return filteredRetval;
            }
            return retval;
        }
        #endregion

        #region Private
        private FolderType AnalyzeRootFolderType(DirectoryInfo directoryInfo, int scanDepth = 1,bool lookForGames = true, bool lookForArchives = true)
        {
            FolderType retval = FolderType.Unknown;
            if (_directoryBlacklist.Contains(directoryInfo.FullName.ToLowerInvariant())) return retval;
            if (lookForGames)if(HasGames(directoryInfo, scanDepth)) retval |= FolderType.GameRoot;
            if (lookForArchives) if (HasArchives(directoryInfo, scanDepth))retval |= FolderType.CompressedArchive;
            return retval;
        }
        private bool HasGames(DirectoryInfo directoryInfo, int scanDepth)
        {
            var games = ScanForGames(directoryInfo, scanDepth);
            if (games.Any())
            {
                GameDirectories = games;
                OnFoundEvent(new FoundEventArgs() { FoundGames = games });
                return true;
            }
            return false;
        }
        private bool HasArchives(DirectoryInfo directoryInfo, int scanDepth)
        {
            var files = ScanForArchives(directoryInfo, scanDepth);
            if (files.Any())
            {
                Archives = new List<Archive.Archive>();
                foreach (FileInfo infoCompreddedFile in files)
                {

                    var archive = new Archive.Archive(_sevenZipExeFile, infoCompreddedFile);
                    Archives.Add(archive);
                    OnFoundEvent(new FoundEventArgs(){FoundArchive = archive});
                }
                return true;
            }
            return false;
        }
        private static bool IsGameFolder(DirectoryInfo directory, out GameInfo info)
        {
            info = GameInfo.ScanRoot(new GameRoot(directory));
            return info != null;
        }
        private static Dictionary<DirectoryInfo,GameInfo> ScanForGames(DirectoryInfo directoryInfo, int maxDepthLevel, int rootDepth = 0, HashSet<string> blackList = null)
        {
            int currentDepth = rootDepth;
            var returnValues = new Dictionary<DirectoryInfo, GameInfo>();
            if (!directoryInfo.Exists) return returnValues;

            GameInfo game;
            if (IsGameFolder(directoryInfo, out game))
            {
                returnValues.Add(directoryInfo,game);
                return returnValues;
            }
            if (currentDepth == maxDepthLevel) return returnValues;
            //For Folders
            currentDepth++;
            try
            {
                foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
                {
                    if (blackList != null && blackList.Contains(directoryInfo.FullName.ToLowerInvariant()))continue;
                    var value = ScanForGames(directory, maxDepthLevel, currentDepth);
                    if (value.Any())
                    {
                        returnValues.MergeWithoutDuplicates(value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return returnValues;
        }
        private static List<FileInfo> ScanForArchives(DirectoryInfo directoryInfo, int maxDepthLevel, int rootDepth = 0, HashSet<string> blackList = null)
        {
            int currentDepth = rootDepth;
            var returnValues = new List<FileInfo>();
            if (!directoryInfo.Exists) return returnValues;
            
            //For Files
            returnValues.AddRange(GetArchiveFiles(directoryInfo));
            if (currentDepth == maxDepthLevel) return returnValues;
            
            //For Folders
            currentDepth++;
            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                if (blackList != null && blackList.Contains(directoryInfo.FullName.ToLowerInvariant())) continue;
                returnValues.AddRange(ScanForArchives(directory,maxDepthLevel, currentDepth));
            }
            return returnValues;
        }
        /// <summary>
        /// Provides knowledge about multipart archive files
        /// </summary>
        /// <param name="files">A list with files</param>
        /// <returns>Only Archive files and only first part</returns>
        private static List<FileInfo> FilterMultiPartArchives(IEnumerable<FileInfo> files)
        {
            var dicMultiPartArchive = new Dictionary<string,Dictionary<int,FileInfo>>();
            var listNonMultiPart = new List<FileInfo>();
            var listRetval = new List<FileInfo>();
            foreach (FileInfo file in files)
            {
                int multiPartIndex;
                var splitParts = file.Name.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                //Multipart file
                // .part1.rar , .part2.rar
                if (splitParts[splitParts.Length - 2].ToLowerInvariant().Contains("part") &&
                    int.TryParse(splitParts[splitParts.Length - 2].ToLowerInvariant().Replace("part",""),out multiPartIndex))
                {
                    AddMultiPartFile(dicMultiPartArchive, file, multiPartIndex, splitParts, 2);
                }
                //  xxx.7z.001, xxx.7z.002
                else if (splitParts[splitParts.Length - 2].ToLowerInvariant().Contains("7z") &&
                         int.TryParse(splitParts[splitParts.Length - 1].ToLowerInvariant(),out multiPartIndex))
                {
                    AddMultiPartFile(dicMultiPartArchive, file, multiPartIndex, splitParts, 1);
                }
                //  xxx.zip.001, xxx.zip.002
                else if (splitParts[splitParts.Length - 2].ToLowerInvariant().Contains("zip") &&
                         int.TryParse(splitParts[splitParts.Length - 1].ToLowerInvariant(), out multiPartIndex))
                {
                    AddMultiPartFile(dicMultiPartArchive, file, multiPartIndex, splitParts, 1);
                }
                // .r01, .r02 ,.r03
                else if (splitParts[splitParts.Length - 1].ToLowerInvariant().StartsWith("r") &&
                         int.TryParse(splitParts[splitParts.Length - 1].ToLowerInvariant().Replace("r",""), out multiPartIndex)) 
                {
                    AddMultiPartFile(dicMultiPartArchive, file, multiPartIndex, splitParts, 1);
                }
                //Not a multipart
                else if (splitParts.Length > 1)
                {
                    listNonMultiPart.Add(file);
                }
            }
            
            //Evaluate Multi File Archives
            foreach (Dictionary<int, FileInfo> multiPartArchive in dicMultiPartArchive.Values)
            {
                bool archiveContinues = true;
                FileInfo firstArchiveFile = null;
                FileInfo lastFoundFile = null;

                for (int i = 1; i < multiPartArchive.Count+1; i++)
                {
                    FileInfo file = null;
                    if (multiPartArchive.TryGetValue(i, out file))
                    {
                        if (i == 1) firstArchiveFile = file;
                        lastFoundFile = file;
                        continue;
                    }
                    else
                    {
                        if (firstArchiveFile == null || firstArchiveFile.Length <= lastFoundFile.Length ||
                            multiPartArchive.TryGetValue(i + 1, out file))
                        {
                            archiveContinues = false;
                        }   
                    }
                }
                if (archiveContinues)
                {
                    listRetval.Add(firstArchiveFile);
                }
            }
            listRetval.AddRange(listNonMultiPart);
            return listRetval;
        }
        private static bool AddMultiPartFile(Dictionary<string, Dictionary<int, FileInfo>> dictionary, FileInfo file, int partIndex, string[] splitParts, int skipSplitParts)
        {
            var name = "";
            for (int i = 0; i < splitParts.Length - skipSplitParts; i++)
            {
                name = name + splitParts[i] + ".";
            }
            if (dictionary.ContainsKey(name))
            {
                if (dictionary[name].ContainsKey(partIndex))
                {
                    throw new NotSupportedException(
                        $"Found Files with duplicate multi archive index for same archive! {Environment.NewLine}" +
                        $"File 1: {dictionary[name][partIndex].FullName} {Environment.NewLine}" +
                        $"File 2: {file.FullName}");
                }
                dictionary[name].Add(partIndex, file);
            }
            else
            {
                dictionary.Add(name,new Dictionary<int, FileInfo>{{partIndex, file}});
            }
            return true;
        }
        private static List<FileInfo> GetArchiveFiles(DirectoryInfo directoryInfo)
        {
            //For Files
            int archiveNum;
            var unfiltredValues = new List<FileInfo>();
            var filter = Library.ArchivesFileExtension;
            foreach (var item in directoryInfo.EnumerateFiles())
            {
                bool added = false;
                foreach (var extension in filter)
                {
                    if (item.Extension.ToLowerInvariant() == extension)
                    {
                        unfiltredValues.Add(item);
                        added = true;
                    }
                }
                if(!added && int.TryParse(item.Name.ToLowerInvariant().Substring(item.Name.Length - 3), out archiveNum)) unfiltredValues.Add(item);
            }
            return FilterMultiPartArchives(unfiltredValues);
        }
        #endregion

        protected virtual void OnFoundEvent(FoundEventArgs e)
        {
            FoundEvent?.Invoke(this, e);
        }
    }

    [Flags]
    public enum FolderType
    {
        Unknown = 0,
        CompressedArchive = 1,
        GameRoot = 2,
    }


    public class FoundEventArgs
    {
        public Archive.Archive FoundArchive { get; set; }
        public Dictionary<DirectoryInfo,GameInfo> FoundGames { get; set; }
    }
}
