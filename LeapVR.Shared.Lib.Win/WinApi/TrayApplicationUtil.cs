#region Licence
/****************************************************************
 *  Filename: TrayApplicationUtil.cs
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
using LeapVR.Shared.Lib.Win.WinApi.Win32;

namespace LeapVR.Shared.Lib.Win.WinApi
{
    public class TrayApplicationUtil
    {
        #region Private Fields
        private Process _process = null;
        #endregion

        #region Private Properties
        private User32.TrayIconInfo TrayIconInfo { get; set; }
        #endregion

        #region Public Properties
        public IntPtr HWnd { get; private set; }
        public IntPtr TrayIconHwnd { get; private set; }
        public User32.ToolBarButtonInfo TrayButtonInfo { get; set; }
        public User32.ToolBarButtonData TrayButtonData { get; set; }
        public User32.Rectangle GetTrayPosition
        {
            get
            {
                //Try to get the Icon Position in the Tray
                Shell32.Notifyiconidentifier icon;
                var rectangle = new User32.Rectangle(0, 0, 0, 0);
                if (Shell32.CanGetNotifyIconIdentifier(TrayButtonData, out icon))
                {
                    Shell32.Shell_NotifyIconGetRect(ref icon, out rectangle);
                }
                return rectangle;
            }
        }

        public string Title { get; private set; }
        public Process Process
        {
            get
            {
                if (this._process == null)
                {
                    try
                    {
                        this._process = Process.GetProcessById(this.ProcessId);
                    }
                    catch { }
                }
                return this._process;
            }
        }
        public int ProcessId { get; private set; }
        public string ProcessName { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a Window Object
        /// </summary>
        /// <param name="title">Title Caption</param>
        /// <param name="handleWindow">Handle</param>
        /// <param name="processId">Owning Process</param>
        /// <param name="processName">Name of the process</param>
        public TrayApplicationUtil(string title, IntPtr handleWindow, int processId, string processName)
        {
            Title = title ?? string.Empty;
            HWnd = handleWindow;
            ProcessId = processId;
            ProcessName = processName;
        }
        public TrayApplicationUtil(int processId, string processName, User32.TrayIconInfo notificationArea, User32.ToolBarButtonData traydata)
            : this(notificationArea.ToolTip, notificationArea.MainWindowHandle, processId, processName)
        {
            TrayButtonInfo = notificationArea.ToolBarButtonInfo;
            TrayIconHwnd = notificationArea.TrayIconHandle;
            TrayIconInfo = notificationArea;
            TrayButtonData = traydata;
        }
        #endregion

        #region Public Methods
        public static IntPtr MakeLParam(int loWord, int hiWord)
        {
            return new IntPtr((hiWord << 16) | (loWord & 0xffff));
        }

        public void HideTrayIcon(bool hide)
        {
            User32.SendMessage(User32.GetNotificationToolbarWindowHandle(), User32.ToolBar.Hidebutton, TrayButtonInfo.idCommand, hide);
        }

        public void DeleteTrayIcon()
        {
            User32.SendMessage(User32.GetNotificationToolbarWindowHandle(), User32.ToolBar.Deletebutton, TrayButtonInfo.idCommand, IntPtr.Zero);
        }

        /// <summary>
        /// Works only for Windows Forms Application
        /// Doesn't work for Qt Applications
        /// </summary>
        public void SendRightClickEvent()
        {
            User32.RightClickEvent(TrayButtonData);
        }

        public void StartNewInstance()
        {
            var p = new Process
            {
                StartInfo =
                {
                    RedirectStandardOutput = false,
                    FileName = this.ProcessName,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };
            p.Start();
        }

        public override string ToString()
        {
            return this.Title ?? string.Empty;
        }
        #endregion
    }
}
