#region Licence
/****************************************************************
 *  Filename: IGenericFileRepository.cs
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
using System.Collections.Generic;
using System.IO;
using LeapVR.Shell.Repository.Interfaces.Entities;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces {
    public interface IGenericFileRepository {
        /// <summary>
        /// Stores the specified source file in the file repository.
        /// </summary>
        /// <param name="sourceFilePath">The full filepathname of the source file.</param>
        /// <param name="filename">The filename to store in cache.</param>
        /// <param name="category">The subdirectory to store in cache</param>
        /// <returns></returns>
        IDBFileInfo StoreFile(string sourceFilePath, string filename, string category = null);
        /// <summary>
        /// Stores the specified source file in the file repository.
        /// </summary>
        /// <param name="stream">The Stream for the Data to store</param>
        /// <param name="filename">The filename to store in cache.</param>
        /// <param name="category">The subdirectory to store in cache</param>
        /// <returns></returns>
        IDBFileInfo StoreFile(Stream stream, string filename, string category = null);
        bool DeleteFile(string filename, string category = null);
        bool SaveFileToDisk(string destinationFilePathName, string filename,bool overwrite= true, string category = null);
        Stream GetFileAsStream(string filename, string category = null);
        IEnumerable<IDBFileInfo> GetAllFiles();
        IDBFileInfo FindFile(string filename, string category = null);
    }
}