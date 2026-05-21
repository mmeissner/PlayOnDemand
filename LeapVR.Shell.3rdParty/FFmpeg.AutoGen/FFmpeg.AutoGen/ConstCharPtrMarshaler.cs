#region Licence
/****************************************************************
 *  Filename: ConstCharPtrMarshaler.cs
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

namespace FFmpeg.AutoGen
{
    internal class ConstCharPtrMarshaler : ICustomMarshaler
    {
        public object MarshalNativeToManaged(IntPtr pNativeData) => Marshal.PtrToStringAnsi(pNativeData);

        public IntPtr MarshalManagedToNative(object managedObj) => IntPtr.Zero;

        public void CleanUpNativeData(IntPtr pNativeData)
        {
        }

        public void CleanUpManagedData(object managedObj)
        {
        }

        public int GetNativeDataSize() => IntPtr.Size;

        private static readonly ConstCharPtrMarshaler Instance = new ConstCharPtrMarshaler();

        public static ICustomMarshaler GetInstance(string cookie) => Instance;
    }
}