#region Licence
/****************************************************************
 *  Filename: User32WindowUtil.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using NLog;

namespace LeapVR.Shared.Lib.Win.WinApi
{
    public static class User32WindowUtil
    {
               private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public static List<IWindowData> GetWindowData(Process process,IWindowFilter windowFilter)
        {
            try
            {
                if (process.HasExited)
                {
                    return null;
                }

                List<IWindowData> result = new List<IWindowData>();
                uint threadId;
                foreach (ProcessThread thread in process.Threads)
                {
                    threadId = (uint)thread.Id;
                    User32.EnumThreadWindows(threadId, CheckEnumThreadWindow, IntPtr.Zero); // TODO [RM]: check if can change from enum all windows to FindWindow
                }

                return result;

                //Inline Function
                bool CheckEnumThreadWindow(IntPtr hwnd, IntPtr lParam)
                {
                    var sb = new StringBuilder(64);
                    User32.GetClassName(hwnd, sb, sb.Capacity);
                    var className = sb.ToString();

                    if (CheckFilterMatch(process,windowFilter, hwnd, className))
                    {
                        var retval = GetWindowData(process,hwnd,threadId, className);
                        if(retval != null) result.Add(retval);
                    }

                    return true;
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, "Exception during GetWindowData");
                throw;
            }
        }


        private static IWindowData GetWindowData(Process process,IntPtr hwnd,uint threadId, string windowClassName)
        {
            try
            {
                Logger.Debug("Trying to get Window Data");

                var wnd = new HandleRef(process, hwnd);
                var success = User32.GetWindowRect(wnd, out var rect);
                var stringBuilder = new StringBuilder(500);
                User32.GetWindowText(hwnd, stringBuilder, 500);

                if (success)
                {
                    Logger.Debug("GetWindowData succeeded");
                    return new WindowData
                           {
                                   WindowClassName = windowClassName,
                                   WindowTitle = stringBuilder.ToString(),
                                   HWnd = wnd.Handle,
                                   ThreadId = threadId,
                                   PosX = rect.Left,
                                   PosY = rect.Top,
                                   Width = rect.Width,
                                   Height = rect.Height,
                           };
                }
                else { Logger.Debug("GetWindowData failed"); }

                return null;
            }
            catch(Exception e)
            {
                Logger.Debug(e, "GetWindowData encountered Exception");
                throw;
            }
        }

        
        private static bool CheckFilterMatch(Process process,IWindowFilter windowFilter, IntPtr hwndToCheck, string classNameToCheck)
        {
            try
            {
                switch (windowFilter)
                {
                    case IWindowClassNameFilter classNameFilter:
                        return classNameToCheck == classNameFilter.ClassName;

                    case IMainWindowFilter _:
                        return process.MainWindowHandle != IntPtr.Zero && hwndToCheck == process.MainWindowHandle;
                }

                throw new InvalidOperationException($"Unsupported windowFilter type; GetType = `{windowFilter?.GetType()}`.");
            }
            catch(Exception e)
            {
                Logger.Debug(e, "CheckFilterMatch encountered Exception");
                throw;
            }
        }
    }

    public class WindowClassNameFilter : IWindowClassNameFilter
    {
        public string ClassName { get; set; }
    }
    public class MainWindowFilter : IMainWindowFilter
    {
        //
    }
    public interface IWindowClassNameFilter : IWindowFilter
    {
        string ClassName { get; }
    }
    public interface IWindowFilter
    {
    }
    public interface IMainWindowFilter : IWindowFilter
    {
    }

    public interface IWindowData
    {
        string WindowClassName { get; }
        string WindowTitle { get; }
        IntPtr HWnd { get; }
        UInt32 ThreadId { get; }
        int PosX { get; }
        int PosY { get; }

        int Width { get; }
        int Height { get; }
    }
    class WindowData : IWindowData
    {
        #region Properties & Fields

        public string WindowClassName { get; internal set; }
        public string WindowTitle { get; internal set; }
        public IntPtr HWnd { get;internal set; }
        public uint ThreadId { get;internal set; }

        public int PosX { get; internal set; }
        public int PosY { get; internal set; }

        public int Width { get; internal set; }
        public int Height { get; internal set; }

        #endregion Properties & Fields

        #region Constructors

        internal WindowData()
        {
            //
        }

        #endregion Constructors

        #region Methods

        //

        #endregion Methods
    }
}
