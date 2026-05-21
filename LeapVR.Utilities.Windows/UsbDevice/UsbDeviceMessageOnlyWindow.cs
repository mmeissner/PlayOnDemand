#region Licence
/****************************************************************
 *  Filename: UsbDeviceMessageOnlyWindow.cs
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
using System.Threading;
using System.Windows.Forms;
using LeapVR.Shared.Lib.Win.WinApi;
using LeapVR.Shared.Lib.Win.WinApi.Win32;

namespace LeapVR.Utilities.Windows.WindowsMessages
{
    /// <summary>
    /// UsbDeviceMessageOnlyWindow is class inheriting from <see cref="NativeWindow"/>, that does not contains any drawing logic.
    /// It's main purpose is to listen to and handle Window mesages.
    /// </summary>
    public sealed class UsbDeviceMessageOnlyWindow : NativeWindow, IDisposable
    {
        #region Properties & Fields    

        public Predicate<MessageData> MessageFilter { get; }

        private const int DbtDevtypDeviceinterface = 5;
        private static readonly Guid GuidDevInterfaceUsbDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices

        private int _isDisposed; // 0 = false, 1 = true
        private IntPtr _notificationHandle;

        public EventHandler<MessageData> MessageArrived;

        #endregion Properties & Fields

        #region Constructors

        /// <summary>
        /// Creates new instance of UsbDeviceMessageOnlyWindow. Must be called from the UI thread.
        /// </summary>
        public UsbDeviceMessageOnlyWindow(Predicate<MessageData> messageFilter = null)
        {
            MessageFilter = messageFilter;

            CreateHandle(new CreateParams());
            RegisterUsbDeviceNotification();
        }

        ~UsbDeviceMessageOnlyWindow()
        {
            Dispose(false);
        }

        #endregion Constructors

        #region Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void WndProc(ref Message msg)
        {
            try
            {
                var messageData = new MessageData
                {
                    HWnd = msg.HWnd,
                    LParam = msg.LParam,
                    Msg = msg.Msg,
                    Result = msg.Result,
                    WParam = msg.WParam,
                };

                if (MessageFilter != null && !MessageFilter(messageData))
                {
                    // Don't process messages that are cut-out by filter
                    return;
                }

                MessageArrived?.Invoke(this, messageData);
            }
            finally
            {
                base.WndProc(ref msg);
            }
        }

        private void RegisterUsbDeviceNotification()
        {
            DevBroadcastDeviceinterface dbi = new DevBroadcastDeviceinterface
            {
                DeviceType = DbtDevtypDeviceinterface,
                Reserved = 0,
                ClassGuid = GuidDevInterfaceUsbDevice,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);

            _notificationHandle = User32.RegisterDeviceNotification(Handle, buffer, 0);
        }

        private void Dispose(bool disposing)
        {
            int wasDisposed = Interlocked.Exchange(ref _isDisposed, 1);
            if (wasDisposed == 1)
            {
                return;
            }

            UnregisterUsbDeviceNotification();

            if (disposing)
            {
                // dispose managed
            }
        }

        private void UnregisterUsbDeviceNotification()
        {
            User32.UnregisterDeviceNotification(_notificationHandle);
        }

        #endregion Methods
    }

    public class MessageData
    {
        public IntPtr HWnd { get; internal set; }
        public IntPtr LParam { get; internal set; }
        public int Msg { get; internal set; }
        public IntPtr Result { get; internal set; }
        public IntPtr WParam { get; internal set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DevBroadcastDeviceinterface
    {
        internal int Size;
        internal int DeviceType;
        internal int Reserved;
        internal Guid ClassGuid;
        internal short Name;
    }
}
