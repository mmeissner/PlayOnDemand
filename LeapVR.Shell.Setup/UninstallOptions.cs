#region Licence
/****************************************************************
 *  Filename: UninstallOptions.cs
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

namespace LeapVR.Shell.Setup {
    public class UninstallOptions
    {
        private const string DeleteGamesArgument = "-deletegames=";
        private bool _deleteGames;
        public UninstallOptions()
        {
            ParseOptions();
        }
        public bool DeleteGames => _deleteGames;
        public bool RemoveStartupTask { get; } = true;
        public bool EnableWer { get; } = true;
        public bool DeleteCustomConfig { get; } = true;
        public bool RemoveWindowsDefenderExclusion { get; private set; }

        private void ParseOptions()
        {
            var commandLine = Environment.GetCommandLineArgs();
            foreach(string args in commandLine)
            {
                var param = args.ToLowerInvariant();
                if(param.Contains(DeleteGamesArgument))
                {
                    Boolean.TryParse(param.Replace(DeleteGamesArgument, ""), out _deleteGames);
                    if (_deleteGames) RemoveWindowsDefenderExclusion = true;
                }
            }
        }
    }
}