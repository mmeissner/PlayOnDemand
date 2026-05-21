#region Licence
/****************************************************************
 *  Filename: User32.cs
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using LeapVR.Shared.Lib.Win.VirtualKeyboard.Native;
using NLog;

namespace LeapVR.Shared.Lib.Win.WinApi.Win32
{
    public delegate void WinEventDelegate(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime);

    public delegate bool MonitorEnumDelegate(
            IntPtr hMonitor,
            IntPtr hdcMonitor,
            ref User32.Rect lprcMonitor,
            IntPtr dwData);

    public class User32
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        #region User32 Methods
        [DllImport("USER32.DLL")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        #region Clipboard Functions
        /// <summary>
        /// The WM_DRAWCLIPBOARD message notifies a clipboard viewer window that 
        /// the content of the clipboard has changed. 
        /// </summary>
        public const int WmDrawclipboard = 0x0308;

        /// <summary>
        /// A clipboard viewer window receives the WM_CHANGECBCHAIN message when 
        /// another window is removing itself from the clipboard viewer chain.
        /// </summary>
        public const int WmChangecbchain = 0x030D;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        #endregion

        /// <summary>
        /// filter function
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDlgItem(IntPtr hWnd, int nIDDlgItem);

        /// <summary>
        /// check if windows visible
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// Disables UserInputs as Mouse and Keyboard for this Window.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="bEnable">if set to <c>true</c> [b enable].</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);


        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        /// <summary>
        /// return windows text
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpWindowText"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        [DllImport(
                "user32.dll",
                EntryPoint = "GetWindowText",
                ExactSpelling = false,
                CharSet = CharSet.Auto,
                SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        /// <summary>
        /// Blocks the User Input like Mouse and Keyboard
        /// </summary>
        /// <param name="fBlockIt"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "BlockInput")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BlockInput([MarshalAs(UnmanagedType.Bool)] bool fBlockIt);

        /// <summary>
        /// Enables or Disables Windows Aero
        /// </summary>
        /// <param name="fEnable"></param>
        /// <returns></returns>
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern int DwmEnableComposition(bool fEnable);

        /// <summary>
        /// Returns if Windows Aero is enabled
        /// </summary>
        /// <returns></returns>
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref Rectangle lpPoints, UInt32 cPoints);


        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern UInt32 SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, bool lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, ref ToolBarButtonInfo lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int SendMessage(
                IntPtr hwndControl,
                uint Msg,
                int wParam,
                StringBuilder strBuffer); // get text


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
                IntPtr hWnd,
                uint msg,
                UIntPtr wParam,
                IntPtr lParam,
                SendMessageTimeoutFlags fuFlags,
                uint uTimeout,
                out UIntPtr lpdwResult);
        [DllImport("User32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        [DllImport("User32.dll")]
        public static extern bool MessageBeep(MessageBox beepType);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern bool ShowCursor(bool showOrHide);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hWnd, out UInt32 lpdwProcessId);

        [DllImport("User32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(
                IntPtr hwndParent,
                IntPtr hwndChildAfter,
                string lpszClass,
                string lpszWindow);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, Gw uCmd);

        [DllImport("User32.dll")]
        public static extern Int32 GetWindowTextLength(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern Int32 GetWindowText(IntPtr hWnd, out StringBuilder lpString, Int32 nMaxCount);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 GetClassName(IntPtr hWnd, out StringBuilder lpClassName, Int32 nMaxCount);

        [DllImport(
                "user32.dll",
                CharSet = CharSet.Auto,
                CallingConvention = CallingConvention.StdCall,
                ExactSpelling = true,
                SetLastError = true)]
        public static extern void MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern UInt32 GetClassLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern uint SetClassLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("User32.dll")]
        public static extern bool EnumDisplayDevices(
                string lpDevice,
                int iDevNum,
                ref DisplayDevice lpDisplayDevice,
                int dwFlags);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(
                IntPtr hdc,
                IntPtr lprcClip,
                MonitorEnumDelegate lpfnEnum,
                IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hmon, ref MonitorInfo mi);

        /// <summary>
        /// enumarator on all desktop windows
        /// </summary>
        /// <param name="hDesktop"></param>
        /// <param name="lpEnumCallbackFunction"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport(
                "user32.dll",
                EntryPoint = "EnumDesktopWindows",
                ExactSpelling = false,
                CharSet = CharSet.Auto,
                SetLastError = true)]
        public static extern bool EnumDesktopWindows(
                IntPtr hDesktop,
                EnumDelegate lpEnumCallbackFunction,
                IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);


        /// <summary>
        ///     Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are ordered
        ///     according to their appearance on the screen. The topmost window receives the highest rank and is the first window
        ///     in the Z order.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx for more information.</para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window.</param>
        /// <param name="hWndInsertAfter">
        ///     C++ ( hWndInsertAfter [in, optional]. Type: HWND )<br />A handle to the window to precede the positioned window in
        ///     the Z order. This parameter must be a window handle or one of the following values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND placement</term><description>Window to precede placement</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>HWND_BOTTOM ((HWND)1)</term>
        ///         <description>
        ///         Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost
        ///         window, the window loses its topmost status and is placed at the bottom of all other windows.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_NOTOPMOST ((HWND)-2)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows (that is, behind all topmost windows). This
        ///         flag has no effect if the window is already a non-topmost window.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOP ((HWND)0)</term><description>Places the window at the top of the Z order.</description>
        ///     </item>
        ///     <item>
        ///         <term>HWND_TOPMOST ((HWND)-1)</term>
        ///         <description>
        ///         Places the window above all non-topmost windows. The window maintains its topmost position
        ///         even when it is deactivated.
        ///         </description>
        ///     </item>
        ///     </list>
        ///     <para>For more information about how this parameter is used, see the following Remarks section.</para>
        /// </param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />The new position of the left side of the window, in client coordinates.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br />The new position of the top of the window, in client coordinates.</param>
        /// <param name="cx">C++ ( cx [in]. Type: int )<br />The new width of the window, in pixels.</param>
        /// <param name="cy">C++ ( cy [in]. Type: int )<br />The new height of the window, in pixels.</param>
        /// <param name="uFlags">
        ///     C++ ( uFlags [in]. Type: UINT )<br />The window sizing and positioning flags. This parameter can be a combination
        ///     of the following values.
        ///     <list type="table">
        ///     <itemheader>
        ///         <term>HWND sizing and positioning flags</term>
        ///         <description>Where to place and size window. Can be a combination of any</description>
        ///     </itemheader>
        ///     <item>
        ///         <term>SWP_ASYNCWINDOWPOS (0x4000)</term>
        ///         <description>
        ///         If the calling thread and the thread that owns the window are attached to different input
        ///         queues, the system posts the request to the thread that owns the window. This prevents the calling
        ///         thread from blocking its execution while other threads process the request.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DEFERERASE (0x2000)</term>
        ///         <description>Prevents generation of the WM_SYNCPAINT message. </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_DRAWFRAME (0x0020)</term>
        ///         <description>Draws a frame (defined in the window's class description) around the window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_FRAMECHANGED (0x0020)</term>
        ///         <description>
        ///         Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message
        ///         to the window, even if the window's size is not being changed. If this flag is not specified,
        ///         WM_NCCALCSIZE is sent only when the window's size is being changed
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_HIDEWINDOW (0x0080)</term><description>Hides the window.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOACTIVATE (0x0010)</term>
        ///         <description>
        ///         Does not activate the window. If this flag is not set, the window is activated and moved to
        ///         the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
        ///         parameter).
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOCOPYBITS (0x0100)</term>
        ///         <description>
        ///         Discards the entire contents of the client area. If this flag is not specified, the valid
        ///         contents of the client area are saved and copied back into the client area after the window is sized or
        ///         repositioned.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOMOVE (0x0002)</term>
        ///         <description>Retains the current position (ignores X and Y parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOOWNERZORDER (0x0200)</term>
        ///         <description>Does not change the owner window's position in the Z order.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREDRAW (0x0008)</term>
        ///         <description>
        ///         Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies
        ///         to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///         parent window uncovered as a result of the window being moved. When this flag is set, the application
        ///         must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOREPOSITION (0x0200)</term><description>Same as the SWP_NOOWNERZORDER flag.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSENDCHANGING (0x0400)</term>
        ///         <description>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOSIZE (0x0001)</term>
        ///         <description>Retains the current size (ignores the cx and cy parameters).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_NOZORDER (0x0004)</term>
        ///         <description>Retains the current Z order (ignores the hWndInsertAfter parameter).</description>
        ///     </item>
        ///     <item>
        ///         <term>SWP_SHOWWINDOW (0x0040)</term><description>Displays the window.</description>
        ///     </item>
        ///     </list>
        /// </param>
        /// <returns><c>true</c> or nonzero if the function succeeds, <c>false</c> or zero otherwise or if function fails.</returns>
        /// <remarks>
        ///     <para>
        ///     As part of the Vista re-architecture, all services were moved off the interactive desktop into Session 0.
        ///     hwnd and window manager operations are only effective inside a session and cross-session attempts to manipulate
        ///     the hwnd will fail. For more information, see The Windows Vista Developer Story: Application Compatibility
        ///     Cookbook.
        ///     </para>
        ///     <para>
        ///     If you have changed certain window data using SetWindowLong, you must call SetWindowPos for the changes to
        ///     take effect. Use the following combination for uFlags: SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
        ///     SWP_FRAMECHANGED.
        ///     </para>
        ///     <para>
        ///     A window can be made a topmost window either by setting the hWndInsertAfter parameter to HWND_TOPMOST and
        ///     ensuring that the SWP_NOZORDER flag is not set, or by setting a window's position in the Z order so that it is
        ///     above any existing topmost windows. When a non-topmost window is made topmost, its owned windows are also made
        ///     topmost. Its owners, however, are not changed.
        ///     </para>
        ///     <para>
        ///     If neither the SWP_NOACTIVATE nor SWP_NOZORDER flag is specified (that is, when the application requests that
        ///     a window be simultaneously activated and its position in the Z order changed), the value specified in
        ///     hWndInsertAfter is used only in the following circumstances.
        ///     </para>
        ///     <list type="bullet">
        ///     <item>Neither the HWND_TOPMOST nor HWND_NOTOPMOST flag is specified in hWndInsertAfter. </item>
        ///     <item>The window identified by hWnd is not the active window. </item>
        ///     </list>
        ///     <para>
        ///     An application cannot activate an inactive window without also bringing it to the top of the Z order.
        ///     Applications can change an activated window's position in the Z order without restrictions, or it can activate
        ///     a window and then move it to the top of the topmost or non-topmost windows.
        ///     </para>
        ///     <para>
        ///     If a topmost window is repositioned to the bottom (HWND_BOTTOM) of the Z order or after any non-topmost
        ///     window, it is no longer topmost. When a topmost window is made non-topmost, its owners and its owned windows
        ///     are also made non-topmost windows.
        ///     </para>
        ///     <para>
        ///     A non-topmost window can own a topmost window, but the reverse cannot occur. Any window (for example, a
        ///     dialog box) owned by a topmost window is itself made a topmost window, to ensure that all owned windows stay
        ///     above their owner.
        ///     </para>
        ///     <para>
        ///     If an application is not in the foreground, and should be in the foreground, it must call the
        ///     SetForegroundWindow function.
        ///     </para>
        ///     <para>
        ///     To use SetWindowPos to bring a window to the top, the process that owns the window must have
        ///     SetForegroundWindow permission.
        ///     </para>
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int X,
                int Y,
                int cx,
                int cy,
                SetWindowPosFlags uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        public static extern bool UnregisterDeviceNotification(IntPtr handle);

        /// <summary>
        /// The GetAsyncKeyState function determines whether a key is up or down at the time the function is called, and whether the key was pressed after a previous call to GetAsyncKeyState. (See: http://msdn.microsoft.com/en-us/library/ms646293(VS.85).aspx)
        /// </summary>
        /// <param name="virtualKeyCode">Specifies one of 256 possible virtual-key codes. For more information, see Virtual Key Codes. Windows NT/2000/XP: You can use left- and right-distinguishing constants to specify certain keys. See the Remarks section for further information.</param>
        /// <returns>
        /// If the function succeeds, the return value specifies whether the key was pressed since the last call to GetAsyncKeyState, and whether the key is currently up or down. If the most significant bit is set, the key is down, and if the least significant bit is set, the key was pressed after the previous call to GetAsyncKeyState. However, you should not rely on this last behavior; for more information, see the Remarks. 
        /// 
        /// Windows NT/2000/XP: The return value is zero for the following cases: 
        /// - The current desktop is not the active desktop
        /// - The foreground thread belongs to another process and the desktop does not allow the hook or the journal record.
        /// 
        /// Windows 95/98/Me: The return value is the global asynchronous key state for each virtual key. The system does not check which thread has the keyboard focus.
        /// 
        /// Windows 95/98/Me: Windows 95 does not support the left- and right-distinguishing constants. If you call GetAsyncKeyState with these constants, the return value is zero. 
        /// </returns>
        /// <remarks>
        /// The GetAsyncKeyState function works with mouse buttons. However, it checks on the state of the physical mouse buttons, not on the logical mouse buttons that the physical buttons are mapped to. For example, the call GetAsyncKeyState(VK_LBUTTON) always returns the state of the left physical mouse button, regardless of whether it is mapped to the left or right logical mouse button. You can determine the system's current mapping of physical mouse buttons to logical mouse buttons by calling 
        /// Copy CodeGetSystemMetrics(SM_SWAPBUTTON) which returns TRUE if the mouse buttons have been swapped.
        /// 
        /// Although the least significant bit of the return value indicates whether the key has been pressed since the last query, due to the pre-emptive multitasking nature of Windows, another application can call GetAsyncKeyState and receive the "recently pressed" bit instead of your application. The behavior of the least significant bit of the return value is retained strictly for compatibility with 16-bit Windows applications (which are non-preemptive) and should not be relied upon.
        /// 
        /// You can use the virtual-key code constants VK_SHIFT, VK_CONTROL, and VK_MENU as values for the vKey parameter. This gives the state of the SHIFT, CTRL, or ALT keys without distinguishing between left and right. 
        /// 
        /// Windows NT/2000/XP: You can use the following virtual-key code constants as values for vKey to distinguish between the left and right instances of those keys. 
        /// 
        /// Code Meaning 
        /// VK_LSHIFT Left-shift key. 
        /// VK_RSHIFT Right-shift key. 
        /// VK_LCONTROL Left-control key. 
        /// VK_RCONTROL Right-control key. 
        /// VK_LMENU Left-menu key. 
        /// VK_RMENU Right-menu key. 
        /// 
        /// These left- and right-distinguishing constants are only available when you call the GetKeyboardState, SetKeyboardState, GetAsyncKeyState, GetKeyState, and MapVirtualKey functions. 
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern Int16 GetAsyncKeyState(UInt16 virtualKeyCode);

        /// <summary>
        /// The GetKeyState function retrieves the status of the specified virtual key. The status specifies whether the key is up, down, or toggled (on, off alternating each time the key is pressed). (See: http://msdn.microsoft.com/en-us/library/ms646301(VS.85).aspx)
        /// </summary>
        /// <param name="virtualKeyCode">
        /// Specifies a virtual key. If the desired virtual key is a letter or digit (A through Z, a through z, or 0 through 9), nVirtKey must be set to the ASCII value of that character. For other keys, it must be a virtual-key code. 
        /// If a non-English keyboard layout is used, virtual keys with values in the range ASCII A through Z and 0 through 9 are used to specify most of the character keys. For example, for the German keyboard layout, the virtual key of value ASCII O (0x4F) refers to the "o" key, whereas VK_OEM_1 refers to the "o with umlaut" key.
        /// </param>
        /// <returns>
        /// The return value specifies the status of the specified virtual key, as follows: 
        /// If the high-order bit is 1, the key is down; otherwise, it is up.
        /// If the low-order bit is 1, the key is toggled. A key, such as the CAPS LOCK key, is toggled if it is turned on. The key is off and untoggled if the low-order bit is 0. A toggle key's indicator light (if any) on the keyboard will be on when the key is toggled, and off when the key is untoggled.
        /// </returns>
        /// <remarks>
        /// The key status returned from this function changes as a thread reads key messages from its message queue. The status does not reflect the interrupt-level state associated with the hardware. Use the GetAsyncKeyState function to retrieve that information. 
        /// An application calls GetKeyState in response to a keyboard-input message. This function retrieves the state of the key when the input message was generated. 
        /// To retrieve state information for all the virtual keys, use the GetKeyboardState function. 
        /// An application can use the virtual-key code constants VK_SHIFT, VK_CONTROL, and VK_MENU as values for the nVirtKey parameter. This gives the status of the SHIFT, CTRL, or ALT keys without distinguishing between left and right. An application can also use the following virtual-key code constants as values for nVirtKey to distinguish between the left and right instances of those keys. 
        /// VK_LSHIFT
        /// VK_RSHIFT
        /// VK_LCONTROL
        /// VK_RCONTROL
        /// VK_LMENU
        /// VK_RMENU
        /// 
        /// These left- and right-distinguishing constants are available to an application only through the GetKeyboardState, SetKeyboardState, GetAsyncKeyState, GetKeyState, and MapVirtualKey functions. 
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern Int16 GetKeyState(UInt16 virtualKeyCode);

        /// <summary>
        /// The SendInput function synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        /// <param name="numberOfInputs">Number of structures in the Inputs array.</param>
        /// <param name="inputs">Pointer to an array of INPUT structures. Each structure represents an event to be inserted into the keyboard or mouse input stream.</param>
        /// <param name="sizeOfInputStructure">Specifies the size, in bytes, of an INPUT structure. If cbSize is not the size of an INPUT structure, the function fails.</param>
        /// <returns>The function returns the number of events that it successfully inserted into the keyboard or mouse input stream. If the function returns zero, the input was already blocked by another thread. To get extended error information, call GetLastError.Microsoft Windows Vista. This function fails when it is blocked by User Interface Privilege Isolation (UIPI). Note that neither GetLastError nor the return value will indicate the failure was caused by UIPI blocking.</returns>
        /// <remarks>
        /// Microsoft Windows Vista. This function is subject to UIPI. Applications are permitted to inject input only into applications that are at an equal or lesser integrity level.
        /// The SendInput function inserts the events in the INPUT structures serially into the keyboard or mouse input stream. These events are not interspersed with other keyboard or mouse input events inserted either by the user (with the keyboard or mouse) or by calls to keybd_event, mouse_event, or other calls to SendInput.
        /// This function does not reset the keyboard's current state. Any keys that are already pressed when the function is called might interfere with the events that this function generates. To avoid this problem, check the keyboard's state with the GetAsyncKeyState function and correct as necessary.
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern UInt32 SendInput(UInt32 numberOfInputs, Input[] inputs, Int32 sizeOfInputStructure);

        /// <summary>
        /// The GetMessageExtraInfo function retrieves the extra message information for the current thread. Extra message information is an application- or driver-defined value associated with the current thread's message queue. 
        /// </summary>
        /// <returns></returns>
        /// <remarks>To set a thread's extra message information, use the SetMessageExtraInfo function. </remarks>
        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
                uint wVirtKey,
                uint wScanCode,
                byte[] lpKeyState,
                [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
                StringBuilder pwszBuff,
                int cchBuff,
                uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, KeyboardKeyMapType uKeyboardKeyMapType);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
                uint eventMin,
                uint eventMax,
                IntPtr hmodWinEventProc,
                WinEventDelegate lpfnWinEventProc,
                uint idProcess,
                uint idThread,
                uint dwFlags);
        #endregion

        #region Methods
        #region Tray 
        public static IntPtr GetNotificationToolbarWindowHandle()
        {
            IntPtr hShell = User32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            IntPtr hTray = User32.FindWindowEx(hShell, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr hPager = User32.FindWindowEx(hTray, IntPtr.Zero, "SysPager", null);
            IntPtr hToolbar = User32.FindWindowEx(hPager, IntPtr.Zero, "ToolbarWindow32", null);
            return hToolbar;
        }

        public static int GetButtonCount(IntPtr hwnd)
        {
            return (int)SendMessage(hwnd, ToolBar.Buttoncount, IntPtr.Zero, IntPtr.Zero);
        }

        public static TrayApplicationUtil GetTrayAppByIndex(IntPtr systemTrayHwnd, int idCommand)
        {
            //Prepare Reference Values
            var tbButton = new ToolBarButtonInfo();
            var text = String.Empty;
            var ipWindowHandle = IntPtr.Zero;
            TrayIconInfo trayIconInfo;
            var tray = new ToolBarButtonData();
            //Try to get the ToolBar Button
            bool b = GetTbButton(
                    systemTrayHwnd,
                    idCommand,
                    ref tbButton,
                    ref text,
                    ref ipWindowHandle,
                    ref tray,
                    out trayIconInfo);
            if(!b) return null;

            uint processId;
            GetWindowThreadProcessId(trayIconInfo.MainWindowHandle, out processId);
            if(processId == 0) return null;
            var fileName = GetWindowModuleFileNameFromHandle(trayIconInfo.MainWindowHandle);
            if(fileName.ToLower().EndsWith("explorer.exe"))
                return null;
            var window = new TrayApplicationUtil((int)processId, fileName, trayIconInfo, tray);
            return window;
        }
        public static void RefreshTrayArea()
        {
            _logger.Debug("Trying to Clean up Oculus Icon from Tray Area");
            var hWnd = GetNotificationToolbarWindowHandle();
            Rectangle rect;
            GetClientRect(hWnd, out rect);
            for(var x = 0; x < rect.Right; x += 5)
            for(var y = 0; y < rect.Bottom; y += 5)
                SendMessage(hWnd, (uint)Wm.Mousemove, 0, (y << 16) + x);
        }

        public static unsafe bool GetTbButton(
                IntPtr hToolbar,
                int i,
                ref ToolBarButtonInfo toolBarBtn,
                ref string toolTip,
                ref IntPtr ipWindowHandle,
                ref ToolBarButtonData traydata,
                out TrayIconInfo trayIconInfo)
        {
            // Initialize the Out Value
            trayIconInfo = new TrayIconInfo();

            // Set Buffer Size
            const int bufferSize = 0x1000;

            // Create a ByteBuffer
            byte[] localBuffer = new byte[bufferSize];

            //Declare and get ProcessID + ProcessHandle
            uint processId;
            UInt32 threadId = GetWindowThreadProcessId(hToolbar, out processId);
            IntPtr hProcess = Kernel32.OpenProcess(ProcessRights.AllAccess, false, processId);
            if(hProcess == IntPtr.Zero)
            {
                Debug.Assert(false);
                return false;
            }

            IntPtr ipRemoteBuffer = Kernel32.VirtualAllocEx(
                    hProcess,
                    IntPtr.Zero,
                    new UIntPtr(bufferSize),
                    MemAllocationType.Commit,
                    MemoryProtection.PageReadwrite);
            if(ipRemoteBuffer == IntPtr.Zero)
            {
                Debug.Assert(false);
                return false;
            }

            // TBButton
            fixed (ToolBarButtonInfo* pTbButton = &toolBarBtn)
            {
                var ipTbButton = new IntPtr(pTbButton);
                var b = (int)SendMessage(hToolbar, ToolBar.Getbutton, (IntPtr)i, ipRemoteBuffer);
                if(b == 0)
                {
                    Debug.Assert(false);
                    return false;
                }

                var dwBytesRead = 0;
                var ipBytesRead = new IntPtr(&dwBytesRead);
                bool b2 = Kernel32.ReadProcessMemory(
                        hProcess,
                        ipRemoteBuffer,
                        ipTbButton,
                        new UIntPtr((uint)sizeof(ToolBarButtonInfo)),
                        ipBytesRead);
                if(!b2)
                {
                    Debug.Assert(false);
                    return false;
                }
            }

            // button text
            fixed (byte* pLocalBuffer = localBuffer)
            {
                var ipLocalBuffer = new IntPtr(pLocalBuffer);
                var chars = (int)SendMessage(
                        hToolbar,
                        ToolBar.Getbuttontextw,
                        (IntPtr)toolBarBtn.idCommand,
                        ipRemoteBuffer);
                if(chars == -1)
                {
                    Debug.Assert(false);
                    return false;
                }

                var dwBytesRead = 0;
                var ipBytesRead = new IntPtr(&dwBytesRead);
                bool b4 = Kernel32.ReadProcessMemory(
                        hProcess,
                        ipRemoteBuffer,
                        ipLocalBuffer,
                        new UIntPtr(bufferSize),
                        ipBytesRead);
                if(!b4)
                {
                    Debug.Assert(false);
                    return false;
                }

                toolTip = Marshal.PtrToStringUni(ipLocalBuffer, chars);
                if(toolTip == " ") toolTip = String.Empty;
            }

            // window handle
            fixed (byte* pLocalBuffer = localBuffer)
            {
                var ipLocalBuffer = new IntPtr(pLocalBuffer);
                var dwBytesRead = 0;
                var ipBytesRead = new IntPtr(&dwBytesRead);
                var ipRemoteData = toolBarBtn.dwData;
                var b4 = Kernel32.ReadProcessMemory(hProcess, ipRemoteData, ipLocalBuffer, new UIntPtr(4), ipBytesRead);
                if(!b4) return false;
                if(dwBytesRead != 4) return false;
                var iWindowHandle = BitConverter.ToInt32(localBuffer, 0);
                if(iWindowHandle == -1) Debug.Assert(false);
                ipWindowHandle = new IntPtr(iWindowHandle);
            }

            int num = 0;
            var lpNumberOfBytesRead = new IntPtr(&num);

            //Create a fixed TRAYDATA structure
            fixed (ToolBarButtonData* traydataRef = &traydata)
            {
                IntPtr ptr5 = new IntPtr(traydataRef);
                //Read the value back in to our TRAYDATA structure
                Kernel32.ReadProcessMemory(
                        hProcess,
                        new IntPtr((int)toolBarBtn.dwData),
                        ptr5,
                        new UIntPtr((uint)sizeof(ToolBarButtonData)),
                        out lpNumberOfBytesRead);
            }

            //Release Memory
            Kernel32.VirtualFreeEx(hProcess, ipRemoteBuffer, UIntPtr.Zero, MemAllocationType.Release);
            Kernel32.CloseHandle(hProcess);

            //Get Process Infos
            uint dwTrayProcessID;
            GetWindowThreadProcessId(hToolbar, out dwTrayProcessID);
            if(dwTrayProcessID <= 0)
            {
                return false;
            }

            IntPtr hTrayProc = Kernel32.OpenProcess(ProcessAllAccess, 0, dwTrayProcessID);
            if(hTrayProc == IntPtr.Zero)
            {
                return false;
            }

            IntPtr lpData = Kernel32.VirtualAllocEx(
                    hTrayProc,
                    IntPtr.Zero,
                    Marshal.SizeOf(toolBarBtn.GetType()),
                    MemCommit,
                    PageReadwrite);
            if(lpData == IntPtr.Zero)
            {
                Kernel32.CloseHandle(hTrayProc);
                return false;
            }

            //Release Memory
            Kernel32.VirtualFreeEx(hTrayProc, lpData, UIntPtr.Zero, MemRelease);
            Kernel32.CloseHandle(hTrayProc);

            //Create new TrayInfoIcon
            trayIconInfo = new TrayIconInfo()
                           {
                                   ToolBarButtonInfo = toolBarBtn,
                                   MainWindowHandle = ipWindowHandle,
                                   TrayIconHandle = hToolbar,
                                   ToolTip = toolTip,
                           };
            return true;
        }

        public static string GetWindowModuleFileNameFromHandle(IntPtr hWnd)
        {
            var r = string.Empty;
            uint processId = 0;
            const int nChars = 1024;
            StringBuilder filename = new StringBuilder(nChars);
            User32.GetWindowThreadProcessId(hWnd, out processId);
            IntPtr hProcess = Kernel32.OpenProcess(1040, 0, processId);
            Psapi.GetModuleFileNameEx(hProcess, IntPtr.Zero, filename, nChars);
            Kernel32.CloseHandle(hProcess);
            r = filename.ToString();
            if(r.StartsWith("?"))
                r = "C" + r.Substring(1, r.Length - 1);
            return r;
        }

        public static void RightClickEvent(ToolBarButtonData traydata)
        {
            PostMessage(traydata.hwnd, traydata.uCallbackMessage, traydata.uID, 0x204);
            PostMessage(traydata.hwnd, traydata.uCallbackMessage, traydata.uID, 0x205);
        }


        public static int MakeLParam(int loWord, int hiWord) { return ((hiWord << 16) | (loWord & 0xffff)); }
        #endregion

        public static bool GetWindowZOrder(IntPtr hwnd, out int zOrder)
        {
            var lowestHwnd = GetWindow(hwnd, Gw.Hwndlast);

            var z = 0;
            var hwndTmp = lowestHwnd;
            while(hwndTmp != IntPtr.Zero)
            {
                if(hwnd == hwndTmp)
                {
                    zOrder = z;
                    return true;
                }

                hwndTmp = GetWindow(hwndTmp, Gw.Hwndprev);
                z++;
            }

            zOrder = int.MinValue;
            return false;
        }
        #endregion

        #region ENUM & Structs & Classes
        const uint ProcessAllAccess = 0x1F0FFF;
        const uint MemCommit = 0x1000;
        const uint MemRelease = 0x8000;
        const uint PageReadwrite = 0x04;

        public enum KeyboardKeyMapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        #region WM_Constants
        /// <summary>
        /// Windows Messages
        /// Defined in winuser.h from Windows SDK v6.1
        /// Documentation pulled from MSDN.
        /// </summary>
        public enum Wm : uint
        {
            /// <summary>
            /// The WM_NULL message performs no operation. An application sends the WM_NULL message if it wants to post a message that the recipient window will ignore.
            /// </summary>
            Null = 0x0000,
            /// <summary>
            /// The WM_CREATE message is sent when an application requests that a window be created by calling the CreateWindowEx or CreateWindow function. (The message is sent before the function returns.) The window procedure of the new window receives this message after the window is created, but before the window becomes visible.
            /// </summary>
            Create = 0x0001,
            /// <summary>
            /// The WM_DESTROY message is sent when a window is being destroyed. It is sent to the window procedure of the window being destroyed after the window is removed from the screen.
            /// This message is sent first to the window being destroyed and then to the child windows (if any) as they are destroyed. During the processing of the message, it can be assumed that all child windows still exist.
            /// /// </summary>
            Destroy = 0x0002,
            /// <summary>
            /// The WM_MOVE message is sent after a window has been moved.
            /// </summary>
            Move = 0x0003,
            /// <summary>
            /// The WM_SIZE message is sent to a window after its size has changed.
            /// </summary>
            Size = 0x0005,
            /// <summary>
            /// The WM_ACTIVATE message is sent to both the window being activated and the window being deactivated. If the windows use the same input queue, the message is sent synchronously, first to the window procedure of the top-level window being deactivated, then to the window procedure of the top-level window being activated. If the windows use different input queues, the message is sent asynchronously, so the window is activated immediately.
            /// </summary>
            Activate = 0x0006,
            /// <summary>
            /// The WM_SETFOCUS message is sent to a window after it has gained the keyboard focus.
            /// </summary>
            Setfocus = 0x0007,
            /// <summary>
            /// The WM_KILLFOCUS message is sent to a window immediately before it loses the keyboard focus.
            /// </summary>
            Killfocus = 0x0008,
            /// <summary>
            /// The WM_ENABLE message is sent when an application changes the enabled state of a window. It is sent to the window whose enabled state is changing. This message is sent before the EnableWindow function returns, but after the enabled state (WS_DISABLED style bit) of the window has changed.
            /// </summary>
            Enable = 0x000A,
            /// <summary>
            /// An application sends the WM_SETREDRAW message to a window to allow changes in that window to be redrawn or to prevent changes in that window from being redrawn.
            /// </summary>
            Setredraw = 0x000B,
            /// <summary>
            /// An application sends a WM_SETTEXT message to set the text of a window.
            /// </summary>
            Settext = 0x000C,
            /// <summary>
            /// An application sends a WM_GETTEXT message to copy the text that corresponds to a window into a buffer provided by the caller.
            /// </summary>
            Gettext = 0x000D,
            /// <summary>
            /// An application sends a WM_GETTEXTLENGTH message to determine the length, in characters, of the text associated with a window.
            /// </summary>
            Gettextlength = 0x000E,
            /// <summary>
            /// The WM_PAINT message is sent when the system or another application makes a request to paint a portion of an application's window. The message is sent when the UpdateWindow or RedrawWindow function is called, or by the DispatchMessage function when the application obtains a WM_PAINT message by using the GetMessage or PeekMessage function.
            /// </summary>
            Paint = 0x000F,
            /// <summary>
            /// The WM_CLOSE message is sent as a signal that a window or an application should terminate.
            /// </summary>
            Close = 0x0010,
            /// <summary>
            /// The WM_QUERYENDSESSION message is sent when the user chooses to end the session or when an application calls one of the system shutdown functions. If any application returns zero, the session is not ended. The system stops sending WM_QUERYENDSESSION messages as soon as one application returns zero.
            /// After processing this message, the system sends the WM_ENDSESSION message with the wParam parameter set to the results of the WM_QUERYENDSESSION message.
            /// </summary>
            Queryendsession = 0x0011,
            /// <summary>
            /// The WM_QUERYOPEN message is sent to an icon when the user requests that the window be restored to its previous size and position.
            /// </summary>
            Queryopen = 0x0013,
            /// <summary>
            /// The WM_ENDSESSION message is sent to an application after the system processes the results of the WM_QUERYENDSESSION message. The WM_ENDSESSION message informs the application whether the session is ending.
            /// </summary>
            Endsession = 0x0016,
            /// <summary>
            /// The WM_QUIT message indicates a request to terminate an application and is generated when the application calls the PostQuitMessage function. It causes the GetMessage function to return zero.
            /// </summary>
            Quit = 0x0012,
            /// <summary>
            /// The WM_ERASEBKGND message is sent when the window background must be erased (for example, when a window is resized). The message is sent to prepare an invalidated portion of a window for painting.
            /// </summary>
            Erasebkgnd = 0x0014,
            /// <summary>
            /// This message is sent to all top-level windows when a change is made to a system color setting.
            /// </summary>
            Syscolorchange = 0x0015,
            /// <summary>
            /// The WM_SHOWWINDOW message is sent to a window when the window is about to be hidden or shown.
            /// </summary>
            Showwindow = 0x0018,
            /// <summary>
            /// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
            /// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
            /// </summary>
            Wininichange = 0x001A,
            /// <summary>
            /// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
            /// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
            /// </summary>
            Settingchange = Wininichange,
            /// <summary>
            /// The WM_DEVMODECHANGE message is sent to all top-level windows whenever the user changes device-mode settings.
            /// </summary>
            Devmodechange = 0x001B,
            /// <summary>
            /// The WM_ACTIVATEAPP message is sent when a window belonging to a different application than the active window is about to be activated. The message is sent to the application whose window is being activated and to the application whose window is being deactivated.
            /// </summary>
            Activateapp = 0x001C,
            /// <summary>
            /// An application sends the WM_FONTCHANGE message to all top-level windows in the system after changing the pool of font resources.
            /// </summary>
            Fontchange = 0x001D,
            /// <summary>
            /// A message that is sent whenever there is a change in the system time.
            /// </summary>
            Timechange = 0x001E,
            /// <summary>
            /// The WM_CANCELMODE message is sent to cancel certain modes, such as mouse capture. For example, the system sends this message to the active window when a dialog box or message box is displayed. Certain functions also send this message explicitly to the specified window regardless of whether it is the active window. For example, the EnableWindow function sends this message when disabling the specified window.
            /// </summary>
            Cancelmode = 0x001F,
            /// <summary>
            /// The WM_SETCURSOR message is sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured.
            /// </summary>
            Setcursor = 0x0020,
            /// <summary>
            /// The WM_MOUSEACTIVATE message is sent when the cursor is in an inactive window and the user presses a mouse button. The parent window receives this message only if the child window passes it to the DefWindowProc function.
            /// </summary>
            Mouseactivate = 0x0021,
            /// <summary>
            /// The WM_CHILDACTIVATE message is sent to a child window when the user clicks the window's title bar or when the window is activated, moved, or sized.
            /// </summary>
            Childactivate = 0x0022,
            /// <summary>
            /// The WM_QUEUESYNC message is sent by a computer-based training (CBT) application to separate user-input messages from other messages sent through the WH_JOURNALPLAYBACK Hook procedure.
            /// </summary>
            Queuesync = 0x0023,
            /// <summary>
            /// The WM_GETMINMAXINFO message is sent to a window when the size or position of the window is about to change. An application can use this message to override the window's default maximized size and position, or its default minimum or maximum tracking size.
            /// </summary>
            Getminmaxinfo = 0x0024,
            /// <summary>
            /// Windows NT 3.51 and earlier: The WM_PAINTICON message is sent to a minimized window when the icon is to be painted. This message is not sent by newer versions of Microsoft Windows, except in unusual circumstances explained in the Remarks.
            /// </summary>
            Painticon = 0x0026,
            /// <summary>
            /// Windows NT 3.51 and earlier: The WM_ICONERASEBKGND message is sent to a minimized window when the background of the icon must be filled before painting the icon. A window receives this message only if a class icon is defined for the window; otherwise, WM_ERASEBKGND is sent. This message is not sent by newer versions of Windows.
            /// </summary>
            Iconerasebkgnd = 0x0027,
            /// <summary>
            /// The WM_NEXTDLGCTL message is sent to a dialog box procedure to set the keyboard focus to a different control in the dialog box.
            /// </summary>
            Nextdlgctl = 0x0028,
            /// <summary>
            /// The WM_SPOOLERSTATUS message is sent from Print Manager whenever a job is added to or removed from the Print Manager queue.
            /// </summary>
            Spoolerstatus = 0x002A,
            /// <summary>
            /// The WM_DRAWITEM message is sent to the parent window of an owner-drawn button, combo box, list box, or menu when a visual aspect of the button, combo box, list box, or menu has changed.
            /// </summary>
            Drawitem = 0x002B,
            /// <summary>
            /// The WM_MEASUREITEM message is sent to the owner window of a combo box, list box, list view control, or menu item when the control or menu is created.
            /// </summary>
            Measureitem = 0x002C,
            /// <summary>
            /// Sent to the owner of a list box or combo box when the list box or combo box is destroyed or when items are removed by the LB_DELETESTRING, LB_RESETCONTENT, CB_DELETESTRING, or CB_RESETCONTENT message. The system sends a WM_DELETEITEM message for each deleted item. The system sends the WM_DELETEITEM message for any deleted list box or combo box item with nonzero item data.
            /// </summary>
            Deleteitem = 0x002D,
            /// <summary>
            /// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_KEYDOWN message.
            /// </summary>
            Vkeytoitem = 0x002E,
            /// <summary>
            /// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_CHAR message.
            /// </summary>
            Chartoitem = 0x002F,
            /// <summary>
            /// An application sends a WM_SETFONT message to specify the font that a control is to use when drawing text.
            /// </summary>
            Setfont = 0x0030,
            /// <summary>
            /// An application sends a WM_GETFONT message to a control to retrieve the font with which the control is currently drawing its text.
            /// </summary>
            Getfont = 0x0031,
            /// <summary>
            /// An application sends a WM_SETHOTKEY message to a window to associate a hot key with the window. When the user presses the hot key, the system activates the window.
            /// </summary>
            Sethotkey = 0x0032,
            /// <summary>
            /// An application sends a WM_GETHOTKEY message to determine the hot key associated with a window.
            /// </summary>
            Gethotkey = 0x0033,
            /// <summary>
            /// The WM_QUERYDRAGICON message is sent to a minimized (iconic) window. The window is about to be dragged by the user but does not have an icon defined for its class. An application can return a handle to an icon or cursor. The system displays this cursor or icon while the user drags the icon.
            /// </summary>
            Querydragicon = 0x0037,
            /// <summary>
            /// The system sends the WM_COMPAREITEM message to determine the relative position of a new item in the sorted list of an owner-drawn combo box or list box. Whenever the application adds a new item, the system sends this message to the owner of a combo box or list box created with the CBS_SORT or LBS_SORT style.
            /// </summary>
            Compareitem = 0x0039,
            /// <summary>
            /// Active Accessibility sends the WM_GETOBJECT message to obtain information about an accessible object contained in a server application.
            /// Applications never send this message directly. It is sent only by Active Accessibility in response to calls to AccessibleObjectFromPoint, AccessibleObjectFromEvent, or AccessibleObjectFromWindow. However, server applications handle this message.
            /// </summary>
            Getobject = 0x003D,
            /// <summary>
            /// The WM_COMPACTING message is sent to all top-level windows when the system detects more than 12.5 percent of system time over a 30- to 60-second interval is being spent compacting memory. This indicates that system memory is low.
            /// </summary>
            Compacting = 0x0041,
            /// <summary>
            /// WM_COMMNOTIFY is Obsolete for Win32-Based Applications
            /// </summary>
            [Obsolete]
            Commnotify = 0x0044,
            /// <summary>
            /// The WM_WINDOWPOSCHANGING message is sent to a window whose size, position, or place in the Z order is about to change as a result of a call to the SetWindowPos function or another window-management function.
            /// </summary>
            Windowposchanging = 0x0046,
            /// <summary>
            /// The WM_WINDOWPOSCHANGED message is sent to a window whose size, position, or place in the Z order has changed as a result of a call to the SetWindowPos function or another window-management function.
            /// </summary>
            Windowposchanged = 0x0047,
            /// <summary>
            /// Notifies applications that the system, typically a battery-powered personal computer, is about to enter a suspended mode.
            /// Use: POWERBROADCAST
            /// </summary>
            [Obsolete]
            Power = 0x0048,
            /// <summary>
            /// An application sends the WM_COPYDATA message to pass data to another application.
            /// </summary>
            Copydata = 0x004A,
            /// <summary>
            /// The WM_CANCELJOURNAL message is posted to an application when a user cancels the application's journaling activities. The message is posted with a NULL window handle.
            /// </summary>
            Canceljournal = 0x004B,
            /// <summary>
            /// Sent by a common control to its parent window when an event has occurred or the control requires some information.
            /// </summary>
            Notify = 0x004E,
            /// <summary>
            /// The WM_INPUTLANGCHANGEREQUEST message is posted to the window with the focus when the user chooses a new input language, either with the hotkey (specified in the Keyboard control panel application) or from the indicator on the system taskbar. An application can accept the change by passing the message to the DefWindowProc function or reject the change (and prevent it from taking place) by returning immediately.
            /// </summary>
            Inputlangchangerequest = 0x0050,
            /// <summary>
            /// The WM_INPUTLANGCHANGE message is sent to the topmost affected window after an application's input language has been changed. You should make any application-specific settings and pass the message to the DefWindowProc function, which passes the message to all first-level child windows. These child windows can pass the message to DefWindowProc to have it pass the message to their child windows, and so on.
            /// </summary>
            Inputlangchange = 0x0051,
            /// <summary>
            /// Sent to an application that has initiated a training card with Microsoft Windows Help. The message informs the application when the user clicks an authorable button. An application initiates a training card by specifying the HELP_TCARD command in a call to the WinHelp function.
            /// </summary>
            Tcard = 0x0052,
            /// <summary>
            /// Indicates that the user pressed the F1 key. If a menu is active when F1 is pressed, WM_HELP is sent to the window associated with the menu; otherwise, WM_HELP is sent to the window that has the keyboard focus. If no window has the keyboard focus, WM_HELP is sent to the currently active window.
            /// </summary>
            Help = 0x0053,
            /// <summary>
            /// The WM_USERCHANGED message is sent to all windows after the user has logged on or off. When the user logs on or off, the system updates the user-specific settings. The system sends this message immediately after updating the settings.
            /// </summary>
            Userchanged = 0x0054,
            /// <summary>
            /// Determines if a window accepts ANSI or Unicode structures in the WM_NOTIFY notification message. WM_NOTIFYFORMAT messages are sent from a common control to its parent window and from the parent window to the common control.
            /// </summary>
            Notifyformat = 0x0055,
            /// <summary>
            /// The WM_CONTEXTMENU message notifies a window that the user clicked the right mouse button (right-clicked) in the window.
            /// </summary>
            Contextmenu = 0x007B,
            /// <summary>
            /// The WM_STYLECHANGING message is sent to a window when the SetWindowLong function is about to change one or more of the window's styles.
            /// </summary>
            Stylechanging = 0x007C,
            /// <summary>
            /// The WM_STYLECHANGED message is sent to a window after the SetWindowLong function has changed one or more of the window's styles
            /// </summary>
            Stylechanged = 0x007D,
            /// <summary>
            /// The WM_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.
            /// </summary>
            Displaychange = 0x007E,
            /// <summary>
            /// The WM_GETICON message is sent to a window to retrieve a handle to the large or small icon associated with a window. The system displays the large icon in the ALT+TAB dialog, and the small icon in the window caption.
            /// </summary>
            Geticon = 0x007F,
            /// <summary>
            /// An application sends the WM_SETICON message to associate a new large or small icon with a window. The system displays the large icon in the ALT+TAB dialog box, and the small icon in the window caption.
            /// </summary>
            Seticon = 0x0080,
            /// <summary>
            /// The WM_NCCREATE message is sent prior to the WM_CREATE message when a window is first created.
            /// </summary>
            Nccreate = 0x0081,
            /// <summary>
            /// The WM_NCDESTROY message informs a window that its nonclient area is being destroyed. The DestroyWindow function sends the WM_NCDESTROY message to the window following the WM_DESTROY message. WM_DESTROY is used to free the allocated memory object associated with the window.
            /// The WM_NCDESTROY message is sent after the child windows have been destroyed. In contrast, WM_DESTROY is sent before the child windows are destroyed.
            /// </summary>
            Ncdestroy = 0x0082,
            /// <summary>
            /// The WM_NCCALCSIZE message is sent when the size and position of a window's client area must be calculated. By processing this message, an application can control the content of the window's client area when the size or position of the window changes.
            /// </summary>
            Nccalcsize = 0x0083,
            /// <summary>
            /// The WM_NCHITTEST message is sent to a window when the cursor moves, or when a mouse button is pressed or released. If the mouse is not captured, the message is sent to the window beneath the cursor. Otherwise, the message is sent to the window that has captured the mouse.
            /// </summary>
            Nchittest = 0x0084,
            /// <summary>
            /// The WM_NCPAINT message is sent to a window when its frame must be painted.
            /// </summary>
            Ncpaint = 0x0085,
            /// <summary>
            /// The WM_NCACTIVATE message is sent to a window when its nonclient area needs to be changed to indicate an active or inactive state.
            /// </summary>
            Ncactivate = 0x0086,
            /// <summary>
            /// The WM_GETDLGCODE message is sent to the window procedure associated with a control. By default, the system handles all keyboard input to the control; the system interprets certain types of keyboard input as dialog box navigation keys. To override this default behavior, the control can respond to the WM_GETDLGCODE message to indicate the types of input it wants to process itself.
            /// </summary>
            Getdlgcode = 0x0087,
            /// <summary>
            /// The WM_SYNCPAINT message is used to synchronize painting while avoiding linking independent GUI threads.
            /// </summary>
            Syncpaint = 0x0088,
            /// <summary>
            /// The WM_NCMOUSEMOVE message is posted to a window when the cursor is moved within the nonclient area of the window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncmousemove = 0x00A0,
            /// <summary>
            /// The WM_NCLBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Nclbuttondown = 0x00A1,
            /// <summary>
            /// The WM_NCLBUTTONUP message is posted when the user releases the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Nclbuttonup = 0x00A2,
            /// <summary>
            /// The WM_NCLBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Nclbuttondblclk = 0x00A3,
            /// <summary>
            /// The WM_NCRBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncrbuttondown = 0x00A4,
            /// <summary>
            /// The WM_NCRBUTTONUP message is posted when the user releases the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncrbuttonup = 0x00A5,
            /// <summary>
            /// The WM_NCRBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncrbuttondblclk = 0x00A6,
            /// <summary>
            /// The WM_NCMBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncmbuttondown = 0x00A7,
            /// <summary>
            /// The WM_NCMBUTTONUP message is posted when the user releases the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncmbuttonup = 0x00A8,
            /// <summary>
            /// The WM_NCMBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncmbuttondblclk = 0x00A9,
            /// <summary>
            /// The WM_NCXBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncxbuttondown = 0x00AB,
            /// <summary>
            /// The WM_NCXBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncxbuttonup = 0x00AC,
            /// <summary>
            /// The WM_NCXBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
            /// </summary>
            Ncxbuttondblclk = 0x00AD,
            /// <summary>
            /// The WM_INPUT_DEVICE_CHANGE message is sent to the window that registered to receive raw input. A window receives this message through its WindowProc function.
            /// </summary>
            InputDeviceChange = 0x00FE,
            /// <summary>
            /// The WM_INPUT message is sent to the window that is getting raw input.
            /// </summary>
            Input = 0x00FF,
            /// <summary>
            /// This message filters for keyboard messages.
            /// </summary>
            Keyfirst = 0x0100,
            /// <summary>
            /// The WM_KEYDOWN message is posted to the window with the keyboard focus when a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed.
            /// </summary>
            Keydown = 0x0100,
            /// <summary>
            /// The WM_KEYUP message is posted to the window with the keyboard focus when a nonsystem key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard key that is pressed when a window has the keyboard focus.
            /// </summary>
            Keyup = 0x0101,
            /// <summary>
            /// The WM_CHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_CHAR message contains the character code of the key that was pressed.
            /// </summary>
            Char = 0x0102,
            /// <summary>
            /// The WM_DEADCHAR message is posted to the window with the keyboard focus when a WM_KEYUP message is translated by the TranslateMessage function. WM_DEADCHAR specifies a character code generated by a dead key. A dead key is a key that generates a character, such as the umlaut (double-dot), that is combined with another character to form a composite character. For example, the umlaut-O character (? is generated by typing the dead key for the umlaut character, and then typing the O key.
            /// </summary>
            Deadchar = 0x0103,
            /// <summary>
            /// The WM_SYSKEYDOWN message is posted to the window with the keyboard focus when the user presses the F10 key (which activates the menu bar) or holds down the ALT key and then presses another key. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYDOWN message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter.
            /// </summary>
            Syskeydown = 0x0104,
            /// <summary>
            /// The WM_SYSKEYUP message is posted to the window with the keyboard focus when the user releases a key that was pressed while the ALT key was held down. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter.
            /// </summary>
            Syskeyup = 0x0105,
            /// <summary>
            /// The WM_SYSCHAR message is posted to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. It specifies the character code of a system character key ?that is, a character key that is pressed while the ALT key is down.
            /// </summary>
            Syschar = 0x0106,
            /// <summary>
            /// The WM_SYSDEADCHAR message is sent to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. WM_SYSDEADCHAR specifies the character code of a system dead key ?that is, a dead key that is pressed while holding down the ALT key.
            /// </summary>
            Sysdeadchar = 0x0107,
            /// <summary>
            /// The WM_UNICHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_UNICHAR message contains the character code of the key that was pressed.
            /// The WM_UNICHAR message is equivalent to WM_CHAR, but it uses Unicode Transformation Format (UTF)-32, whereas WM_CHAR uses UTF-16. It is designed to send or post Unicode characters to ANSI windows and it can can handle Unicode Supplementary Plane characters.
            /// </summary>
            Unichar = 0x0109,
            /// <summary>
            /// This message filters for keyboard messages.
            /// </summary>
            Keylast = 0x0109,
            /// <summary>
            /// Sent immediately before the IME generates the composition string as a result of a keystroke. A window receives this message through its WindowProc function.
            /// </summary>
            ImeStartcomposition = 0x010D,
            /// <summary>
            /// Sent to an application when the IME ends composition. A window receives this message through its WindowProc function.
            /// </summary>
            ImeEndcomposition = 0x010E,
            /// <summary>
            /// Sent to an application when the IME changes composition status as a result of a keystroke. A window receives this message through its WindowProc function.
            /// </summary>
            ImeComposition = 0x010F,
            ImeKeylast = 0x010F,
            /// <summary>
            /// The WM_INITDIALOG message is sent to the dialog box procedure immediately before a dialog box is displayed. Dialog box procedures typically use this message to initialize controls and carry out any other initialization tasks that affect the appearance of the dialog box.
            /// </summary>
            Initdialog = 0x0110,
            /// <summary>
            /// The WM_COMMAND message is sent when the user selects a command item from a menu, when a control sends a notification message to its parent window, or when an accelerator keystroke is translated.
            /// </summary>
            Command = 0x0111,
            /// <summary>
            /// A window receives this message when the user chooses a command from the Window menu, clicks the maximize button, minimize button, restore button, close button, or moves the form. You can stop the form from moving by filtering this out.
            /// </summary>
            Syscommand = 0x0112,
            /// <summary>
            /// The WM_TIMER message is posted to the installing thread's message queue when a timer expires. The message is posted by the GetMessage or PeekMessage function.
            /// </summary>
            Timer = 0x0113,
            /// <summary>
            /// The WM_HSCROLL message is sent to a window when a scroll event occurs in the window's standard horizontal scroll bar. This message is also sent to the owner of a horizontal scroll bar control when a scroll event occurs in the control.
            /// </summary>
            Hscroll = 0x0114,
            /// <summary>
            /// The WM_VSCROLL message is sent to a window when a scroll event occurs in the window's standard vertical scroll bar. This message is also sent to the owner of a vertical scroll bar control when a scroll event occurs in the control.
            /// </summary>
            Vscroll = 0x0115,
            /// <summary>
            /// The WM_INITMENU message is sent when a menu is about to become active. It occurs when the user clicks an item on the menu bar or presses a menu key. This allows the application to modify the menu before it is displayed.
            /// </summary>
            Initmenu = 0x0116,
            /// <summary>
            /// The WM_INITMENUPOPUP message is sent when a drop-down menu or submenu is about to become active. This allows an application to modify the menu before it is displayed, without changing the entire menu.
            /// </summary>
            Initmenupopup = 0x0117,
            /// <summary>
            /// The WM_MENUSELECT message is sent to a menu's owner window when the user selects a menu item.
            /// </summary>
            Menuselect = 0x011F,
            /// <summary>
            /// The WM_MENUCHAR message is sent when a menu is active and the user presses a key that does not correspond to any mnemonic or accelerator key. This message is sent to the window that owns the menu.
            /// </summary>
            Menuchar = 0x0120,
            /// <summary>
            /// The WM_ENTERIDLE message is sent to the owner window of a modal dialog box or menu that is entering an idle state. A modal dialog box or menu enters an idle state when no messages are waiting in its queue after it has processed one or more previous messages.
            /// </summary>
            Enteridle = 0x0121,
            /// <summary>
            /// The WM_MENURBUTTONUP message is sent when the user releases the right mouse button while the cursor is on a menu item.
            /// </summary>
            Menurbuttonup = 0x0122,
            /// <summary>
            /// The WM_MENUDRAG message is sent to the owner of a drag-and-drop menu when the user drags a menu item.
            /// </summary>
            Menudrag = 0x0123,
            /// <summary>
            /// The WM_MENUGETOBJECT message is sent to the owner of a drag-and-drop menu when the mouse cursor enters a menu item or moves from the center of the item to the top or bottom of the item.
            /// </summary>
            Menugetobject = 0x0124,
            /// <summary>
            /// The WM_UNINITMENUPOPUP message is sent when a drop-down menu or submenu has been destroyed.
            /// </summary>
            Uninitmenupopup = 0x0125,
            /// <summary>
            /// The WM_MENUCOMMAND message is sent when the user makes a selection from a menu.
            /// </summary>
            Menucommand = 0x0126,
            /// <summary>
            /// An application sends the WM_CHANGEUISTATE message to indicate that the user interface (UI) state should be changed.
            /// </summary>
            Changeuistate = 0x0127,
            /// <summary>
            /// An application sends the WM_UPDATEUISTATE message to change the user interface (UI) state for the specified window and all its child windows.
            /// </summary>
            Updateuistate = 0x0128,
            /// <summary>
            /// An application sends the WM_QUERYUISTATE message to retrieve the user interface (UI) state for a window.
            /// </summary>
            Queryuistate = 0x0129,
            /// <summary>
            /// The WM_CTLCOLORMSGBOX message is sent to the owner window of a message box before Windows draws the message box. By responding to this message, the owner window can set the text and background colors of the message box by using the given display device context handle.
            /// </summary>
            Ctlcolormsgbox = 0x0132,
            /// <summary>
            /// An edit control that is not read-only or disabled sends the WM_CTLCOLOREDIT message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the edit control.
            /// </summary>
            Ctlcoloredit = 0x0133,
            /// <summary>
            /// Sent to the parent window of a list box before the system draws the list box. By responding to this message, the parent window can set the text and background colors of the list box by using the specified display device context handle.
            /// </summary>
            Ctlcolorlistbox = 0x0134,
            /// <summary>
            /// The WM_CTLCOLORBTN message is sent to the parent window of a button before drawing the button. The parent window can change the button's text and background colors. However, only owner-drawn buttons respond to the parent window processing this message.
            /// </summary>
            Ctlcolorbtn = 0x0135,
            /// <summary>
            /// The WM_CTLCOLORDLG message is sent to a dialog box before the system draws the dialog box. By responding to this message, the dialog box can set its text and background colors using the specified display device context handle.
            /// </summary>
            Ctlcolordlg = 0x0136,
            /// <summary>
            /// The WM_CTLCOLORSCROLLBAR message is sent to the parent window of a scroll bar control when the control is about to be drawn. By responding to this message, the parent window can use the display context handle to set the background color of the scroll bar control.
            /// </summary>
            Ctlcolorscrollbar = 0x0137,
            /// <summary>
            /// A static control, or an edit control that is read-only or disabled, sends the WM_CTLCOLORSTATIC message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the static control.
            /// </summary>
            Ctlcolorstatic = 0x0138,
            /// <summary>
            /// Use WM_MOUSEFIRST to specify the first mouse message. Use the PeekMessage() Function.
            /// </summary>
            Mousefirst = 0x0200,
            /// <summary>
            /// The WM_MOUSEMOVE message is posted to a window when the cursor moves. If the mouse is not captured, the message is posted to the window that contains the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Mousemove = 0x0200,
            /// <summary>
            /// The WM_LBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Lbuttondown = 0x0201,
            /// <summary>
            /// The WM_LBUTTONUP message is posted when the user releases the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Lbuttonup = 0x0202,
            /// <summary>
            /// The WM_LBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Lbuttondblclk = 0x0203,
            /// <summary>
            /// The WM_RBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Rbuttondown = 0x0204,
            /// <summary>
            /// The WM_RBUTTONUP message is posted when the user releases the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Rbuttonup = 0x0205,
            /// <summary>
            /// The WM_RBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Rbuttondblclk = 0x0206,
            /// <summary>
            /// The WM_MBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Mbuttondown = 0x0207,
            /// <summary>
            /// The WM_MBUTTONUP message is posted when the user releases the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Mbuttonup = 0x0208,
            /// <summary>
            /// The WM_MBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Mbuttondblclk = 0x0209,
            /// <summary>
            /// The WM_MOUSEWHEEL message is sent to the focus window when the mouse wheel is rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
            /// </summary>
            Mousewheel = 0x020A,
            /// <summary>
            /// The WM_XBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Xbuttondown = 0x020B,
            /// <summary>
            /// The WM_XBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Xbuttonup = 0x020C,
            /// <summary>
            /// The WM_XBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
            /// </summary>
            Xbuttondblclk = 0x020D,
            /// <summary>
            /// The WM_MOUSEHWHEEL message is sent to the focus window when the mouse's horizontal scroll wheel is tilted or rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
            /// </summary>
            Mousehwheel = 0x020E,
            /// <summary>
            /// Use WM_MOUSELAST to specify the last mouse message. Used with PeekMessage() Function.
            /// </summary>
            Mouselast = 0x020E,
            /// <summary>
            /// The WM_PARENTNOTIFY message is sent to the parent of a child window when the child window is created or destroyed, or when the user clicks a mouse button while the cursor is over the child window. When the child window is being created, the system sends WM_PARENTNOTIFY just before the CreateWindow or CreateWindowEx function that creates the window returns. When the child window is being destroyed, the system sends the message before any processing to destroy the window takes place.
            /// </summary>
            Parentnotify = 0x0210,
            /// <summary>
            /// The WM_ENTERMENULOOP message informs an application's main window procedure that a menu modal loop has been entered.
            /// </summary>
            Entermenuloop = 0x0211,
            /// <summary>
            /// The WM_EXITMENULOOP message informs an application's main window procedure that a menu modal loop has been exited.
            /// </summary>
            Exitmenuloop = 0x0212,
            /// <summary>
            /// The WM_NEXTMENU message is sent to an application when the right or left arrow key is used to switch between the menu bar and the system menu.
            /// </summary>
            Nextmenu = 0x0213,
            /// <summary>
            /// The WM_SIZING message is sent to a window that the user is resizing. By processing this message, an application can monitor the size and position of the drag rectangle and, if needed, change its size or position.
            /// </summary>
            Sizing = 0x0214,
            /// <summary>
            /// The WM_CAPTURECHANGED message is sent to the window that is losing the mouse capture.
            /// </summary>
            Capturechanged = 0x0215,
            /// <summary>
            /// The WM_MOVING message is sent to a window that the user is moving. By processing this message, an application can monitor the position of the drag rectangle and, if needed, change its position.
            /// </summary>
            Moving = 0x0216,
            /// <summary>
            /// Notifies applications that a power-management event has occurred.
            /// </summary>
            Powerbroadcast = 0x0218,
            /// <summary>
            /// Notifies an application of a change to the hardware configuration of a device or the computer.
            /// </summary>
            Devicechange = 0x0219,
            /// <summary>
            /// An application sends the WM_MDICREATE message to a multiple-document interface (MDI) client window to create an MDI child window.
            /// </summary>
            Mdicreate = 0x0220,
            /// <summary>
            /// An application sends the WM_MDIDESTROY message to a multiple-document interface (MDI) client window to close an MDI child window.
            /// </summary>
            Mdidestroy = 0x0221,
            /// <summary>
            /// An application sends the WM_MDIACTIVATE message to a multiple-document interface (MDI) client window to instruct the client window to activate a different MDI child window.
            /// </summary>
            Mdiactivate = 0x0222,
            /// <summary>
            /// An application sends the WM_MDIRESTORE message to a multiple-document interface (MDI) client window to restore an MDI child window from maximized or minimized size.
            /// </summary>
            Mdirestore = 0x0223,
            /// <summary>
            /// An application sends the WM_MDINEXT message to a multiple-document interface (MDI) client window to activate the next or previous child window.
            /// </summary>
            Mdinext = 0x0224,
            /// <summary>
            /// An application sends the WM_MDIMAXIMIZE message to a multiple-document interface (MDI) client window to maximize an MDI child window. The system resizes the child window to make its client area fill the client window. The system places the child window's window menu icon in the rightmost position of the frame window's menu bar, and places the child window's restore icon in the leftmost position. The system also appends the title bar text of the child window to that of the frame window.
            /// </summary>
            Mdimaximize = 0x0225,
            /// <summary>
            /// An application sends the WM_MDITILE message to a multiple-document interface (MDI) client window to arrange all of its MDI child windows in a tile format.
            /// </summary>
            Mditile = 0x0226,
            /// <summary>
            /// An application sends the WM_MDICASCADE message to a multiple-document interface (MDI) client window to arrange all its child windows in a cascade format.
            /// </summary>
            Mdicascade = 0x0227,
            /// <summary>
            /// An application sends the WM_MDIICONARRANGE message to a multiple-document interface (MDI) client window to arrange all minimized MDI child windows. It does not affect child windows that are not minimized.
            /// </summary>
            Mdiiconarrange = 0x0228,
            /// <summary>
            /// An application sends the WM_MDIGETACTIVE message to a multiple-document interface (MDI) client window to retrieve the handle to the active MDI child window.
            /// </summary>
            Mdigetactive = 0x0229,
            /// <summary>
            /// An application sends the WM_MDISETMENU message to a multiple-document interface (MDI) client window to replace the entire menu of an MDI frame window, to replace the window menu of the frame window, or both.
            /// </summary>
            Mdisetmenu = 0x0230,
            /// <summary>
            /// The WM_ENTERSIZEMOVE message is sent one time to a window after it enters the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns.
            /// The system sends the WM_ENTERSIZEMOVE message regardless of whether the dragging of full windows is enabled.
            /// </summary>
            Entersizemove = 0x0231,
            /// <summary>
            /// The WM_EXITSIZEMOVE message is sent one time to a window, after it has exited the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns.
            /// </summary>
            Exitsizemove = 0x0232,
            /// <summary>
            /// Sent when the user drops a file on the window of an application that has registered itself as a recipient of dropped files.
            /// </summary>
            Dropfiles = 0x0233,
            /// <summary>
            /// An application sends the WM_MDIREFRESHMENU message to a multiple-document interface (MDI) client window to refresh the window menu of the MDI frame window.
            /// </summary>
            Mdirefreshmenu = 0x0234,
            /// <summary>
            /// Sent to an application when a window is activated. A window receives this message through its WindowProc function.
            /// </summary>
            ImeSetcontext = 0x0281,
            /// <summary>
            /// Sent to an application to notify it of changes to the IME window. A window receives this message through its WindowProc function.
            /// </summary>
            ImeNotify = 0x0282,
            /// <summary>
            /// Sent by an application to direct the IME window to carry out the requested command. The application uses this message to control the IME window that it has created. To send this message, the application calls the SendMessage function with the following parameters.
            /// </summary>
            ImeControl = 0x0283,
            /// <summary>
            /// Sent to an application when the IME window finds no space to extend the area for the composition window. A window receives this message through its WindowProc function.
            /// </summary>
            ImeCompositionfull = 0x0284,
            /// <summary>
            /// Sent to an application when the operating system is about to change the current IME. A window receives this message through its WindowProc function.
            /// </summary>
            ImeSelect = 0x0285,
            /// <summary>
            /// Sent to an application when the IME gets a character of the conversion result. A window receives this message through its WindowProc function.
            /// </summary>
            ImeChar = 0x0286,
            /// <summary>
            /// Sent to an application to provide commands and request information. A window receives this message through its WindowProc function.
            /// </summary>
            ImeRequest = 0x0288,
            /// <summary>
            /// Sent to an application by the IME to notify the application of a key press and to keep message order. A window receives this message through its WindowProc function.
            /// </summary>
            ImeKeydown = 0x0290,
            /// <summary>
            /// Sent to an application by the IME to notify the application of a key release and to keep message order. A window receives this message through its WindowProc function.
            /// </summary>
            ImeKeyup = 0x0291,
            /// <summary>
            /// The WM_MOUSEHOVER message is posted to a window when the cursor hovers over the client area of the window for the period of time specified in a prior call to TrackMouseEvent.
            /// </summary>
            Mousehover = 0x02A1,
            /// <summary>
            /// The WM_MOUSELEAVE message is posted to a window when the cursor leaves the client area of the window specified in a prior call to TrackMouseEvent.
            /// </summary>
            Mouseleave = 0x02A3,
            /// <summary>
            /// The WM_NCMOUSEHOVER message is posted to a window when the cursor hovers over the nonclient area of the window for the period of time specified in a prior call to TrackMouseEvent.
            /// </summary>
            Ncmousehover = 0x02A0,
            /// <summary>
            /// The WM_NCMOUSELEAVE message is posted to a window when the cursor leaves the nonclient area of the window specified in a prior call to TrackMouseEvent.
            /// </summary>
            Ncmouseleave = 0x02A2,
            /// <summary>
            /// The WM_WTSSESSION_CHANGE message notifies applications of changes in session state.
            /// </summary>
            WtssessionChange = 0x02B1,
            TabletFirst = 0x02c0,
            TabletLast = 0x02df,
            /// <summary>
            /// An application sends a WM_CUT message to an edit control or combo box to delete (cut) the current selection, if any, in the edit control and copy the deleted text to the clipboard in CF_TEXT format.
            /// </summary>
            Cut = 0x0300,
            /// <summary>
            /// An application sends the WM_COPY message to an edit control or combo box to copy the current selection to the clipboard in CF_TEXT format.
            /// </summary>
            Copy = 0x0301,
            /// <summary>
            /// An application sends a WM_PASTE message to an edit control or combo box to copy the current content of the clipboard to the edit control at the current caret position. Data is inserted only if the clipboard contains data in CF_TEXT format.
            /// </summary>
            Paste = 0x0302,
            /// <summary>
            /// An application sends a WM_CLEAR message to an edit control or combo box to delete (clear) the current selection, if any, from the edit control.
            /// </summary>
            Clear = 0x0303,
            /// <summary>
            /// An application sends a WM_UNDO message to an edit control to undo the last operation. When this message is sent to an edit control, the previously deleted text is restored or the previously added text is deleted.
            /// </summary>
            Undo = 0x0304,
            /// <summary>
            /// The WM_RENDERFORMAT message is sent to the clipboard owner if it has delayed rendering a specific clipboard format and if an application has requested data in that format. The clipboard owner must render data in the specified format and place it on the clipboard by calling the SetClipboardData function.
            /// </summary>
            Renderformat = 0x0305,
            /// <summary>
            /// The WM_RENDERALLFORMATS message is sent to the clipboard owner before it is destroyed, if the clipboard owner has delayed rendering one or more clipboard formats. For the content of the clipboard to remain available to other applications, the clipboard owner must render data in all the formats it is capable of generating, and place the data on the clipboard by calling the SetClipboardData function.
            /// </summary>
            Renderallformats = 0x0306,
            /// <summary>
            /// The WM_DESTROYCLIPBOARD message is sent to the clipboard owner when a call to the EmptyClipboard function empties the clipboard.
            /// </summary>
            Destroyclipboard = 0x0307,
            /// <summary>
            /// The WM_DRAWCLIPBOARD message is sent to the first window in the clipboard viewer chain when the content of the clipboard changes. This enables a clipboard viewer window to display the new content of the clipboard.
            /// </summary>
            Drawclipboard = 0x0308,
            /// <summary>
            /// The WM_PAINTCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area needs repainting.
            /// </summary>
            Paintclipboard = 0x0309,
            /// <summary>
            /// The WM_VSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's vertical scroll bar. The owner should scroll the clipboard image and update the scroll bar values.
            /// </summary>
            Vscrollclipboard = 0x030A,
            /// <summary>
            /// The WM_SIZECLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area has changed size.
            /// </summary>
            Sizeclipboard = 0x030B,
            /// <summary>
            /// The WM_ASKCBFORMATNAME message is sent to the clipboard owner by a clipboard viewer window to request the name of a CF_OWNERDISPLAY clipboard format.
            /// </summary>
            Askcbformatname = 0x030C,
            /// <summary>
            /// The WM_CHANGECBCHAIN message is sent to the first window in the clipboard viewer chain when a window is being removed from the chain.
            /// </summary>
            Changecbchain = 0x030D,
            /// <summary>
            /// The WM_HSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window. This occurs when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's horizontal scroll bar. The owner should scroll the clipboard image and update the scroll bar values.
            /// </summary>
            Hscrollclipboard = 0x030E,
            /// <summary>
            /// This message informs a window that it is about to receive the keyboard focus, giving the window the opportunity to realize its logical palette when it receives the focus.
            /// </summary>
            Querynewpalette = 0x030F,
            /// <summary>
            /// The WM_PALETTEISCHANGING message informs applications that an application is going to realize its logical palette.
            /// </summary>
            Paletteischanging = 0x0310,
            /// <summary>
            /// This message is sent by the OS to all top-level and overlapped windows after the window with the keyboard focus realizes its logical palette.
            /// This message enables windows that do not have the keyboard focus to realize their logical palettes and update their client areas.
            /// </summary>
            Palettechanged = 0x0311,
            /// <summary>
            /// The WM_HOTKEY message is posted when the user presses a hot key registered by the RegisterHotKey function. The message is placed at the top of the message queue associated with the thread that registered the hot key.
            /// </summary>
            Hotkey = 0x0312,
            /// <summary>
            /// The WM_PRINT message is sent to a window to request that it draw itself in the specified device context, most commonly in a printer device context.
            /// </summary>
            Print = 0x0317,
            /// <summary>
            /// The WM_PRINTCLIENT message is sent to a window to request that it draw its client area in the specified device context, most commonly in a printer device context.
            /// </summary>
            Printclient = 0x0318,
            /// <summary>
            /// The WM_APPCOMMAND message notifies a window that the user generated an application command event, for example, by clicking an application command button using the mouse or typing an application command key on the keyboard.
            /// </summary>
            Appcommand = 0x0319,
            /// <summary>
            /// The WM_THEMECHANGED message is broadcast to every window following a theme change event. Examples of theme change events are the activation of a theme, the deactivation of a theme, or a transition from one theme to another.
            /// </summary>
            Themechanged = 0x031A,
            /// <summary>
            /// Sent when the contents of the clipboard have changed.
            /// </summary>
            Clipboardupdate = 0x031D,
            /// <summary>
            /// The system will send a window the WM_DWMCOMPOSITIONCHANGED message to indicate that the availability of desktop composition has changed.
            /// </summary>
            Dwmcompositionchanged = 0x031E,
            /// <summary>
            /// WM_DWMNCRENDERINGCHANGED is called when the non-client area rendering status of a window has changed. Only windows that have set the flag DWM_BLURBEHIND.fTransitionOnMaximized to true will get this message.
            /// </summary>
            Dwmncrenderingchanged = 0x031F,
            /// <summary>
            /// Sent to all top-level windows when the colorization color has changed.
            /// </summary>
            Dwmcolorizationcolorchanged = 0x0320,
            /// <summary>
            /// WM_DWMWINDOWMAXIMIZEDCHANGE will let you know when a DWM composed window is maximized. You also have to register for this message as well. You'd have other windowd go opaque when this message is sent.
            /// </summary>
            Dwmwindowmaximizedchange = 0x0321,
            /// <summary>
            /// Sent to request extended title bar information. A window receives this message through its WindowProc function.
            /// </summary>
            Gettitlebarinfoex = 0x033F,
            Handheldfirst = 0x0358,
            Handheldlast = 0x035F,
            Afxfirst = 0x0360,
            Afxlast = 0x037F,
            Penwinfirst = 0x0380,
            Penwinlast = 0x038F,
            /// <summary>
            /// The WM_APP constant is used by applications to help define private messages, usually of the form WM_APP+X, where X is an integer value.
            /// </summary>
            App = 0x8000,
            /// <summary>
            /// The WM_USER constant is used by applications to help define private messages for use by private window classes, usually of the form WM_USER+X, where X is an integer value.
            /// </summary>
            User = 0x0400,

            /// <summary>
            /// An application sends the WM_CPL_LAUNCH message to Windows Control Panel to request that a Control Panel application be started.
            /// </summary>
            CplLaunch = User + 0x1000,
            /// <summary>
            /// The WM_CPL_LAUNCHED message is sent when a Control Panel application, started by the WM_CPL_LAUNCH message, has closed. The WM_CPL_LAUNCHED message is sent to the window identified by the wParam parameter of the WM_CPL_LAUNCH message that started the application.
            /// </summary>
            CplLaunched = User + 0x1001,
            /// <summary>
            /// WM_SYSTIMER is a well-known yet still undocumented message. Windows uses WM_SYSTIMER for internal actions like scrolling.
            /// </summary>
            Systimer = 0x118,

            /// <summary>
            /// The accessibility state has changed.
            /// </summary>
            HshellAccessibilitystate = 11,
            /// <summary>
            /// The shell should activate its main window.
            /// </summary>
            HshellActivateshellwindow = 3,
            /// <summary>
            /// The user completed an input event (for example, pressed an application command button on the mouse or an application command key on the keyboard), and the application did not handle the WM_APPCOMMAND message generated by that input.
            /// If the Shell procedure handles the WM_COMMAND message, it should not call CallNextHookEx. See the Return Value section for more information.
            /// </summary>
            HshellAppcommand = 12,
            /// <summary>
            /// A window is being minimized or maximized. The system needs the coordinates of the minimized rectangle for the window.
            /// </summary>
            HshellGetminrect = 5,
            /// <summary>
            /// Keyboard language was changed or a new keyboard layout was loaded.
            /// </summary>
            HshellLanguage = 8,
            /// <summary>
            /// The title of a window in the task bar has been redrawn.
            /// </summary>
            HshellRedraw = 6,
            /// <summary>
            /// The user has selected the task list. A shell application that provides a task list should return TRUE to prevent Windows from starting its task list.
            /// </summary>
            HshellTaskman = 7,
            /// <summary>
            /// A top-level, unowned window has been created. The window exists when the system calls this hook.
            /// </summary>
            HshellWindowcreated = 1,
            /// <summary>
            /// A top-level, unowned window is about to be destroyed. The window still exists when the system calls this hook.
            /// </summary>
            HshellWindowdestroyed = 2,
            /// <summary>
            /// The activation has changed to a different top-level, unowned window.
            /// </summary>
            HshellWindowactivated = 4,
            /// <summary>
            /// A top-level window is being replaced. The window exists when the system calls this hook.
            /// </summary>
            HshellWindowreplaced = 13
        }
        #endregion

        public class TrayIconInfo
        {
            public ToolBarButtonInfo ToolBarButtonInfo { get; set; }
            public IntPtr MainWindowHandle { get; set; }
            public IntPtr TrayIconHandle { get; set; }
            public string ToolTip { get; set; }
        }

        public enum Gw : uint
        {
            Hwndfirst = 0,
            Hwndlast = 1,
            Hwndnext = 2,
            Hwndprev = 3,
            Owner = 4,
            Child = 5,
            Max = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ToolBarButtonData
        {
            public IntPtr hwnd;
            public uint uID;
            public uint uCallbackMessage;
            private uint Reserved;
            private uint Reserved2;
            public IntPtr hIcon;
        }

        public class Icon
        {
            public const UInt32 Small = 0;
            public const UInt32 Big = 1;
            public const UInt32 Small2 = 2; // XP+
        }

        public enum MessageBox : uint
        {
            SimpleBeep = 0xFFFFFFFF,
            IconAsterisk = 0x00000040,
            IconWarning = 0x00000030,
            IconError = 0x00000010,
            IconQuestion = 0x00000020,
            Ok = 0x00000000
        }

        //Window State
        public class WindowState
        {
            public const int Hide = 0;
            public const int Shownormal = 1;
            public const int Normal = 1;
            public const int Showminimized = 2;
            public const int Showmaximized = 3;
            public const int Maximize = 3;
            public const int Shownoactivate = 4;
            public const int Show = 5;
            public const int Minimize = 6;
            public const int Showminnoactive = 7;
            public const int Showna = 8;
            public const int Restore = 9;
            public const int Showdefault = 10;
            public const int Forceminimize = 11;
            public const int Max = 11;
        }

        public class ToolBar
        {
            public const uint Getbutton = WindowsMessage.User + 23;
            public const uint Buttoncount = WindowsMessage.User + 24;
            public const uint Customize = WindowsMessage.User + 27;
            public const uint Getbuttontexta = WindowsMessage.User + 45;
            public const uint Getbuttontextw = WindowsMessage.User + 75;
            public const uint Hidebutton = WindowsMessage.User + 4;
            public const uint Deletebutton = WindowsMessage.User + 22;
            public const uint Getbuttoninfo = WindowsMessage.User + 63;
            public const uint Getitemrect = (WindowsMessage.User + 29);
            public const uint Pressbutton = (WindowsMessage.User + 3);
            public const uint WmLbuttondblclk = 0x0203;
            public const uint StateHidden = 0x08;
        }

        public class ToolBarState
        {
            public const uint Checked = 0x01;
            public const uint Pressed = 0x02;
            public const uint Enabled = 0x04;
            public const uint Hidden = 0x08;
            public const uint Indeterminate = 0x10;
            public const uint Wrap = 0x20;
            public const uint Ellipses = 0x40;
            public const uint Marked = 0x80;
        }

        internal class WindowsMessage
        {
            public const uint Close = 0x0010;
            public const uint Geticon = 0x007F;
            public const uint Keydown = 0x0100;
            public const uint Command = 0x0111;
            public const uint User = 0x0400; // 0x0400 - 0x7FFF
            public const uint App = 0x8000; // 0x8000 - 0xBFFF
        }

        public class Gcl
        {
            public const int Menuname = -8;
            public const int Hbrbackground = -10;
            public const int Hcursor = -12;
            public const int Hicon = -14;
            public const int Hmodule = -16;
            public const int Cbwndextra = -18;
            public const int Cbclsextra = -20;
            public const int Wndproc = -24;
            public const int Style = -26;
            public const int Atom = -32;
            public const int Hiconsm = -34;

            // GetClassLongPtr ( 64-bit )
            private const int GcwAtom = -32;
            private const int GclCbclsextra = -20;
            private const int GclCbwndextra = -18;
            private const int GclpMenuname = -8;
            private const int GclpHbrbackground = -10;
            private const int GclpHcursor = -12;
            private const int GclpHicon = -14;
            private const int GclpHmodule = -16;
            private const int GclpWndproc = -24;
            private const int GclpHiconsm = -34;
            private const int GclStyle = -26;
        }

        internal class MemoryProtection
        {
            public const UInt32 PageNoaccess = 0x01;
            public const UInt32 PageReadonly = 0x02;
            public const UInt32 PageReadwrite = 0x04;
            public const UInt32 PageWritecopy = 0x08;
            public const UInt32 PageExecute = 0x10;
            public const UInt32 PageExecuteRead = 0x20;
            public const UInt32 PageExecuteReadwrite = 0x40;
            public const UInt32 PageExecuteWritecopy = 0x80;
            public const UInt32 PageGuard = 0x100;
            public const UInt32 PageNocache = 0x200;
            public const UInt32 PageWritecombine = 0x400;
        }

        internal class MemAllocationType
        {
            public const UInt32 Commit = 0x1000;
            public const UInt32 Reserve = 0x2000;
            public const UInt32 Decommit = 0x4000;
            public const UInt32 Release = 0x8000;
            public const UInt32 Free = 0x10000;
            public const UInt32 Private = 0x20000;
            public const UInt32 Mapped = 0x40000;
            public const UInt32 Reset = 0x80000;
            public const UInt32 TopDown = 0x100000;
            public const UInt32 WriteWatch = 0x200000;
            public const UInt32 Physical = 0x400000;
            public const UInt32 LargePages = 0x20000000;
            public const UInt32 FourmbPages = 0x80000000;
        }

        internal class ProcessRights
        {
            public const UInt32 Terminate = 0x0001;
            public const UInt32 CreateThread = 0x0002;
            public const UInt32 SetSessionid = 0x0004;
            public const UInt32 VmOperation = 0x0008;
            public const UInt32 VmRead = 0x0010;
            public const UInt32 VmWrite = 0x0020;
            public const UInt32 DupHandle = 0x0040;
            public const UInt32 CreateProcess = 0x0080;
            public const UInt32 SetQuota = 0x0100;
            public const UInt32 SetInformation = 0x0200;
            public const UInt32 QueryInformation = 0x0400;
            public const UInt32 SuspendResume = 0x0800;
            private const UInt32 StandardRightsRequired = 0x000F0000;
            private const UInt32 Synchronize = 0x00100000;
            public const uint AllAccess = StandardRightsRequired | Synchronize | 0xFFF;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ToolBarButtonInfo
        {
            public int iBitmap;
            public int idCommand;

            [StructLayout(LayoutKind.Explicit)]
            private struct TbbuttonU
            {
                [FieldOffset(0)]
                public byte fsState;
                [FieldOffset(1)]
                public byte fsStyle;
                [FieldOffset(0)]
                private IntPtr bReserved;
            }

            private TbbuttonU union;
            public byte FsState
            {
                get { return union.fsState; }
                set { union.fsState = value; }
            }
            public byte FsStyle
            {
                get { return union.fsStyle; }
                set { union.fsStyle = value; }
            }
            public IntPtr dwData;
            public IntPtr iString;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rectangle
        {
            public int Left; // x position of upper-left corner
            public int Top; // y position of upper-left corner
            public int Right; // x position of lower-right corner
            public int Bottom; // y position of lower-right corner
            public int Height
            {
                get { return Bottom - Top; }
            }
            public int Width
            {
                get { return Right - Left; }
            }
            public Size Size
            {
                get { return new Size(Width, Height); }
            }

            public bool IsEmpty
            {
                get
                {
                    if(Left == 0 &&
                       Top == 0 &&
                       Right == 0 &&
                       Bottom == 0 &&
                       Width == 0 &&
                       Height == 0) return true;
                    return false;
                }
            }

            public static Rectangle ToRectangleDrawing(System.Drawing.Rectangle rectangle)
            {
                return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }
            public System.Windows.Size ToWindowsSize()
            {
                return new System.Windows.Size(Width,Height);
            }
            #region Constructor
            public Rectangle(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
            #endregion
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfo
        {
            public uint size;
            public Rect monitor;
            public Rect work;
            public uint flags;
        }

        public struct WindowInfo
        {
            public uint CbSize;
            public Rectangle RcWindow;
            public Rectangle RcClient;
            public uint DwStyle;
            public WindowStylesEx DwExStyle;
            public uint DwWindowStatus;
            public uint CxWindowBorders;
            public uint CyWindowBorders;
            public ushort AtomWindowType;
            public ushort WCreatorVersion;
            public WindowInfo(Boolean? filler)
                    : this()
            {
                CbSize = (UInt32)(Marshal.SizeOf(typeof(WindowInfo)));
            }
        }

        public struct WindowPlacement
        {
            public int Length;
            public int Flags;
            public int ShowCmd;
            public System.Drawing.Point PtMinPosition;
            public System.Drawing.Point PtMaxPosition;
            public System.Drawing.Rectangle RcNormalPosition;
        }

        [Flags]
        public enum WindowStylesEx : uint
        {
            /// <summary>
            /// Specifies that a window created with this style accepts drag-drop files.
            /// </summary>
            WsExAcceptfiles = 0x00000010,
            /// <summary>
            /// Forces a top-level window onto the taskbar when the window is visible.
            /// </summary>
            WsExAppwindow = 0x00040000,
            /// <summary>
            /// Specifies that a window has a border with a sunken edge.
            /// </summary>
            WsExClientedge = 0x00000200,
            /// <summary>
            /// Windows XP: Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
            /// </summary>
            WsExComposited = 0x02000000,
            /// <summary>
            /// Includes a question mark in the title bar of the window. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window.
            /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
            /// </summary>
            WsExContexthelp = 0x00000400,
            /// <summary>
            /// The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
            /// </summary>
            WsExControlparent = 0x00010000,
            /// <summary>
            /// Creates a window that has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
            /// </summary>
            WsExDlgmodalframe = 0x00000001,
            /// <summary>
            /// Windows 2000/XP: Creates a layered window. Note that this cannot be used for child windows. Also, this cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
            /// </summary>
            WsExLayered = 0x00080000,
            /// <summary>
            /// Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose horizontal origin is on the right edge. Increasing horizontal values advance to the left.
            /// </summary>
            WsExLayoutrtl = 0x00400000,
            /// <summary>
            /// Creates a window that has generic left-aligned properties. This is the default.
            /// </summary>
            WsExLeft = 0x00000000,
            /// <summary>
            /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
            /// </summary>
            WsExLeftscrollbar = 0x00004000,
            /// <summary>
            /// The window text is displayed using left-to-right reading-order properties. This is the default.
            /// </summary>
            WsExLtrreading = 0x00000000,
            /// <summary>
            /// Creates a multiple-document interface (MDI) child window.
            /// </summary>
            WsExMdichild = 0x00000040,
            /// <summary>
            /// Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
            /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
            /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
            /// </summary>
            WsExNoactivate = 0x08000000,
            /// <summary>
            /// Windows 2000/XP: A window created with this style does not pass its window layout to its child windows.
            /// </summary>
            WsExNoinheritlayout = 0x00100000,
            /// <summary>
            /// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
            /// </summary>
            WsExNoparentnotify = 0x00000004,
            /// <summary>
            /// Combines the WS_EX_CLIENTEDGE and WS_EX_WINDOWEDGE styles.
            /// </summary>
            WsExOverlappedwindow = WsExWindowedge | WsExClientedge,
            /// <summary>
            /// Combines the WS_EX_WINDOWEDGE, WS_EX_TOOLWINDOW, and WS_EX_TOPMOST styles.
            /// </summary>
            WsExPalettewindow = WsExWindowedge | WsExToolwindow | WsExTopmost,
            /// <summary>
            /// The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
            /// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
            /// </summary>
            WsExRight = 0x00001000,
            /// <summary>
            /// Vertical scroll bar (if present) is to the right of the client area. This is the default.
            /// </summary>
            WsExRightscrollbar = 0x00000000,
            /// <summary>
            /// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
            /// </summary>
            WsExRtlreading = 0x00002000,
            /// <summary>
            /// Creates a window with a three-dimensional border style intended to be used for items that do not accept user input.
            /// </summary>
            WsExStaticedge = 0x00020000,
            /// <summary>
            /// Creates a tool window; that is, a window intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
            /// </summary>
            WsExToolwindow = 0x00000080,
            /// <summary>
            /// Specifies that a window created with this style should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
            /// </summary>
            WsExTopmost = 0x00000008,
            /// <summary>
            /// Specifies that a window created with this style should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
            /// To achieve transparency without these restrictions, use the SetWindowRgn function.
            /// </summary>
            WsExTransparent = 0x00000020,
            /// <summary>
            /// Specifies that a window has a border with a raised edge.
            /// </summary>
            WsExWindowedge = 0x00000100
        }

        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SmtoNormal = 0x0,
            SmtoBlock = 0x1,
            SmtoAbortifhung = 0x2,
            SmtoNotimeoutifnothung = 0x8
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DisplayDevice
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;

            public DisplayDevice(int flags)
            {
                cb = 0;
                StateFlags = flags;
                DeviceName = new string((char)32, 32);
                DeviceString = new string((char)32, 128);
                DeviceID = new string((char)32, 128);
                DeviceKey = new string((char)32, 128);
                cb = Marshal.SizeOf(this);
            }
        }

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        /// <summary>
        /// Window handles (HWND) used for hWndInsertAfter @ SetWindowPos
        /// </summary>
        public static class HWND
        {
            public static IntPtr
                    NoTopMost = new IntPtr(-2),
                    TopMost = new IntPtr(-1),
                    Top = new IntPtr(0),
                    Bottom = new IntPtr(1);
        }

        [Flags]
        public enum SetWindowPosFlags : uint
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
            /// </summary>
            SWP_ASYNCWINDOWPOS = 0x4000,

            /// <summary>
            ///     Prevents generation of the WM_SYNCPAINT message.
            /// </summary>
            SWP_DEFERERASE = 0x2000,

            /// <summary>
            ///     Draws a frame (defined in the window's class description) around the window.
            /// </summary>
            SWP_DRAWFRAME = 0x0020,

            /// <summary>
            ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            /// </summary>
            SWP_FRAMECHANGED = 0x0020,

            /// <summary>
            ///     Hides the window.
            /// </summary>
            SWP_HIDEWINDOW = 0x0080,

            /// <summary>
            ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOACTIVATE = 0x0010,

            /// <summary>
            ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            /// </summary>
            SWP_NOCOPYBITS = 0x0100,

            /// <summary>
            ///     Retains the current position (ignores X and Y parameters).
            /// </summary>
            SWP_NOMOVE = 0x0002,

            /// <summary>
            ///     Does not change the owner window's position in the Z order.
            /// </summary>
            SWP_NOOWNERZORDER = 0x0200,

            /// <summary>
            ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            /// </summary>
            SWP_NOREDRAW = 0x0008,

            /// <summary>
            ///     Same as the SWP_NOOWNERZORDER flag.
            /// </summary>
            SWP_NOREPOSITION = 0x0200,

            /// <summary>
            ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            /// </summary>
            SWP_NOSENDCHANGING = 0x0400,

            /// <summary>
            ///     Retains the current size (ignores the cx and cy parameters).
            /// </summary>
            SWP_NOSIZE = 0x0001,

            /// <summary>
            ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
            /// </summary>
            SWP_NOZORDER = 0x0004,

            /// <summary>
            ///     Displays the window.
            /// </summary>
            SWP_SHOWWINDOW = 0x0040,

            // ReSharper restore InconsistentNaming
        }

        public class WinEventHook
        {
            public const uint WINEVENT_OUTOFCONTEXT = 0x0000; // Events are ASYNC

            public const uint WINEVENT_SKIPOWNTHREAD = 0x0001; // Don't call back for events on installer's thread

            public const uint WINEVENT_SKIPOWNPROCESS = 0x0002; // Don't call back for events on installer's process

            public const uint
                    WINEVENT_INCONTEXT =
                            0x0004; // Events are SYNC, this causes your dll to be injected into every process

            public const uint EVENT_MIN = 0x00000001;

            public const uint EVENT_MAX = 0x7FFFFFFF;

            public const uint EVENT_SYSTEM_SOUND = 0x0001;

            public const uint EVENT_SYSTEM_ALERT = 0x0002;

            public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

            public const uint EVENT_SYSTEM_MENUSTART = 0x0004;

            public const uint EVENT_SYSTEM_MENUEND = 0x0005;

            public const uint EVENT_SYSTEM_MENUPOPUPSTART = 0x0006;

            public const uint EVENT_SYSTEM_MENUPOPUPEND = 0x0007;

            public const uint EVENT_SYSTEM_CAPTURESTART = 0x0008;

            public const uint EVENT_SYSTEM_CAPTUREEND = 0x0009;

            public const uint EVENT_SYSTEM_MOVESIZESTART = 0x000A;

            public const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

            public const uint EVENT_SYSTEM_CONTEXTHELPSTART = 0x000C;

            public const uint EVENT_SYSTEM_CONTEXTHELPEND = 0x000D;

            public const uint EVENT_SYSTEM_DRAGDROPSTART = 0x000E;

            public const uint EVENT_SYSTEM_DRAGDROPEND = 0x000F;

            public const uint EVENT_SYSTEM_DIALOGSTART = 0x0010;

            public const uint EVENT_SYSTEM_DIALOGEND = 0x0011;

            public const uint EVENT_SYSTEM_SCROLLINGSTART = 0x0012;

            public const uint EVENT_SYSTEM_SCROLLINGEND = 0x0013;

            public const uint EVENT_SYSTEM_SWITCHSTART = 0x0014;

            public const uint EVENT_SYSTEM_SWITCHEND = 0x0015;

            public const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;

            public const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;

            public const uint EVENT_SYSTEM_DESKTOPSWITCH = 0x0020;

            public const uint EVENT_SYSTEM_END = 0x00FF;

            public const uint EVENT_OEM_DEFINED_START = 0x0101;

            public const uint EVENT_OEM_DEFINED_END = 0x01FF;

            public const uint EVENT_UIA_EVENTID_START = 0x4E00;

            public const uint EVENT_UIA_EVENTID_END = 0x4EFF;

            public const uint EVENT_UIA_PROPID_START = 0x7500;

            public const uint EVENT_UIA_PROPID_END = 0x75FF;

            public const uint EVENT_CONSOLE_CARET = 0x4001;

            public const uint EVENT_CONSOLE_UPDATE_REGION = 0x4002;

            public const uint EVENT_CONSOLE_UPDATE_SIMPLE = 0x4003;

            public const uint EVENT_CONSOLE_UPDATE_SCROLL = 0x4004;

            public const uint EVENT_CONSOLE_LAYOUT = 0x4005;

            public const uint EVENT_CONSOLE_START_APPLICATION = 0x4006;

            public const uint EVENT_CONSOLE_END_APPLICATION = 0x4007;

            public const uint EVENT_CONSOLE_END = 0x40FF;

            public const uint EVENT_OBJECT_CREATE = 0x8000; // hwnd ID idChild is created item

            public const uint EVENT_OBJECT_DESTROY = 0x8001; // hwnd ID idChild is destroyed item

            public const uint EVENT_OBJECT_SHOW = 0x8002; // hwnd ID idChild is shown item

            public const uint EVENT_OBJECT_HIDE = 0x8003; // hwnd ID idChild is hidden item

            public const uint EVENT_OBJECT_REORDER = 0x8004; // hwnd ID idChild is parent of zordering children

            public const uint EVENT_OBJECT_FOCUS = 0x8005; // hwnd ID idChild is focused item

            public const uint
                    EVENT_OBJECT_SELECTION =
                            0x8006; // hwnd ID idChild is selected item (if only one), or idChild is OBJID_WINDOW if complex

            public const uint EVENT_OBJECT_SELECTIONADD = 0x8007; // hwnd ID idChild is item added

            public const uint EVENT_OBJECT_SELECTIONREMOVE = 0x8008; // hwnd ID idChild is item removed

            public const uint EVENT_OBJECT_SELECTIONWITHIN = 0x8009; // hwnd ID idChild is parent of changed selected items

            public const uint EVENT_OBJECT_STATECHANGE = 0x800A; // hwnd ID idChild is item w/ state change

            public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B; // hwnd ID idChild is moved/sized item

            public const uint EVENT_OBJECT_NAMECHANGE = 0x800C; // hwnd ID idChild is item w/ name change

            public const uint EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D; // hwnd ID idChild is item w/ desc change

            public const uint EVENT_OBJECT_VALUECHANGE = 0x800E; // hwnd ID idChild is item w/ value change

            public const uint EVENT_OBJECT_PARENTCHANGE = 0x800F; // hwnd ID idChild is item w/ new parent

            public const uint EVENT_OBJECT_HELPCHANGE = 0x8010; // hwnd ID idChild is item w/ help change

            public const uint EVENT_OBJECT_DEFACTIONCHANGE = 0x8011; // hwnd ID idChild is item w/ def action change

            public const uint EVENT_OBJECT_ACCELERATORCHANGE = 0x8012; // hwnd ID idChild is item w/ keybd accel change

            public const uint EVENT_OBJECT_INVOKED = 0x8013; // hwnd ID idChild is item invoked

            public const uint EVENT_OBJECT_TEXTSELECTIONCHANGED = 0x8014; // hwnd ID idChild is item w? test selection change

            public const uint EVENT_OBJECT_CONTENTSCROLLED = 0x8015;

            public const uint EVENT_SYSTEM_ARRANGMENTPREVIEW = 0x8016;

            public const uint EVENT_OBJECT_END = 0x80FF;

            public const uint EVENT_AIA_START = 0xA000;

            public const uint EVENT_AIA_END = 0xAFFF;
        }
        #endregion
    }
}