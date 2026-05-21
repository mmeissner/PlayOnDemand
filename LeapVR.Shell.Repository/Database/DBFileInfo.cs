#region Licence
/****************************************************************
 *  Filename: DBFileInfo.cs
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
using LeapVR.Shell.Repository.Interfaces;
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using LiteDB;

namespace LeapVR.Shell.Repository.Database {
    public class DBFileInfo : IDBFileInfo
    {
        private readonly LiteFileInfo _liteFileInfo;

        public DBFileInfo(LiteFileInfo liteFileInfo, bool removeBaseId = true)
        {
            _liteFileInfo = liteFileInfo; 
            //Remove the BaseId as it needs to be transparent to consumers
            if(removeBaseId) Id = _liteFileInfo.Id.Substring(37);
            else Id = _liteFileInfo.Id;
        }

        public string Id { get; }

        public string Filename => _liteFileInfo.Filename;

        public string MimeType => _liteFileInfo.MimeType;

        public long Length => _liteFileInfo.Length;

        public int Chunks => _liteFileInfo.Chunks;

        public DateTime UploadDate => _liteFileInfo.UploadDate;

        public void SaveAs(string filename, bool overwritten = true){_liteFileInfo.SaveAs(filename,overwritten);}
        
        public Stream OpenRead() { return _liteFileInfo.OpenRead(); }

        public Stream OpenWrite() { return _liteFileInfo.OpenWrite(); }

        public void CopyTo(Stream stream){_liteFileInfo.CopyTo(stream);}

    }
}