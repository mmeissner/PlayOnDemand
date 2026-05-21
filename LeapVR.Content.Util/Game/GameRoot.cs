#region Licence
/****************************************************************
 *  Filename: GameRoot.cs
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
using System.IO;
using LeapVR.Content.Util.Archive;

namespace LeapVR.Content.Util.Game
{
    public class GameRoot : ContentDirectory
    {
        public GameRoot(DirectoryInfo directoryInfo): base(directoryInfo,directoryInfo)
        { }
        public GameRoot(ContentDirectory contentRootDirectory) : base(contentRootDirectory)
        { }

        public bool IsGameRoot(bool ignoreUrlFiles = true, bool ignoreTxtFiles= true, bool ignoreLaunchers = true)
        {
            //return Directories.Count >= 1 && Files.Count == 0;
            if (Directories.Count < 1) return true;
            if (Files.Count == 0) return false;
            if (!ignoreTxtFiles && !ignoreUrlFiles && !ignoreLaunchers) return true;

            int totalFiles = Files.Count;
            if (ignoreTxtFiles)
            {
                var txtFiles = Files.FindAll(x => x.Extension.ToLowerInvariant() == ".txt");
                totalFiles = totalFiles - txtFiles.Count;
            }
            if (ignoreUrlFiles)
            {
                var urlFiles = Files.FindAll(x => x.Extension.ToLowerInvariant() == ".url");
                totalFiles = totalFiles - urlFiles.Count;
            }
            if (ignoreLaunchers)
            {
                var laucherFiles = Files.FindAll(x => x.FileName.ToLowerInvariant().Contains("launcher_") ||
                                                      x.FileName.ToLowerInvariant().Contains("_launcher.exe") ||
                                                      x.FileName.ToLowerInvariant().Contains("loader.exe") ||
                                                      x.FileName.ToLowerInvariant().Contains("sselauncher.exe") ||
                                                      x.FileName.ToLowerInvariant().Contains("smartsteamemu"));
                totalFiles = totalFiles - laucherFiles.Count;
            }
            if (totalFiles > 0) return true;
            return false;
        }
    }
}