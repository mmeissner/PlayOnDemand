#region Licence
/****************************************************************
 *  Filename: HardwareDeviceUtil.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.Hardware;
using LeapVR.Shell.Modules.Interfaces.Hardware;
using LeapVR.Shell.Modules.Properties;
using NLog;

namespace LeapVR.Shell.Modules.Hardware
{
    public class HardwareDeviceUtil : IHardwareDeviceUtil, IDisposable
    {
        #region private Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _devconFilePath;
        #endregion

        #region Constructor
        public HardwareDeviceUtil()
        {
            _devconFilePath = CreateDevConExe();
        }
        #endregion

        #region Public Methods
        public DeviceData GetDeviceState(string deviceId)
        {
            Logger.Debug("GetDeviceState for device with deviceId {0}", deviceId);
            var searchId = deviceId;
            if (!searchId.EndsWith("*")) searchId = $"{searchId}*";

            string args = "status " + "@" + @"""" + searchId + @"""";
            var start = GetDevConProcessStartInfo(_devconFilePath, args);
            var process = new Process { StartInfo = start };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            if (String.IsNullOrEmpty(output)) throw new ArgumentException("Output from DevCon can not be null!");
            if (output.StartsWith("No matching devices found."))
            {
                return new DeviceData { DeviceId = deviceId, Name = "", State = DeviceState.NotPresent };
            }
            var retval = DevConToDeviceInfo(output.Split());
            if (retval.HasValue)
            {
                Logger.Debug("GetDeviceState is {1} for with deviceId {0}", retval, deviceId);
                return retval.Value;
            }
            Logger.Error("Output String from DevCon was null or empty!");
            Logger.Info("GetDeviceState is {0} for with deviceId {0}", DeviceState.NotPresent);
            throw new ArgumentException("Unexpected DevCon Output!");
        }
        public List<DeviceData> GetDeviceStates()
        {
            var retval = new List<DeviceData>();
            Logger.Debug("GetDeviceStates for local machine");
            string args = "status *";
            var start = GetDevConProcessStartInfo(_devconFilePath, args);
            var process = new Process { StartInfo = start };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var lines = output.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < lines.Length && i + 4 <= lines.Length; i = i + 6)
            {
                var test = new[] { lines[i], lines[i + 2], lines[i + 4] };

                //Special Case for Root Node
                if (lines[i].StartsWith(@"HTREE\ROOT\0"))
                {
                    retval.Add(
                        new DeviceData()
                        {
                            DeviceId = lines[i],
                            Name = @"Root Node[HTREE\ROOT\0]",
                            State = DeviceState.Enabled
                        });
                    i = i - 2;
                    continue;
                }
                var device = DevConToDeviceInfo(test);
                if (!device.HasValue)
                {
                    Logger.Error("Failed to Detect Device");
                }
                else retval.Add(device.Value);
            }
            return retval;
        }
        public bool EnableDevice(string deviceId)
        {
            Logger.Debug("Enable Device with deviceId {0}", deviceId);
            string args = "enable " + @"@""" + @deviceId + @"""";
            var start = GetDevConProcessStartInfo(_devconFilePath, args);
            start.RedirectStandardOutput = true;
            var process = new Process { StartInfo = start };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var retval = !string.IsNullOrEmpty(output) && output.Contains("are enabled");
            Logger.Debug("Enable for device Id returned {0}", retval);
            return retval;
        }
        public bool DisableDevice(string deviceId)
        {
            Console.WriteLine("Disable Device with deviceId {0}", deviceId);
            string args = "disable " + @"@""" + @deviceId + @"""";
            var start = GetDevConProcessStartInfo(_devconFilePath, args);
            start.RedirectStandardOutput = true;
            var process = new Process { StartInfo = start };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var retval = !String.IsNullOrEmpty(output) && output.Contains("disabled");
            Console.WriteLine("Disable for device Id returned {0}", retval);
            return retval;
        }
        public bool RestartDevice(string deviceId)
        {
            Console.WriteLine("Restart Device with deviceId {0}", deviceId);
            string args = "restart " + @"@""" + @deviceId + @"""";
            var start = GetDevConProcessStartInfo(_devconFilePath, args);
            start.RedirectStandardOutput = true;
            var process = new Process { StartInfo = start };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var retval = !string.IsNullOrEmpty(output) && output.Contains("restarted");
            Console.WriteLine("Restart for device Id returned {0}", retval);
            return retval;
        }
        #endregion

        #region Private Methods
        private static DeviceData? DevConToDeviceInfo(string[] inputString)
        {
            if (inputString.Length != 3 || !inputString[0].Contains(@"\")) throw new NotSupportedException();
            if (!inputString[1].StartsWith("    Name:"))
            {
                throw new NotSupportedException();
            }
            var state = DevConOutputToDeviceState(inputString[2]);
            if (!state.HasValue) return null;
            return new DeviceData()
            {
                DeviceId = inputString[0],
                Name = inputString[1].TrimStart("    Name:".ToCharArray()),
                State = state.Value
            };
        }
        private static DeviceState? DevConOutputToDeviceState(string output)
        {
            DeviceState? retval = DeviceState.Unknown;
            if (!String.IsNullOrEmpty(output))
            {
                if (output.Contains("Driver is running."))
                {
                    retval = DeviceState.Enabled;
                }
                else if (output.Contains("Device is disabled."))
                {
                    retval = DeviceState.Disabled;
                }
                else if (output.Contains("Device is currently stopped."))
                {
                    Console.WriteLine(@"Device is currently stopped");
                    retval = DeviceState.Disabled;
                }
                else if (output.Contains("No matching devices found"))
                {
                    Console.WriteLine(@"No matching devices found");
                    retval = DeviceState.NotPresent;
                }
            }
            else
            {
                Logger.Warn("Output String from DevCon was null or empty!");
                return null;
            }
            return retval;
        }
        private static ProcessStartInfo GetDevConProcessStartInfo(string filepath, string arguments)
        {
            var start = new ProcessStartInfo();
            start.FileName = filepath;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.Arguments = arguments;
            start.CreateNoWindow = true;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            return start;
        }

        private static string CreateDevConExe()
        {
            Logger.Debug("Creating DevCon");
            try
            {
                string tempExeName = Path.Combine(Path.GetTempPath(), "HardwareDeviceUtil.exe");
                using (FileStream fsDst = new FileStream(tempExeName, FileMode.CreateNew, FileAccess.Write))
                {
                    byte[] bytes = Resources.DevCon;

                    fsDst.Write(bytes, 0, bytes.Length);
                }
                return tempExeName;
            }
            catch(Exception e)
            {
                Logger.Error(e,"Error during creation of DevCon File");
                return null;
            }
        }

        ~HardwareDeviceUtil()
        {
            ReleaseUnmanagedResources();
        }
        #endregion

        private void ReleaseUnmanagedResources()
        {
            try
            {
                var fileInfo = new FileInfo(_devconFilePath);
                if (fileInfo.Exists) fileInfo.Delete();
            }
            catch(Exception e)
            {
                Logger.Error(e,"Deconstructor failed to delete DevCon");
            }
        }
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}
