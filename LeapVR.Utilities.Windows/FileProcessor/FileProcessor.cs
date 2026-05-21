#region Licence
/****************************************************************
 *  Filename: FileProcessor.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-1-23
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
using System.IO;
using System.Linq;
using LeapVR.Shared.Lib.Win.WinApi;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using NLog;

namespace LeapVR.Utilities.Windows.FileProcessor
{
    public static class FileProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static string[] GetFilesOrderedByNumeration(string path, string searchPatern, string replaceString)
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

        /// <summary>
        /// Reads the filecontent.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>file content</returns>
        public static string ReadFile(string filePath)
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

        /// <summary>
        /// Creates a new file and writes the content to it. If a file already exists, it will get deleted and recreated with the content.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileContent">Content to write into the file.</param>
        /// <returns>success of operation</returns>
        public static bool WriteFile(string filePath, string fileContent)
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

        public static List<string> GetAllPipes()
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




    }

}
