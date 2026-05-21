#region Licence
/****************************************************************
 *  Filename: Kernel32.cs
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
    public static class Kernel32
    {
        #region Kernel32 Methods
        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(out SystemInfo lpSystemInfo);

        [DllImport("Kernel32.dll")]
        public static extern bool GetFileInformationByHandle(IntPtr hFile, out ByHandleFileInformation lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(string lpFileName, EFileAccess dwDesiredAccess, EFileShare dwShareMode, IntPtr lpSecurityAttributes, ECreationDisposition dwCreationDisposition, EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(string fileName, string existingFileName, IntPtr lpSecurityAttributes);

        [DllImport("Kernel32.dll")]
        public static extern bool Beep(UInt32 frequency, UInt32 duration);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, Int32 dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] lpBuffer, Int32 nSize, out Int32 lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer, UIntPtr dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, UIntPtr dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, UInt32 dwFreeType);

        [DllImport("kernel32.dll")]
        public static extern bool GlobalMemoryStatusEx(Memorystatusex buffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA
           lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FindClose(IntPtr hFindFile);

        // Pinvoke for API function
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);

        #endregion

        #region ENUM & Structs & Classes
        public class ProcessRights
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

            public const UInt32 AllAccess = StandardRightsRequired | Synchronize | 0xFFF;
        }

        public class MemoryProtection
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

        public class MemAllocationType
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

        [Flags]
        public enum EFileAccess : uint
        {
            GenericRead = 0x80000000,
            GenericWrite = 0x40000000,
            GenericExecute = 0x20000000,
            GenericAll = 0x10000000,
        }

        [Flags]
        public enum EFileShare : uint
        {
            None = 0x00000000,
            Read = 0x00000001,
            Write = 0x00000002,
            Delete = 0x00000004,
        }

        public enum ECreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5,
        }

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly = 0x00000001,
            Hidden = 0x00000002,
            System = 0x00000004,
            Directory = 0x00000010,
            Archive = 0x00000020,
            Device = 0x00000040,
            Normal = 0x00000080,
            Temporary = 0x00000100,
            SparseFile = 0x00000200,
            ReparsePoint = 0x00000400,
            Compressed = 0x00000800,
            Offline = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted = 0x00004000,
            WriteThrough = 0x80000000,
            Overlapped = 0x40000000,
            NoBuffering = 0x20000000,
            RandomAccess = 0x10000000,
            SequentialScan = 0x08000000,
            DeleteOnClose = 0x04000000,
            BackupSemantics = 0x02000000,
            PosixSemantics = 0x01000000,
            OpenReparsePoint = 0x00200000,
            OpenNoRecall = 0x00100000,
            FirstPipeInstance = 0x00080000
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfo
        {
            public ProcessorInfoUnion uProcessorInfo;
            public uint dwPageSize;
            public uint lpMinimumApplicationAddress;
            public uint lpMaximumApplicationAddress;
            public uint dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public uint dwProcessorLevel;
            public uint dwProcessorRevision;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ProcessorInfoUnion
        {
            [FieldOffset(0)]
            public uint dwOemId;
            [FieldOffset(0)]
            public ushort wProcessorArchitecture;
            [FieldOffset(2)]
            public ushort wReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ByHandleFileInformation
        {
            public UInt32 dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public UInt32 dwVolumeSerialNumber;
            public UInt32 nFileSizeHigh;
            public UInt32 nFileSizeLow;
            public UInt32 nNumberOfLinks;
            public UInt32 nFileIndexHigh;
            public UInt32 nFileIndexLow;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Memorystatusex
        {
            public Int32 Length;
            public Int32 MemoryLoad;
            public UInt64 TotalPhysical;
            public UInt64 AvailablePhysical;
            public UInt64 TotalPageFile;
            public UInt64 AvailablePageFile;
            public UInt64 TotalVirtual;
            public UInt64 AvailableVirtual;
            public UInt64 AvailableExtendedVirtual;

            public Memorystatusex() { Length = Marshal.SizeOf(this); }

            private void StopTheCompilerComplaining()
            {
                Length = 0;
                MemoryLoad = 0;
                TotalPhysical = 0;
                AvailablePhysical = 0;
                TotalPageFile = 0;
                AvailablePageFile = 0;
                TotalVirtual = 0;
                AvailableVirtual = 0;
                AvailableExtendedVirtual = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }
        #endregion
    }
}
