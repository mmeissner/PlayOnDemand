#region Licence
/****************************************************************
 *  Filename: ArchiveReport.cs
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
using LeapVR.Content.Util.Game;

namespace LeapVR.Content.Util.Archive
{
    public class ArchiveReport
    {
        #region Properties
        private readonly Archive _archive;
        private GameInfo _gameInfo;
        private uint _rootLevelDepth = 0;
        private ContentDirectory _archiveRoot = null;
        public Archive Archive => _archive;
        public GameInfo GameInfo => _gameInfo;
        public uint GameRootDirectoryDepth => _rootLevelDepth;
        public ContentDirectory ArchiveRoot => _archiveRoot;
        #endregion

        #region Constructor
        ArchiveReport(Archive archive)
        {
            _archive = archive;
        }
        #endregion

        public static ArchiveReport Analyze(Archive archive)
        {
            var retval = new ArchiveReport(archive);
            if (!archive.HasError && archive.Contents.Count > 0)
            {
                retval._archiveRoot = new ContentDirectory(archive);
                retval._gameInfo = DetectGame(retval);
            }

            return retval;
        }

        private static GameInfo DetectGame(ArchiveReport report)
        {
            GameRoot gameRoot = new GameRoot(report.ArchiveRoot);
            if (gameRoot.IsGameRoot())
            {
                var result = GameInfo.ScanRoot(new GameRoot(gameRoot));
                if (result != null) return result;
            }
            int maxDepth = 10;
            while (!gameRoot.IsGameRoot() && report._rootLevelDepth <= maxDepth)
            {
                report._rootLevelDepth++;
                foreach (ContentDirectory directory in gameRoot.Directories)
                {

                    gameRoot = new GameRoot(directory);
                    var game = GameInfo.ScanRoot(gameRoot);
                    if (game != null) return game;
                }
            }
            return GameInfo.ScanRoot(new GameRoot(gameRoot));
        }
    }
}