#region Licence
/****************************************************************
 *  Filename: Library.cs
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
using System.Text;
using LeapVR.Content.Util.Properties;
using LeapVR.Content.Util.Util;
using LeapVR.Shell.Domain.Models;

namespace LeapVR.Content.Util
{
    public static class Library
    {
        public const string LookUpFileAppId = "AppId";
        public const string LookUpFileCategory = "Category";

        static readonly string ArchiveFile = Path.Combine(Environment.CurrentDirectory, "archives.lst");
        static readonly string ExcludedFileExtensions = Path.Combine(Environment.CurrentDirectory, "excluded.lst");
        static readonly string RenameDirectories = Path.Combine(Environment.CurrentDirectory, "renameDir.lst");

        static readonly List<string> ArchivesFileExtensions;
        static readonly List<string> ExcludeFileExtensions;
        static readonly List<string> ExcludedDirectories;
        static Library()
        {
            ArchivesFileExtensions = new List<string>();
            ArchivesFileExtensions = GetCustomValues(GetDefaultArchiveFileExtensions(), ArchiveFile);
            ExcludeFileExtensions = GetCustomValues(GetExcludedFileExtensions(), ExcludedFileExtensions);
            ExcludedDirectories = GetCustomValues(GetExcludedDirectories(), RenameDirectories);
        }

        private static List<string> GetCustomValues(List<string> defaultvalues,string fullfilename)
        {
            HashSet<string> retval = new HashSet<string>(defaultvalues);
            List<string> readvalues = new List<string>();
            if (File.Exists(fullfilename))
            {
                readvalues = ReadLibrary(fullfilename);
            }
            foreach (var item in readvalues)
            {
                retval.Add(item);
            }
            return retval.ToList();
        }

        private static List<string> ReadLibrary(string fullfilename)
        {
            var retval = new List<string>();
            var lines = File.ReadAllLines(fullfilename);

            foreach(string line in lines)
            {
                if(!String.IsNullOrWhiteSpace(line))retval.Add(line);
            }
            return retval;
        }

        public static List<string> ArchivesFileExtension => ArchivesFileExtensions;
        public static List<string> FileExclusions => ExcludeFileExtensions;
        public static List<string> DirectoryExclusions => ExcludedDirectories;

        static List<string> GetDefaultArchiveFileExtensions()
        {
            return new List<string>
            {
               ".zip",
               ".rar" ,
               ".7zip" ,
               ".r01" ,
               ".7z"
            };
        }
        static List<string> GetExcludedFileExtensions()
        {
            return new List<string>
            {
               ".txt",
               ".url" ,
               ".nfo" ,
               ".info"
            };
        }
        static List<string> GetExcludedDirectories()
        {
            return new List<string>
            {
                "game",
            };
        }
    }
}
