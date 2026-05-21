#region Licence
/****************************************************************
 *  Filename: RunningObjectTableEntry.cs
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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace LeapVR.Shell.Controllers.System
{
    public class RunningObjectTableEntry : IDisposable
    {
        private int cookie;
        private IRunningObjectTable rot = null;
        private IMoniker monkey = null;

        private RunningObjectTableEntry() { }

        /// <summary>
        /// Creates a new entry for the given object
        /// </summary>
        /// <param name="obj">Object to make an entry for. Only one object per class should ever be registered.</param>
        public RunningObjectTableEntry(object obj)
        {
            int hr = GetRunningObjectTable(0, out rot);
            if (hr != 0)
            {
                throw new COMException("Could not receive running object table!", hr);
            }

            Guid clsid = obj.GetType().GUID;
            hr = CreateClassMoniker(ref clsid, out monkey);
            if (hr != 0)
            {
                Marshal.ReleaseComObject(rot);
                throw new COMException("Could not create moniker for CLSID/IID \"" + clsid + "\"!", hr);
            }

            cookie = rot.Register(0x01, obj, monkey);   //weak reference, but allow any user
        }

        [DllImport("ole32.dll", ExactSpelling = true)]
        private static extern int GetRunningObjectTable([MarshalAs(UnmanagedType.U4)] int reserved, out IRunningObjectTable pprot);

        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int CreateClassMoniker([In] ref Guid g, [Out] out IMoniker ppmk);

        #region IDisposable Members

        /// <summary>
        /// De-registers the object and class from the Running Object Table
        /// </summary>
        public void Dispose()
        {
            Marshal.ReleaseComObject(monkey);
            rot.Revoke(cookie);
            Marshal.ReleaseComObject(rot);
        }

        #endregion
    }
}
