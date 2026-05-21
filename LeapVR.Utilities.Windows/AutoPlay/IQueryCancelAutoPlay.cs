#region Licence
/****************************************************************
 *  Filename: IQueryCancelAutoPlay.cs
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
using System.Runtime.InteropServices;

namespace LeapVR.Utilities.Windows.AutoPlay
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("DDEFE873-6997-4e68-BE26-39B633ADBE12")]
    
    public interface IQueryCancelAutoPlay
    {
        [PreserveSig()]
        int AllowAutoPlay(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            [MarshalAs(UnmanagedType.U4)] int dwContentType,
            [MarshalAs(UnmanagedType.LPWStr)] string pszLabel,
            [MarshalAs(UnmanagedType.U4)] int dwSerialNumber);
    }
}
