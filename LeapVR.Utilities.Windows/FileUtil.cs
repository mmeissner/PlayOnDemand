#region Licence
/****************************************************************
 *  Filename: FileUtil.cs
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
using System.IO;
using System.Linq;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using NLog;

namespace LeapVR.Utilities.Windows
{
    public class FileUtil 
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public string[] GetFilesOrderedByNumeration(string path, string searchPatern, string replaceString)
        {
            if (!Directory.Exists(path))
            {
                Logger.Info("Directory :" + path + " does not exist!");
                return null;
            }
            var numberFilePath = new Dictionary<int, string>();
            var files = Directory.GetFiles(path, searchPatern);
            if (!files.Any())
            {
                Logger.Info("Directory :" + path + " does not contain any files!");
                return null;
            }
            foreach (var filepathname in files)
            {
                var splitname = Path.GetFileNameWithoutExtension(filepathname).Replace(replaceString, "");
                if (!String.IsNullOrEmpty(splitname))
                {
                    if (!Int32.TryParse(splitname, out var number)) continue;
                    if (numberFilePath.ContainsKey(number)) continue;
                    numberFilePath.Add(number, filepathname);
                }
            }
            if (!numberFilePath.Any())
            {
                Logger.Info("Directory :" + path + " does not contain any files that matches the right pattern!");
                return null;
            }
            var list = numberFilePath.Keys.ToList();
            list.Sort();
            return list.Select(i => numberFilePath[i]).ToArray();
        }
        public string ReadFile(string filePath)
        {
            try
            {
                using (var streamReader = new StreamReader(filePath))
                {
                    string content;
                    try
                    {
                        content = streamReader.ReadToEnd();
                    }
                    finally
                    {
                        streamReader.Close();
                    }
                    return content;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"Error during reading of file: {filePath}");
                return null;
            }
        }
        public bool WriteFile(string filePath, string fileContent)
        {
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                using (var streamWriter = new StreamWriter(filePath))
                {
                    try
                    {
                        streamWriter.Write(fileContent);
                    }
                    finally
                    {
                        streamWriter.Close();
                    }
                    return true;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"Error during writing of file: {filePath}");
                return false;
            }
        }
        public List<string> GetAllPipes()
        {

            var namedPipes = new List<string>();

            var ptr = Kernel32.FindFirstFile(@"\\.\pipe\*", out var lpFindFileData);
            namedPipes.Add(lpFindFileData.cFileName);
            while (Kernel32.FindNextFile(ptr, out lpFindFileData))
            {
                namedPipes.Add(lpFindFileData.cFileName);
            }
            Kernel32.FindClose(ptr);

            namedPipes.Sort();

            return namedPipes;
        }

        public LogFileMonitorUtil GetMonitor(string logfilePathName, string lineDelimiter)
        {
            return new LogFileMonitorUtil(logfilePathName, lineDelimiter);
        }

    }
}
