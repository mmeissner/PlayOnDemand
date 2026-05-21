#region Licence
/****************************************************************
 *  Filename: TaskbarHelper.cs
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Logger = NLog.Logger;

namespace LeapVR.Shared.Lib.Win
{
    /// <summary>
    /// Helper class for hiding/showing the taskbar and startmenu on
    /// Windows XP and Vista.
    /// </summary>
    public static class Taskbar
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumThreadWindows(int threadId, EnumThreadProc pfnEnum, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern System.IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out int lpdwProcessId);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private const string VistaStartMenuCaption = "Start";
        private static IntPtr vistaStartMenuWnd = IntPtr.Zero;
        private delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);

        /// <summary>
        /// Show the taskbar.
        /// </summary>
        public static void Show()
        {
            SetVisibility(true);
        }

        /// <summary>
        /// Hide the taskbar.
        /// </summary>
        public static void Hide()
        {
            SetVisibility(false);
        }

        /// <summary>
        /// Sets the visibility of the taskbar.
        /// </summary>
        public static bool Visible
        {
            set { SetVisibility(value); }
        }

        /// <summary>
        /// Hide or show the Windows taskbar and startmenu.
        /// </summary>
        /// <param name="show">true to show, false to hide</param>
        private static void SetVisibility(bool show)
        {
            // get taskbar window
            Logger.Debug("Try to get the taskBarWnd by FindWindow('Shell_TrayWnd')");
            IntPtr taskBarWnd = FindWindow("Shell_TrayWnd", null);
            Logger.Debug( $"Current [taskBarWind] is : {taskBarWnd.ToInt64()}, time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            // try it the WinXP way first...
            Logger.Debug("Try to get startWnd with WinXP way");
            IntPtr startWnd = FindWindowEx(taskBarWnd, IntPtr.Zero, "Button", "Start");
            if (startWnd == IntPtr.Zero)
            {
                Logger.Debug("StartWnd not found, try with vista way.");
                // ok, let's try the Vista easy way...
                startWnd = FindWindow("Button", null);
                Logger.Debug($"Current [startWnd] is : {startWnd.ToInt64()}, time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                if (startWnd == IntPtr.Zero)
                {
                    Logger.Debug("StartWnd not found, try harder.");
                    // no chance, we need to to it the hard way...
                    startWnd = GetVistaStartMenuWnd(taskBarWnd);
                    Logger.Debug($"Current [startWnd] is : {startWnd.ToInt64()}, time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }
            }
            Logger.Debug("Ggetting window ended.");
            Logger.Debug($"Current [taskBarWnd] is :{taskBarWnd.ToInt64()}, time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Logger.Debug($"Current [startWnd] is : {startWnd.ToInt64()}, time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            ShowWindow(taskBarWnd, show ? SW_SHOW : SW_HIDE);
            ShowWindow(startWnd, show ? SW_SHOW : SW_HIDE);
        }

        /// <summary>
        /// Returns the window handle of the Vista start menu orb.
        /// </summary>
        /// <param name="taskBarWnd">windo handle of taskbar</param>
        /// <returns>window handle of start menu</returns>
        private static IntPtr GetVistaStartMenuWnd(IntPtr taskBarWnd)
        {
            // get process that owns the taskbar window
            GetWindowThreadProcessId(taskBarWnd, out var procId);

            Process p = Process.GetProcessById(procId);
            if (p != null)
            {
                // enumerate all threads of that process...
                foreach (ProcessThread t in p.Threads)
                {
                    EnumThreadWindows(t.Id, MyEnumThreadWindowsProc, IntPtr.Zero);
                }
            }
            return vistaStartMenuWnd;
        }

        /// <summary>
        /// Callback method that is called from 'EnumThreadWindows' in 'GetVistaStartMenuWnd'.
        /// </summary>
        /// <param name="hWnd">window handle</param>
        /// <param name="lParam">parameter</param>
        /// <returns>true to continue enumeration, false to stop it</returns>
        private static bool MyEnumThreadWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            StringBuilder buffer = new StringBuilder(256);
            if (GetWindowText(hWnd, buffer, buffer.Capacity) > 0)
            {
                Console.WriteLine(buffer);
                if (buffer.ToString() == VistaStartMenuCaption)
                {
                    vistaStartMenuWnd = hWnd;
                    return false;
                }
            }
            return true;
        }
    }
}