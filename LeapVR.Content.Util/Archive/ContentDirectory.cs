#region Licence
/****************************************************************
 *  Filename: ContentDirectory.cs
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
using LeapVR.Content.Util.Util;

namespace LeapVR.Content.Util.Archive
{
    public class ContentDirectory
    {
        #region Properties
        private readonly Archive _sourceArchive;
        private readonly DirectoryInfo _sourceFile;
        private readonly RootType _rootType;

        public string Name { get; private set; }
        public string RelativeFullName { get; }
        public List<ContentDirectory> Directories { get; }
        public List<ContentFile> Files { get; }
        public Archive SourceArchive => _sourceArchive;
        public DirectoryInfo SourceFileSystem => _sourceFile;
        public RootType Type => _rootType;
        #endregion

        #region Constructors
        protected ContentDirectory(ContentDirectory contentRootDirectory)
        {
            _rootType = contentRootDirectory._rootType;
            Directories = new List<ContentDirectory>();
            Files = new List<ContentFile>();
            Name = "%ROOT%";
            RelativeFullName = contentRootDirectory.RelativeFullName;
            Directories.AddRange(contentRootDirectory.Directories);
            Files.AddRange(contentRootDirectory.Files);

            switch (contentRootDirectory._rootType)
            {
                case RootType.Archive:
                    _sourceArchive = contentRootDirectory._sourceArchive;
                    break;
                case RootType.Directory:
                    _sourceFile = contentRootDirectory._sourceFile;
                    break;
                default:
                    throw new Exception("Invalid Operation");
            }
        }
        private ContentDirectory(string relativeFullPath, ContentDirectory rootOfNewDir)
        {
            if (rootOfNewDir.Type == RootType.Undefined) throw new Exception("Invalid Operation");

            _rootType = rootOfNewDir._rootType;
            _sourceFile = rootOfNewDir.SourceFileSystem;
            _sourceArchive = rootOfNewDir.SourceArchive;
            Directories = new List<ContentDirectory>();
            Files = new List<ContentFile>();
            RelativeFullName = relativeFullPath;

            var parts = relativeFullPath.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            Name = parts.Length > 0 ? parts[parts.Length - 1] : relativeFullPath;
        }

        public ContentDirectory(Archive sourceArchive)
        {
            _rootType = RootType.Archive;
            Directories = new List<ContentDirectory>();
            Files = new List<ContentFile>();
            _sourceArchive = sourceArchive;
            RelativeFullName = "\\";
            Name = "%ROOT%";

            foreach (ArchiveContent content in sourceArchive.Contents)
            {
                AddArchiveContent(content);
            }
        }
        public ContentDirectory(DirectoryInfo rootDirectoryInfo, DirectoryInfo directoryInfo)
        {
            _rootType = RootType.Directory;
            _sourceFile = directoryInfo;
            Directories = new List<ContentDirectory>();
            Files = new List<ContentFile>();
            RootFromDirectoryInfo(rootDirectoryInfo, directoryInfo);
            RelativeFullName = directoryInfo.FullName.Replace(rootDirectoryInfo.FullName, "");
            var parts = RelativeFullName.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            Name = parts.Length > 0 ? parts[parts.Length - 1] : RelativeFullName;
            
        }
        #endregion

        #region AddContent
        private void AddArchiveContent(ArchiveContent content)
        {
            var parts = content.FullPath.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            AddContentToDirectory(content, this, parts, parts);
        }
        private void AddArchiveContent(ArchiveContent content, string[] allparts, string[] remainingParts)
        {
            AddContentToDirectory(content, this, allparts, remainingParts);
        }

        /// <summary>
        /// Adds content to 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="currentDirectory">The current analyzed directory</param>
        /// <param name="allparts">All SubDirectory Parts</param>
        /// <param name="remainingParts">SubDirectories not traversed</param>
        private void AddContentToDirectory(ArchiveContent content, ContentDirectory currentDirectory, string[] allparts, string[] remainingParts)
        {
            var newParts = remainingParts.SubArray(1, remainingParts.Length - 1);
            //Must have SubLevel
            if (remainingParts.Length > 1)
            {
                foreach (ContentDirectory directory in currentDirectory.Directories)
                {
                    //Subdirectory found
                    if (directory.Name == remainingParts[0])
                    {
                        directory.AddContentToDirectory(content, directory, allparts, newParts);
                        return;
                    }
                }
                //No directory existing yet
                string newFullPath = "";
                for (int i = 0; i < allparts.Length - (remainingParts.Length - 1); i++)
                {
                    if (newFullPath == "") newFullPath = allparts[i];
                    else newFullPath = newFullPath + "\\" + allparts[i];
                }
                ContentDirectory myNewDirectory = new ContentDirectory(newFullPath, currentDirectory);
                currentDirectory.Directories.Add(myNewDirectory);
                myNewDirectory.AddArchiveContent(content, allparts, newParts);
            }
            //At Destination
            else
            {
                if (content.Type == ContentType.File)
                {
                    if(Type == RootType.Archive)currentDirectory.Files.Add(new ContentFile(content.FullPath,content.Modified, SourceArchive));
                }
                else if (content.Type == ContentType.Folder)
                {
                    if (currentDirectory.Directories.Exists(x => x.Name == remainingParts[0])) return;
                    currentDirectory.Directories.Add(new ContentDirectory(content.FullPath, currentDirectory));
                }
            }
        }
        public enum RootType
        {
            Undefined,
            Archive,
            Directory
        }
        #endregion

        private void RootFromDirectoryInfo(DirectoryInfo rootDirectoryInfo, DirectoryInfo directoryInfo)
        {
            try
            {
                foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
                {
                    Directories.Add(new ContentDirectory(rootDirectoryInfo, directory));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            try
            {
                foreach (FileInfo file in directoryInfo.EnumerateFiles())
                {
                    Files.Add(new ContentFile(directoryInfo, file));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}