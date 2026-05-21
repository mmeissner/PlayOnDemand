#region Licence
/****************************************************************
 *  Filename: ArchiveContent.cs
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

namespace LeapVR.Content.Util.Archive
{
    public class ArchiveContent
    {
        public ArchiveContent(string fullPath,int uncompressedSize, int compressedSize, DateTime modifiedDate)
        {
            Modified = modifiedDate;
            FullPath = fullPath;
            CompressedSize = compressedSize;
            UncompressedSize = uncompressedSize;
            Type = uncompressedSize == 0 ? ContentType.Folder : ContentType.File;

        }
        public ContentType Type { get; }
        public string FullPath { get; }
        public DateTime Modified { get; }
        public int CompressedSize { get; }
        public int UncompressedSize { get; }
    }
    public enum ContentType
    {
        Unset,
        File,
        Folder
    }
}