#region Licence
/****************************************************************
 *  Filename: IAppFileRepository.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
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

namespace LeapVR.Shell.Repository.Interfaces.Interfaces
{
    /// <summary>
    /// Repository for <see cref="FileInfo"/>.
    /// </summary>
    public interface IAppFileRepository
    {
        IEnumerable<FileInfo> GetAll();
        IEnumerable<FileInfo> Get(Guid applicationGuid);
        FileInfo Add(Guid applicationGuid,string filePathName);
        void Delete(FileInfo filePathName);
        void DeleteAll(Guid applicationGuid);
    }
}