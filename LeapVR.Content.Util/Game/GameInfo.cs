#region Licence
/****************************************************************
 *  Filename: GameInfo.cs
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
using System.Collections.Generic;
using System.Linq;
using LeapVR.Content.Util.Archive;
using LeapVR.Content.Util.Enums;
using LeapVR.Content.Util.Util;

namespace LeapVR.Content.Util.Game
{
    public class GameInfo
    {
        #region Properties
        private GameRoot _root;
        public EngineType Engine { get; private set; }
        public List<ContentFile> GameExes { get; private set; }
        public GameRoot Root => _root;
        #endregion

        GameInfo( EngineType engine, GameRoot rootDirectory)
        {
            _root = rootDirectory;
            Engine = engine;
            GameExes = new List<ContentFile>();
        }

        public static GameInfo ScanRoot(GameRoot rootDirectory)
        {
            GameInfo retval;
            //Order is important!
            if ((retval = DetectUnityGame(rootDirectory)) != null) return retval;
            if ((retval = DetectCryEngine(rootDirectory)) != null) return retval;
            if ((retval = DetectUnrealGame(rootDirectory)) != null) return retval;
            if ((retval = DetectEsenthel(rootDirectory)) != null) return retval;
            if ((retval = DetectStandardDisplayLibrary(rootDirectory)) != null) return retval;
            //if ((retval = UnknownPackage(rootDirectory)) != null) return retval;
            return null;
        }

        public string GetIndex()
        {
            return Root.Type == ContentDirectory.RootType.Directory ? Root.SourceFileSystem.FullName : Root.SourceArchive.ArchiveFile.Name;
        }

        public ContentFile GetMainExe()
        {
            if(GameExes.Any()) return GameExes[0];
            return null;
        }

        #region Private Methods
        #region Detect Engines
        private static GameInfo DetectUnityGame(GameRoot rootDirectory)
        {
            GameInfo retval = null;
            foreach (ContentDirectory info in rootDirectory.Directories)
            {
                //Case Unity Game
                if (info.Name.ToLowerInvariant().Contains("_data"))
                {
                    bool hasManaged = false;
                    bool hasMono = false;
                    bool hasPlugins = false;
                    bool hasResources = false;
                    foreach (ContentDirectory subDirectory in info.Directories)
                    {
                        if (subDirectory.Name.ToLowerInvariant().Contains("managed")) hasManaged = true;
                        if (subDirectory.Name.ToLowerInvariant().Contains("mono")) hasMono = true;
                        if (subDirectory.Name.ToLowerInvariant().Contains("plugins")) hasPlugins = true;
                        if (subDirectory.Name.ToLowerInvariant().Contains("resources")) hasResources = true;
                    }
                    if (hasManaged && hasMono && hasPlugins && hasResources)
                    {
                        //Unity Game Detected
                        retval = new GameInfo(EngineType.Unity3D, rootDirectory);
                        //Detect the Exe of the Game
                        var exeName = info.Name.ToLowerInvariant().Replace("_data", "");
                        ContentFile exeFileInfo;
                        if (FindUnityGameExe(rootDirectory, exeName, out exeFileInfo))
                        {
                            retval.GameExes.Add(exeFileInfo);
                        }
                    }
                }
            }
            return retval;
        }
        private static GameInfo DetectCryEngine(GameRoot rootDirectory)
        {
            GameInfo retval = null;
            foreach (ContentDirectory info in rootDirectory.Directories)
            {
                if (info.Name.ToLowerInvariant().Equals("engine"))
                {
                    //Cry Engine
                    var binDirectory =
                        rootDirectory.Directories.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("bin"));
                    if (info.Files.FindAll(x => x.FileName.Contains("*.pak")).Any() && binDirectory != null)
                    {
                        retval = new GameInfo(EngineType.CryEngine, rootDirectory);
                        retval.GameExes.AddRange(DirectoryAnalyzer.GetAllExe(binDirectory));
                    }
                }
            }
            return retval;
        }
        private static GameInfo DetectUnrealGame(GameRoot rootDirectory)
        {
            GameInfo retval = null;
            bool hasEngineDir = false;
            bool hasBinaryDir = false;
            bool hasGameDir = false;
            ContentDirectory binaryDirectory = null;
            foreach (ContentDirectory info in rootDirectory.Directories)
            {
                if (info.Name.ToLowerInvariant().Equals("engine"))
                {
                    hasEngineDir = true;
                    retval = new GameInfo(EngineType.UnrealEngine4, rootDirectory);
                    //Unreal Games Exe is named in general like its Binaries directory
                    //We looking for a directory named like the exe
                    foreach (ContentFile file in rootDirectory.Files)
                    {
                        if (file.Extension.ToLowerInvariant() == ".exe")
                        {
                            foreach (ContentDirectory directoryInfo in rootDirectory.Directories)
                            {
                                if (directoryInfo.Name.ToLowerInvariant().Equals(file.FileName.ToLowerInvariant()
                                    .Substring(0, file.FileName.Length - 4)))
                                {
                                    //We found the root exe and the game source directory
                                    retval.GameExes.Add(file);
                                    //Unreal Games have also another Exe
                                    var binariesDir =
                                        directoryInfo.Directories.Find(
                                            x => x.Name.ToLowerInvariant() == "Binaries");
                                    ContentDirectory win64Dir;
                                    var binDirectory = rootDirectory.Directories.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("bin"));
                                    if (binariesDir != null && (win64Dir =
                                            binDirectory?.Directories.Find(
                                                x => x.Name.ToLowerInvariant() == "win64")) != null)
                                    {
                                        foreach (ContentFile archiveFile in win64Dir.Files)
                                        {
                                            if (archiveFile.Extension.ToLowerInvariant() != ".exe") continue;
                                            retval.GameExes.Add(archiveFile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //Unreal Games can also have no exe in their root and only one in their binaries directory
                    //E.g. Batman Arkham 
                    if (!retval.GameExes.Any())
                    {
                        //Unreal Games can also have their Binaries in other directory
                        foreach (ContentDirectory directory in rootDirectory.Directories)
                        {
                            if (directory.Name.ToLowerInvariant().Equals("engine")) continue;
                            var gamedirectories = directory.Directories
                                .Where(x => x.Name.ToLowerInvariant() == "binaries")
                                .ToList();

                            if (!gamedirectories.Any()) continue;
                            foreach (var gamedirectory in gamedirectories)
                            {
                                foreach (ContentFile file in gamedirectory.Files)
                                {
                                    if (file.Extension.ToLowerInvariant() == ".exe")
                                    {
                                        retval.GameExes.Add(file);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (ContentDirectory directory in rootDirectory.Directories)
                        {
                            if (directory.Name.ToLowerInvariant().Equals("engine")) continue;
                            ContentDirectory ue4Binaries;
                            if ((ue4Binaries = directory.Directories.FirstOrDefault(x => x.Name.ToLowerInvariant()
                                    .Equals("Binaries"))) != null)
                            {
                                ContentDirectory ue4WinExe;
                                if ((ue4WinExe = ue4Binaries.Directories.FirstOrDefault(x => x.Name
                                        .ToLowerInvariant()
                                        .Equals("win64"))) != null)
                                {
                                    foreach (ContentFile file in ue4WinExe.Files)
                                    {
                                        if (file.Extension.ToLowerInvariant() == ".exe")
                                        {
                                            retval.GameExes.Add(file);
                                        }
                                    }
                                }
                                if ((ue4WinExe = ue4Binaries.Directories.FirstOrDefault(x => x.Name
                                        .ToLowerInvariant()
                                        .Equals("win32"))) != null)
                                {
                                    foreach (ContentFile file in ue4WinExe.Files)
                                    {
                                        if (file.Extension.ToLowerInvariant() == ".exe")
                                        {
                                            retval.GameExes.Add(file);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //If we did not found any exe or other pattern it's a false positve
                if (info.Name.ToLowerInvariant().Equals("binaries"))
                {
                    hasBinaryDir = true;
                    binaryDirectory = info;
                }
                if (info.Name.ToLowerInvariant().Contains("game"))
                {
                    if ((info.Directories.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("cookedpc")) !=
                         null) &&
                        (info.Directories.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("config")) != null))
                    {
                        hasGameDir = true;
                    }

                }
            }
            //Could be a game as crookz and rocket league
            if (hasBinaryDir && hasGameDir && hasEngineDir)
            {
                ContentDirectory ue4WinExe;
                if ((ue4WinExe = binaryDirectory.Directories.FirstOrDefault(x => x.Name
                        .ToLowerInvariant()
                        .Equals("win64"))) != null)
                {
                    foreach (ContentFile file in ue4WinExe.Files)
                    {
                        if (file.Extension.ToLowerInvariant() == ".exe")
                        {
                            retval.GameExes.Add(file);
                        }
                    }
                }
                if ((ue4WinExe = binaryDirectory.Directories.FirstOrDefault(x => x.Name
                        .ToLowerInvariant()
                        .Equals("win32"))) != null)
                {
                    foreach (ContentFile file in ue4WinExe.Files)
                    {
                        if (file.Extension.ToLowerInvariant() == ".exe")
                        {
                            retval.GameExes.Add(file);
                        }
                    }
                }
                
            }
            if (retval != null && !retval.GameExes.Any()) retval = null;
            return retval;
        }
        private static GameInfo DetectEsenthel(GameRoot rootDirectory)
        {
            GameInfo retval = null;
            foreach (ContentDirectory info in rootDirectory.Directories)
            {
                if (info.Name.ToLowerInvariant().Equals("bin"))
                {
                    var binDirectory =
                        rootDirectory.Directories.FirstOrDefault(x => x.Name.ToLowerInvariant().Contains("bin"));
                    if (binDirectory?.Files.Find(x => x.Extension == ".pak") != null)
                    {
                        retval = new GameInfo(EngineType.Esenthel, rootDirectory);
                        retval.GameExes.AddRange(DirectoryAnalyzer.GetAllExe(rootDirectory, false));
                    }
                }
            }
            return retval;
        }
        private static GameInfo DetectStandardDisplayLibrary(GameRoot rootDirectory)
        {
            GameInfo retval = null;
            foreach (ContentFile file in rootDirectory.Files)
            {
                if (file.FileName.ToLowerInvariant() == "sdl2.dll" ||
                    file.FileName.ToLowerInvariant() == "sdl.dll")
                {
                    retval = new GameInfo(EngineType.SDL, rootDirectory);
                    retval.GameExes.AddRange(DirectoryAnalyzer.GetAllExe(rootDirectory, false));
                }
            }
            return retval;
        }

        /// <summary>
        /// Very rare case of one app at this time 2017-06-25 known
        /// Creates many false positives
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <returns>Info about the game</returns>
        private static GameInfo DetectOneExeApp(GameRoot rootDirectory)
        {
            GameInfo retval = null;
            if (rootDirectory.Directories.Count == 0 && rootDirectory.Files.Count == 1)
            {
                if (rootDirectory.Files[0].Extension.ToLowerInvariant() == ".exe")
                {
                    retval = new GameInfo(EngineType.Custom, rootDirectory);
                    retval.GameExes.Add(rootDirectory.Files[0]);
                }
            }
            return retval;
        }
        private static bool FindUnityGameExe(ContentDirectory info, string gameName, out ContentFile exeFile)
        {
            foreach (ContentFile fileInfo in info.Files)
            {
                var fileName = fileInfo.FileName.ToLowerInvariant().Substring(0, fileInfo.FileName.Length - 4);
                if (fileName.Equals(gameName.ToLowerInvariant()))
                {
                    exeFile = fileInfo;
                    return true;
                }
            }
            exeFile = null;
            return false;
        }
        
        /// <summary>
        /// A package of other type
        /// </summary>
        /// <param name="rootDirectory"></param>
        /// <returns>Info about the game</returns>
        private static GameInfo UnknownPackage(GameRoot rootDirectory)
        {
            GameInfo retval = null;
            if (rootDirectory.Files.Any() || rootDirectory.Directories.Any()) return new GameInfo(EngineType.Unknown, rootDirectory);
            int maxdepth = 50;
            if (CheckForFile(rootDirectory, maxdepth)) return new GameInfo(EngineType.Unknown, rootDirectory);
            return retval;
        }
        private static bool CheckForFile(ContentDirectory directoryInfo, int maxDepthLevel, int rootDepth = 0)
        {
            int currentDepth = rootDepth;
            if (directoryInfo.Files.Any()) return true;
            currentDepth++;
            foreach (var directory in directoryInfo.Directories)
            {
                if (CheckForFile(directory, maxDepthLevel, currentDepth)) return true ;
            }
            return false;
        }
        #endregion

        #endregion
    }
}
