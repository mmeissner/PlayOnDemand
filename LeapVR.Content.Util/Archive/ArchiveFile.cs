#region Licence
/****************************************************************
 *  Filename: ArchiveFile.cs
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
using System.IO;

namespace LeapVR.Content.Util.Archive
{
    public class ContentFile
    {
        #region Properties
        public string FullPath { get; }
        public string FileName { get; }
        public string Extension { get; }
        public DateTime Modified { get; }
        public Origin Source { get; }
        public object SourceObject { get;}
        #endregion

        #region Constructor
        public ContentFile(string fullPath,DateTime modified,Archive sourceArchive)
        {
            Modified = modified;
            Source = Origin.Archive;
            SourceObject = sourceArchive;
            FullPath = fullPath;
            FileName = GetFileName(FullPath);
            Extension = GetExtension(FileName);
        }

        public ContentFile(DirectoryInfo rootDirectoryInfo, FileInfo fileInfo)
        {
            Source = Origin.FileSystem;
            SourceObject = fileInfo;
            FullPath = fileInfo.FullName.TrimStart(rootDirectoryInfo.FullName.ToCharArray());
            FileName = fileInfo.Name;
            Extension = fileInfo.Extension;

        }
        #endregion

        string GetFileName(string fullPath)
        {
            var parts = fullPath.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            return parts[parts.Length - 1];
        }
        string GetExtension(string filename)
        {
            string retval = "";
            var fileParts = filename.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (fileParts.Length >= 2) retval = $".{fileParts[fileParts.Length - 1]}";
            return retval;
        }


        public enum Origin
        {
            Archive,
            FileSystem
        }
    }
}
