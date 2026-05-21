#region Licence
/****************************************************************
 *  Filename: ExtensionMethods.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using NLog;

namespace LeapVR.Shell.Modules
{
    public static class ExtensionMethods
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static string GetExecutionFileProcessName(this IDiskEntityDto entity)
        {
            try
            {
                var parts = entity.RelativePath.Split('\\');
                var exefileName = parts[parts.Length - 1];
                return exefileName.Remove(exefileName.Length - 4);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Could not get ProcessName from Filepath!");
                return null;
            }
        }
    }
}
