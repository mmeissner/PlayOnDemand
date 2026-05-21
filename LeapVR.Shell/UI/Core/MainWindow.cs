#region Licence
/****************************************************************
 *  Filename: MainWindow.cs
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

namespace LeapVR.Shell.UI.Core {
    internal class MainWindow
    {
        internal Window WindowBase { get; }
        public MainWindow(Window mainWindow)
        {
            WindowBase = mainWindow;
            MainWindowHeight = mainWindow.ActualHeight;
            MainWindowWidth = mainWindow.ActualWidth;
            MainWindowPosX = mainWindow.Left;
            MainWindowPosY = mainWindow.Top;
        }
        public double MainWindowHeight { get; }
        public double MainWindowWidth { get; }
        public double MainWindowPosX { get; }
        public double MainWindowPosY { get; }
    }
}