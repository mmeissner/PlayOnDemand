#region Licence
/****************************************************************
 *  Filename: VersionProvider.cs
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
using System.Reflection;
using NLog;

namespace LeapVR.Shared.Lib.Win
{
    public static class VersionProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static VersionProvider()
        {
            SoftwareVersion = Assembly.GetEntryAssembly().GetName().Version;
            Logger.Info($"Setting SoftwareVersion to {SoftwareVersion}");
        }
        public static System.Version SoftwareVersion { get; }

    }
}
