#region Licence
/****************************************************************
 *  Filename: IDbFileInfo.cs
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

namespace LeapVR.Shell.Repository.Interfaces.Entities
{
    public interface IDBFileInfo
    {
        int Chunks { get; }
        string Filename { get; }
        string Id { get; }
        long Length { get; }
        string MimeType { get; }
        DateTime UploadDate { get; }

        void CopyTo(Stream stream);
        Stream OpenRead();
        Stream OpenWrite();
        void SaveAs(string filename, bool overwritten = true);
    }
}