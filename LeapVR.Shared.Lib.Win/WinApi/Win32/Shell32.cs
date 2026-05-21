#region Licence
/****************************************************************
 *  Filename: Shell32.cs
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
using System.Runtime.InteropServices;

namespace LeapVR.Shared.Lib.Win.WinApi.Win32
{
    public static class Shell32
    {
        #region Native Functions
        [DllImport("shell32.dll")]
        public static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref Notifyicondata pnid);

        [DllImport("Shell32", SetLastError = true)]
        public static extern int Shell_NotifyIconGetRect(ref Notifyiconidentifier identifier, out User32.Rectangle iconLocation);
        #endregion

        public static bool CanGetNotifyIconIdentifier(User32.ToolBarButtonData traydata, out Notifyiconidentifier identifier)
        {
            // You can either use uID + hWnd or a GUID, but GUID is new to Win7 and isn't used by NotifyIcon anyway.
            identifier = new Notifyiconidentifier();
            identifier.UID = traydata.uID;
            identifier.HWnd = traydata.hwnd;
            if (identifier.HWnd == null || identifier.HWnd == IntPtr.Zero)
                return false;
            identifier.CbSize = (uint)Marshal.SizeOf(identifier);
            return true;
        }

        #region ENUM & Structs
        public struct Notifyicondata
        {
            /// <summary>
            /// Size of this structure, in bytes. 
            /// </summary>
            public int CbSize;

            /// <summary>
            /// Handle to the window that receives notification messages associated with an icon in the 
            /// taskbar status area. The Shell uses hWnd and uID to identify which icon to operate on 
            /// when Shell_NotifyIcon is invoked. 
            /// </summary>
            public IntPtr Hwnd;

            /// <summary>
            /// Application-defined identifier of the taskbar icon. The Shell uses hWnd and uID to identify 
            /// which icon to operate on when Shell_NotifyIcon is invoked. You can have multiple icons 
            /// associated with a single hWnd by assigning each a different uID. 
            /// </summary>
            public int UID;

            /// <summary>
            /// Flags that indicate which of the other members contain valid data. This member can be 
            /// a combination of the NIF_XXX constants.
            /// </summary>
            public int UFlags;

            /// <summary>
            /// Application-defined message identifier. The system uses this identifier to send 
            /// notifications to the window identified in hWnd. 
            /// </summary>
            public int UCallbackMessage;

            /// <summary>
            /// Handle to the icon to be added, modified, or deleted. 
            /// </summary>
            public IntPtr HIcon;

            /// <summary>
            /// String with the text for a standard ToolTip. It can have a maximum of 64 characters including 
            /// the terminating NULL. For Version 5.0 and later, szTip can have a maximum of 
            /// 128 characters, including the terminating NULL.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string SzTip;

            /// <summary>
            /// State of the icon. 
            /// </summary>
            public int DwState;

            /// <summary>
            /// A value that specifies which bits of the state member are retrieved or modified. 
            /// For example, setting this member to NIS_HIDDEN causes only the item's hidden state to be retrieved. 
            /// </summary>
            public int DwStateMask;

            /// <summary>
            /// String with the text for a balloon ToolTip. It can have a maximum of 255 characters. 
            /// To remove the ToolTip, set the NIF_INFO flag in uFlags and set szInfo to an empty string. 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string SzInfo;

            /// <summary>
            /// NOTE: This field is also used for the Timeout value. Specifies whether the Shell notify 
            /// icon interface should use Windows 95 or Windows 2000 
            /// behavior. For more information on the differences in these two behaviors, see 
            /// Shell_NotifyIcon. This member is only employed when using Shell_NotifyIcon to send an 
            /// NIM_VERSION message. 
            /// </summary>
            public int UVersion;

            /// <summary>
            /// String containing a title for a balloon ToolTip. This title appears in boldface 
            /// above the text. It can have a maximum of 63 characters. 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string SzInfoTitle;

            /// <summary>
            /// Adds an icon to a balloon ToolTip. It is placed to the left of the title. If the 
            /// szTitleInfo member is zero-length, the icon is not shown. See 
            /// <see cref="BalloonIconStyle">RMUtils.WinAPI.Structs.BalloonIconStyle</see> for more
            /// information.
            /// </summary>
            public int DwInfoFlags;
        }

        /// <summary>
        /// Contains information used by Shell_NotifyIconGetRectangle to identify the icon for which to retrieve the bounding rectangle.
        /// </summary>
        /// <remarks>
        /// The icon can be identified to Shell_NotifyIconGetRectangle through this structure in two ways:
        /// guidItem alone (recommended)
        /// hWnd plus uID
        /// If guidItem is used, hWnd and uID are ignored.
        /// </remarks>
        public struct Notifyiconidentifier
        {
            /// <summary>
            /// Size of this structure, in bytes.
            /// </summary>
            public uint CbSize;
            /// <summary>
            /// A handle to the parent window used by the notification's callback function. For more information, see the hWnd member of the NOTIFYICONDATA structure.
            /// </summary>
            public IntPtr HWnd;
            /// <summary>
            /// The application-defined identifier of the notification icon. Multiple icons can be associated with a single hWnd, each with their own uID.
            /// </summary>
            public uint UID;
            /// <summary>
            /// A registered GUID that identifies the icon.
            /// </summary>
            public Guid GuidItem;
        }
        #endregion
    }
}
