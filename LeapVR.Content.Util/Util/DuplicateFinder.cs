#region Licence
/****************************************************************
 *  Filename: DuplicateFinder.cs
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
using System.Linq;
using LeapVR.Content.Util.Archive;
using LeapVR.Content.Util.Game;
using NLog;

namespace LeapVR.Content.Util.Util
{
    public static class DuplicateFinder
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Finds and sorts duplicates based on exe filenames in GameInfos
        /// </summary>
        /// <param name="gameInfos">Input to search in and sort from</param>
        /// <returns>A List of Games and their related game infos</returns>
        public static List<GameFileCollection> Analyze(List<GameInfo> gameInfos)
        {
            var groupedGamesByExecutables = new Dictionary<string, List<GameInfo>>();
            List<GameFileCollection> retval = null;
            foreach (GameInfo info in gameInfos)
            {
                if(info == null) continue;
                foreach (ContentFile file in info.GameExes)
                {
                    if (groupedGamesByExecutables.ContainsKey(file.FileName.ToLowerInvariant()))
                    {
                        groupedGamesByExecutables[file.FileName.ToLowerInvariant()].Add(info);
                    }
                    else
                    {
                        groupedGamesByExecutables.Add(file.FileName.ToLowerInvariant(),new List<GameInfo> { info });
                    }
                }
            }
            if (groupedGamesByExecutables.Any())
            {
                retval = new List<GameFileCollection>();
                foreach (List<GameInfo> list in groupedGamesByExecutables.Values)
                {
                    GameFileCollection searchedGameFileCollection = null;
                    //All entries should be one Game but, there can be multiple entries for the same game
                    //Check first for all entries if we dont have already a GameFileCollection
                    foreach (GameInfo gameInfo in list)
                    {
                        searchedGameFileCollection = retval.FirstOrDefault(x => x.GameInfos.Any(y => y.Equals(gameInfo)));
                        if(searchedGameFileCollection != null)break;
                    }
                    bool addnew = false;
                    if (searchedGameFileCollection == null)
                    {
                        searchedGameFileCollection = new GameFileCollection();
                        addnew = true;
                    }
                    foreach (GameInfo info in list)
                    {
                        searchedGameFileCollection.AddGameInfo(info);
                    }
                    if(addnew)retval.Add(searchedGameFileCollection);
                }
            }
            return retval;
        }

        public static GameInfo GetNewestRelease(List<GameInfo> gameInfos)
        {
            var latestDateTime = DateTime.MinValue;
            GameInfo retval = null; 
            foreach (GameInfo gameInfo in gameInfos)
            {
                foreach (var gameEx in gameInfo.GameExes)
                {
                    if (gameEx.Modified > latestDateTime)
                    {
                        latestDateTime = gameEx.Modified;
                        retval = gameInfo;
                        continue;
                    }
                    if (gameEx.Modified != latestDateTime) continue;
                    if (gameInfo.Root.SourceArchive.ArchiveFile == null ||
                        retval?.Root.SourceArchive.ArchiveFile == null) continue;

                    if (gameInfo.Root.SourceArchive.ArchiveFile.CreationTimeUtc >
                        retval.Root.SourceArchive.ArchiveFile.CreationTimeUtc)retval = gameInfo;
                }
            }
            return retval;
        }

        public static List<GameInfo> AllToNewestRelease(List<GameFileCollection> gameFileCollections)
        {
            var retval = new List<GameInfo>();
            foreach (GameFileCollection game in gameFileCollections)
            {
                if(game == null)continue;
                var newest = GetNewestRelease(game.GameInfos.ToList());
                if(newest != null)retval.Add(newest);
            }
            return retval;
        }
    }

    public class GameFileCollection
    {
        private List<GameInfo> _gameInfos = new List<GameInfo>();
        private List<string> _executables = new List<string>();
        public GameFileCollection(GameInfo info)
        {
            _gameInfos.Add(info);
            _executables.AddRange(info.GameExes.ConvertAll(x=> x.FullPath));
        }
        public GameFileCollection()
        {
        }

        public IReadOnlyCollection<GameInfo> GameInfos => _gameInfos;
        public IReadOnlyCollection<string> Executables => _executables;

        public void AddGameInfo(GameInfo info)
        {
            if(_gameInfos.Contains(info))return;
            _gameInfos.Add(info);
            foreach (ContentFile file in info.GameExes)
            {
                if(_executables.Contains(file.FullPath))continue;
                _executables.Add(file.FullPath);
            }
        }
    }
}
