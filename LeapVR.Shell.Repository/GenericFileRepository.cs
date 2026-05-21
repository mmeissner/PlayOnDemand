#region Licence
/****************************************************************
 *  Filename: GenericFileRepository.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository
{
    public sealed class GenericFileRepository : IGenericFileRepository
    {
        readonly string _baseIdString;
        public GenericFileRepository(Guid ownerId) { _baseIdString = ownerId + "/"; }

        /// <summary>
        /// Stores the specified source file in the file repository.
        /// </summary>
        /// <param name="sourceFilePath">The full filepathname of the source file.</param>
        /// <param name="filename">The filename to store in cache.</param>
        /// <param name="category">The subdirectory to store in cache</param>
        /// <returns></returns>
        public IDBFileInfo StoreFile(string sourceFilePath, string filename, string category = null)
        {
            try
            {
                QuickLeap.AssertNotNull(sourceFilePath, filename);
                return Database.Database.FileUpload(BuildId(filename, category), sourceFilePath);
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException(
                        $"Exception during CacheFile, SourceFilePath = {sourceFilePath}, filename={filename}, directory={category}",
                        exception);
            }
        }

        /// <summary>
        /// Stores the specified source file in the file repository.
        /// </summary>
        /// <param name="stream">The Stream for the Data to store</param>
        /// <param name="filename">The filename to store in cache.</param>
        /// <param name="category">The subdirectory to store in cache</param>
        /// <returns></returns>
        public IDBFileInfo StoreFile(Stream stream, string filename, string category = null)
        {
            try
            {
                QuickLeap.AssertNotNull(stream, filename);
                return Database.Database.FileUpload(BuildId(filename, category), filename, stream);
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException(
                        $"Exception during StoreFile (Stream), filename={filename}, directory={category}",
                        exception);
            }
        }

        public bool DeleteFile(string filename, string category = null)
        {
            try
            {
                QuickLeap.AssertNotNull(filename);
                return Database.Database.FileDelete(BuildId(filename, category));
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException(
                        $"Exception during DeleteFile, filename={filename}, directory={category}",
                        exception);
            }
        }

        public bool SaveFileToDisk(
                string destinationFilePathName, string filename, bool overwrite = true, string category = null)
        {
            try
            {
                QuickLeap.AssertNotNull(destinationFilePathName, filename);
                var file = Database.Database.FileFindById(BuildId(filename, category));
                if(file == null) return false;
                file.SaveAs(destinationFilePathName, overwrite);
                return true;
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException(
                        $"Exception during SaveFileToDisk,destinationFilePathName ={destinationFilePathName}, filename={filename}, directory={category}",
                        exception);
            }
        }

        public Stream GetFileAsStream(string filename, string category = null)
        {
            try
            {
                QuickLeap.AssertNotNull(filename);
                return Database.Database.FileOpenRead(BuildId(filename, category));
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException(
                        $"Exception during GetFileStream, filename={filename}, directory={category}",
                        exception);
            }
        }

        public IEnumerable<IDBFileInfo> GetAllFiles()
        {
            try
            {
                return Database.Database.FileFind(_baseIdString);
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Exception during GetAllFiles", exception);
            }
        }

        public IDBFileInfo FindFile(string filename, string category = null)
        {
            try
            {
                return Database.Database.FileFindById(BuildId(filename, category));
            }
            catch(LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Exception during GetAllFiles", exception);
            }
        }

        private string BuildId(string filename, string category)
        {
            string id = _baseIdString;
            if(category != null) id = id + category.Replace('\\', '/') + filename;
            else id = id + filename;
            return id;
        }
    }
}