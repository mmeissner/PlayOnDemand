#region Licence
/****************************************************************
 *  Filename: CleanScreenHelper.cs
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
using System.Windows;
using LeapVR.Shared.Lib.Win.WinApi;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using NLog;

namespace LeapVR.Utilities.Windows
{
    public static class CleanScreenHelper
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static void MoveWindowsCursorToSafePosition()
        {
            var x = 0;
            var y = (int)SystemParameters.PrimaryScreenHeight;
            User32.SetCursorPos(x, y);
            Logger.Debug($"Cursor moved to ({x}, {y}).");
        }
    }
}
