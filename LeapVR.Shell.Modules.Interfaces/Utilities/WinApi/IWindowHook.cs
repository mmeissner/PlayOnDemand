#region Licence
/****************************************************************
 *  Filename: IWindowHook.cs
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
using System.Collections.Generic;

namespace LeapVR.Shell.Modules.Interfaces.Utilities.WinApi
{
    public interface IWindowHook : IDisposable
    {
        bool IsHideWindowsEnabled { get; }
        bool IsMoveWindowsEnabled { get; }
        bool IsFocusWindowEnabled { get; }
        bool IsDetectWindowChangesEnabled { get; }

        void EnableHideWindows(IEnumerable<IWindowFilter> windowFilters, bool isWhitelist);
        void EnableMoveWindows(int posX, int posY, IntPtr moveBelowWindow, IEnumerable<IWindowFilter> windowFilters, bool isWhitelist);
        void EnableFocusWindow(IWindowFilter windowFilter);
        void EnableDetectWindowChanges(IEnumerable<IWindowFilter> windowFilters, bool isWhitelist);

        void DisableHideWindows();
        void DisableMoveWindows();
        void DisableFocusWindow();
        void DisableDetectWindowChanges();

        IObservable<IWindowData> WhenWindowDataChanged { get; }
        IWindowData GetWindowData(IWindowFilter windowFilters);

        void HideWindows(IEnumerable<IWindowFilter> windowFilters, bool isWhitelist);
        void ShowWindows(IEnumerable<IWindowFilter> windowFilters, bool isWhitelist);
        void FocusWindow(IWindowFilter windowFilter);
        bool IsWindowFocused();
    }
}
