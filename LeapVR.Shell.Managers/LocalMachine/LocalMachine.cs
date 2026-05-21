#region Licence
/****************************************************************
 *  Filename: LocalMachine.cs
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
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Station;
using Microsoft.VisualBasic.Devices;
using NLog;
using LeapVR.Shared.Lib.Win;

namespace LeapVR.Shell.Managers.LocalMachine
{
    public class LocalMachine : ILocalMachine
    {
        private readonly Lazy<LocalMachineData> _lazyLocalMachineData = new Lazy<LocalMachineData>(ValueFactory());
        private static Func<LocalMachineData> ValueFactory()
        {
            return () =>
                   {
                       var data = new LocalMachineData();
                       data.InitializeData();
                       return data;
                   };
        }

        public Version SoftwareVersion => VersionProvider.SoftwareVersion;
        public string VBoxFingerprint => _lazyLocalMachineData.Value.VBoxFingerprint;
        public string CpuDetails => _lazyLocalMachineData.Value.CpuDetails;
        public string VgaDetails => _lazyLocalMachineData.Value.VgaDetails;
        public string RamDetails => _lazyLocalMachineData.Value.RamDetails;

        class LocalMachineData
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            public string VBoxFingerprint { get; private set; }
            public string CpuDetails { get; private set; }
            public string VgaDetails { get; private set; }
            public string RamDetails { get; private set; }

            public void InitializeData()
            {
                var cpu = Wmi.Query("SELECT Name, ProcessorId FROM Win32_Processor").First();
                var vga = Wmi.Query("SELECT Name FROM Win32_VideoController").First();
                ulong totalRam = new ComputerInfo().TotalPhysicalMemory;
                var disks = Wmi.Query("SELECT Model, SerialNumber, Size FROM Win32_DiskDrive");

                // calculate hardware fingerprint
                var sb = new StringBuilder();
                sb.Append(cpu["Name"]).Append(cpu["ProcessorId"]);
                sb.Append(vga["Name"]);
                sb.Append(totalRam.ToString());
                foreach(var disk in disks.OrderBy(q => q["Model"]))
                {
                    sb.Append(disk["Model"]).Append(disk["SerialNumber"]).Append(disk["Size"]);
                }

                var bulbBytes = Encoding.Unicode.GetBytes(sb.ToString());
                var md5Bytes = MD5.Create().ComputeHash(bulbBytes);

                //generate string in format XXXX-XXXX-XXXX-XXXX
                int bytesCnt = 8;
                int groupCnt = 2;
                var md5BytesStr = Array.ConvertAll(md5Bytes, x => x.ToString("X2"));
                sb = new StringBuilder();
                for(int i = 0; i < bytesCnt; i++)
                {
                    sb.Append(md5BytesStr[i]);
                    if(i % groupCnt == groupCnt - 1 && i != bytesCnt - 1)
                    {
                        sb.Append("-");
                    }
                }

                var fingerprint = sb.ToString();
                VBoxFingerprint = fingerprint;
                CpuDetails = cpu["Name"].ToString();
                VgaDetails = vga["Name"].ToString();
                RamDetails = QuickLeap.ToDiskSize(totalRam, 0);
                Logger.Info(
                        $"LocalMachine: CpuDetails={CpuDetails}, VgaDetails={VgaDetails}, RamDetails={RamDetails},VBoxFingerprint={fingerprint}");
            }
        }
    }
}