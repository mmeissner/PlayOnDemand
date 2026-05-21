#region Licence
/****************************************************************
 *  Filename: DirectoryCleaner.cs
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
using NLog;

namespace LeapVR.Content.Util.Util
{
    public static class DirectoryCleaner
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        //TODO Clean also logfiles that might contain sensitive information
        public static bool Clean(DirectoryInfo info)
        {
            bool retval = true;
            foreach (string exclusion in Library.FileExclusions)
            {
                foreach (var file in info.GetFiles($"*{exclusion}"))
                {
                    try
                    {
                        if(file.Exists)file.Delete();
                    }
                    catch
                    {
                        retval = false;
                    }
                }
            }
            return retval;
        }
    }
}
