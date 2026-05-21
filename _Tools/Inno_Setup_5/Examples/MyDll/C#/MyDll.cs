#region Licence
/****************************************************************
 *  Filename: MyDll.cs
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
using RGiesecke.DllExport;

namespace Mydll
{
    public class Mydll
    {
      [DllExport("MyDllFunc", CallingConvention=CallingConvention.StdCall)]
      public static void MyDllFunc(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] string text, [MarshalAs(UnmanagedType.LPStr)] string caption, int options)
      {
        MessageBox(hWnd, text, caption, options);
      }

      [DllExport("MyDllFuncW", CallingConvention=CallingConvention.StdCall)]
      public static void MyDllFuncW(IntPtr hWnd, string text, string caption, int options)
      {
        MessageBox(hWnd, text, caption, options);
      }

      [DllImport("user32.dll", CharSet=CharSet.Auto)]
      static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);
    }
}
