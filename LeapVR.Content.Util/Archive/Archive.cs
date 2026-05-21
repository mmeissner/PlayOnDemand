#region Licence
/****************************************************************
 *  Filename: Archive.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using LeapVR.Content.Util.Enums;
using LeapVR.Content.Util.Util;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shared.Lib.Win.WinApi;
using NLog;

namespace LeapVR.Content.Util.Archive
{
    public class Archive
    {
        #region  Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly FileInfo _sevenZipExeFile;
        private readonly FileInfo _archiveFile;
        private bool _extractionSuccess = false;
        private readonly List<ArchiveContent> _contents = new List<ArchiveContent>();
        private ulong _physicalSize = 0;
        private string _sevenZipType = "unknown";

        public bool HasError { get; private set; }
        public CompressionType Compression { get; private set; }
        public FileInfo ArchiveFile => _archiveFile;
        public ulong PhysicalSize => _physicalSize;
        public string SevenZipType => _sevenZipType;
        public IReadOnlyList<ArchiveContent> Contents => _contents;
        public bool ExtractionSuccess => _extractionSuccess;
        #endregion

        #region Constructor
        public Archive(FileInfo fileInfoSevenZipExe,FileInfo fileInfo)
        {
            _sevenZipExeFile = fileInfoSevenZipExe;
            _archiveFile = fileInfo;
            Analyze();
        }
        public Archive(FileInfo fileInfoSevenZipExe,string filePath)
        {
            _sevenZipExeFile = fileInfoSevenZipExe;
            _archiveFile = new FileInfo(filePath);
            Analyze();
        }
        #endregion

        #region Public Methods
        public ArchiveReport GetReport()
        {
            return ArchiveReport.Analyze(this);
        }

        public bool Extract(string targetPath,string archivePassword = null, string filter= null)
        {
            List<string> error;
            List<string> output;
            if (HasError) return false;
            if(String.IsNullOrEmpty(archivePassword))archivePassword = "";
            DirectoryInfo taretDirectoryInfo = new DirectoryInfo(targetPath);
            bool targetDirReady = false;
            if (!taretDirectoryInfo.Exists)
            {
                try
                {
                    taretDirectoryInfo.Create();
                    targetDirReady = true;
                }
                catch (Exception exception)
                {
                   Debug.Print(exception.Message);
                }
            }
            else targetDirReady = true;
            if (!targetDirReady) return false;
            if (!_archiveFile.Exists) return false;

            //Not yet added
            //Switch - aoa:
            //This switch overwrites all destination files.Use it when the new versions are preferred.
            //Switch - aos:
            //Skip over existing files without overwriting. Use this for files where the earliest version is most important.
            //Switch - aou:
            //Avoid name collisions.New files extracted will have a number appending to their names.You will have to deal with them later.
            //Switch - aot:
            //Rename existing files.This will not rename the new files, just the old ones already there.
            if (string.IsNullOrEmpty(filter))
            {
                if (!IsEnoughDiskSpace(this, targetPath)) return false;
                ConsoleProcess.Run(_sevenZipExeFile.FullName, $"x -p\"{archivePassword}\" -sccUTF-8 -r -mmt=on -y -o\"{targetPath}\" -- \"{_archiveFile.FullName}\"", out output, out error);
            }
            else
            {
                ConsoleProcess.Run(_sevenZipExeFile.FullName, $"x -p\"{archivePassword}\" -sccUTF-8 -r -mmt=on -y -o\"{targetPath}\" -- \"{_archiveFile.FullName}\" \"{filter}\"", out output, out error);
            }
            if (error.Any(x => x == "")) error.RemoveAll(x => x == "");
            if (!output.Any() || error.Any())
            {
                Logger.Error($"Extraction Error: Output empty or Error occured for archive {ArchiveFile.FullName} ");
                HasError = true;
                return false;
            }
            AnalyzeListExtractionEntries(output,error,targetPath);
            return true;
        }
        #endregion

        #region Private Methods
        private void Analyze()
        {
            List<string> error;
            List<string> output;
            ConsoleProcess.Run(_sevenZipExeFile.FullName, $"l -sccUTF-8 -- \"{_archiveFile.FullName}\"", out output, out error);
            if (output.Any() && !error.Any())
            {
                Logger.Error($"Analyze Error: Output empty or Error occured for archive {ArchiveFile.FullName} ");
                HasError = true;
                return;
            }
            AnalyzeListArchiveEntries(output);
            if(Contents.Count == 0) HasError = true;
        }

        void AnalyzeListArchiveEntries(List<string> entries)
        {
            bool headerStarted = false;
            int bodyStartLine = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                //Detect Start of Data
                if (!headerStarted && entries[i] == "--") headerStarted = true;
                if(!headerStarted) continue;

                if (entries[i].Contains("Type = "))
                {
                    _sevenZipType = entries[i].Replace("Type = ", "").Trim();
                    switch (_sevenZipType.ToLowerInvariant())
                    {
                        case "zip":
                            Compression = CompressionType.Zip;
                            break;
                        case "rar":
                            Compression = CompressionType.Rar;
                            break;
                        case "7z":
                            Compression = CompressionType.SevenZip;
                            break;
                        case "rar5":
                            Compression = CompressionType.Rar5;
                            break;
                        case "split":
                            Compression = CompressionType.SplitArchive;
                            break;
                    }
                }
                else if(entries[i].ToLowerInvariant().Contains("total physical size = "))
                {
                    var sPhysicalSize = entries[i].ToLowerInvariant().Replace("total physical size = ", "");
                    if (!ulong.TryParse(sPhysicalSize, out _physicalSize)) _physicalSize = 0;
                }
                else if (entries[i].ToLowerInvariant().Contains("physical size = "))
                {
                    var sPhysicalSize = entries[i].ToLowerInvariant().Replace("physical size = ", "");
                    if (!ulong.TryParse(sPhysicalSize, out _physicalSize)) _physicalSize = 0;
                }
                
                //Last List Line of Header
                if (entries[i].ToLowerInvariant().Contains("-------------------"))
                {
                    bodyStartLine = i + 1;
                    break;
                }
            }
            if (bodyStartLine != 0)
            {
                for (int i = bodyStartLine; i < entries.Count - 3; i++)
                {
                    //End of Content
                    if (entries[i].StartsWith("-------------------")) break;
                    var newEntry = new ArchiveContent(entries[i].Substring(53), GetUncompressedSizeFromLine(entries[i]), GetCompressedSizeFromLine(entries[i]), GetModifiedDateFromLine(entries[i]));
                    _contents.Add(newEntry);
                }
            }
        }

        void AnalyzeListExtractionEntries(List<string> entries,List<string> error,string targetPath, bool deleteDirectoryOnError = true)
        {
            bool extractionSuccess = false;
            foreach (string entry in entries)
            {
                if (entry.ToLowerInvariant() == "Everything is Ok") extractionSuccess = true;
            }
            var errorFiles = new List<string>();
            const string errorIndicator = @"ERROR:";

            //Analyze Errors
            foreach (string entry in error)
            {
                if (entry.ToLowerInvariant().StartsWith(errorIndicator.ToLowerInvariant()))
                {
                    extractionSuccess = false;
                    var sTemp = entry.Substring(errorIndicator.Length);
                    var sParts = sTemp.Split(new []{':'});
                    if (sParts.Length == 2)
                    {
                        sTemp = sParts[1].TrimStart(' ');
                        errorFiles.Add(sTemp);
                    }
                }
            }
            //Cleanup on Error
            if (deleteDirectoryOnError)
            {
                if (errorFiles.Any() || !extractionSuccess)
                {
                    foreach (string file in errorFiles)
                    {
                        var fileInfo = new FileInfo(Path.Combine(targetPath, file));
                        if (fileInfo.Exists)
                        {
                            try
                            {
                                fileInfo.Delete();
                            }
                            catch (Exception exception)
                            {
                                Debug.WriteLine(exception.Message);
                            }
                            if (fileInfo.DirectoryName != targetPath)
                            {
                                if (fileInfo.Directory != null)
                                {
                                    try
                                    {
                                        if (fileInfo.Directory.Exists) Directory.Delete(fileInfo.Directory.FullName, true);
                                    }
                                    catch (IOException)
                                    {
                                        if (fileInfo.Directory.Exists) Directory.Delete(fileInfo.Directory.FullName, true);
                                    }
                                    catch (UnauthorizedAccessException)
                                    {
                                        if (fileInfo.Directory.Exists) Directory.Delete(fileInfo.Directory.FullName, true);
                                    }
                                    if (fileInfo.Directory.Exists) Directory.Delete(fileInfo.Directory.FullName, true);
                                }
                               
                            }
                        }
                    }
                }
            }
            _extractionSuccess = extractionSuccess;
        }

        private static bool IsEnoughDiskSpace(Archive archive,string targetDirectory)
        {
            ulong freespace = 0;
            if (Kernel32Util.DriveFreeBytes(targetDirectory, out freespace))
            {
                if (freespace > archive._physicalSize) return true;
                return false;
            }
            return false;
        }
        #endregion

        #region Line Data Extraction Methods
        int GetUncompressedSizeFromLine(string line)
        {
            var lineCut = line.Substring(0, 38);
            var lineChars = line.ToCharArray(0, 38);
            for (int i = lineChars.Length - 1; i >= 0; i--)
            {
                int retVal;
                var substring = lineCut.Substring(i);
                if (int.TryParse(substring, out retVal)) continue;
                int.TryParse(lineCut.Substring(i + 1), out retVal);
                return retVal;
            }
            return -1;
        }

        int GetCompressedSizeFromLine(string line)
        {
            var lineCut = line.Substring(0, 51);
            var lineChars = line.ToCharArray(0, 51);
            for (int i = lineChars.Length - 1; i >= 0; i--)
            {
                int retVal;
                var substring = lineCut.Substring(i);
                if (int.TryParse(substring, out retVal)) continue;
                int.TryParse(lineCut.Substring(i + 1), out retVal);
                return retVal;
            }
            return -1;
        }

        DateTime GetModifiedDateFromLine(string line)
        {
            var lineCut = line.Substring(0, 19);
            DateTime retVal;
            if (DateTime.TryParse(lineCut, out retVal))return retVal;
            else return DateTime.MinValue;
        }
        #endregion

    }
}
