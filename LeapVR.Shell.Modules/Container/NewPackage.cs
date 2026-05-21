#region Licence
/****************************************************************
 *  Filename: NewPackage.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using Ionic.Zip;
using LeapVR.Shared.Lib;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using CompressionLevel = Ionic.Zlib.CompressionLevel;

namespace LeapVR.Shell.Modules.Container
{
    public class NewPackage : INewPackage
    {
        // Level3 instead of Level8: game packages are dominated by already-
        // compressed assets (mp4/png/dds/pak/zip etc.) where Level 8 costs ~3x
        // the CPU of Level 3 for ~5% size savings. Combined with parallel
        // deflate below this is the single biggest creation-time win.
        private readonly CompressionLevel CompressionLevel = CompressionLevel.Level3;

        // Files at or above this size run through DotNetZip's parallel deflate
        // worker pool instead of the single-threaded path. -1 disables; 0
        // forces parallel for every entry. 1 MB is the documented sweet spot:
        // small entries pay coordination overhead, large ones win big.
        private const long ParallelDeflateThresholdBytes = 1L * 1024 * 1024;

        // Coalesce per-byte progress callbacks into per-MB notifications so
        // multi-GB packages don't drown the WPF dispatcher in Rx events. The
        // wizard's spinner doesn't need byte-level granularity.
        private const long ProgressNotifyEveryBytes = 1L * 1024 * 1024;

        private readonly ReplaySubject<Empty> _whenPackageOperationStartedSubject;
        private readonly Subject<Empty> _whenProgressChangedSubject;
        private readonly ReplaySubject<Empty> _whenPackageOperationEndedSubject;
        private readonly List<InternalFileInfo> _allFilesInfo;
        private int _wasPackageOperationStarted; // 0 = false, 1 = true
        private int _isPackageOperationEnded; // 0 = false, 1 = true
        private readonly NewAppInstallationContainer _container;



        #region Properties & Fields
        public Guid PackageGuid { get; }
        public uint PackageVersion { get; set; }
        public Guid ApplicationGuid { get; }
        public ContentType ContentType { get; }
        public bool WasPackageOperationStarted => _wasPackageOperationStarted != 0;
        public bool IsPackageOperationEnded => _isPackageOperationEnded != 0;
        public Exception PackageOperationException { get; private set; }
        public int TotalFilesCount { get; private set; }
        public int DoneFilesCount { get; private set; }
        public long TotalFilesSize { get; private set; }
        public long DoneFilesSize { get; private set; }
        public IObservable<Empty> WhenProgressChanged { get; }
        public IObservable<PackageProgress> WhenPackageProgressChanged { get; }

        #endregion Properties & Fields

        #region Constructors

        internal NewPackage(NewAppInstallationContainer container, Guid applicationGuid, ContentType contentType)
        {
            QuickLeap.AssertNotNull(container);
            _container = container;
            _whenPackageOperationStartedSubject = new ReplaySubject<Empty>();
            _whenProgressChangedSubject = new Subject<Empty>();
            _whenPackageOperationEndedSubject = new ReplaySubject<Empty>();
            _allFilesInfo = new List<InternalFileInfo>();

            PackageGuid = Guid.NewGuid();
            ApplicationGuid = applicationGuid;
            ContentType = contentType;
            WhenProgressChanged = _whenProgressChangedSubject.AsObservable();
        }

        // TODO [RM]: maybe allow to assign PackageGuid by constructor

        #endregion Constructors

        #region Methods

        public void AddFile(string fullFilePath, string packageRelativePath)
        {
            QuickLeap.OperateInterlockedFlag(ref _wasPackageOperationStarted, null, false);

            // TODO [RM]: check if files/directories exists?
            // TODO [RM]: check if already added (covered)? (check full path and relative path)

            var fileName = Path.GetFileName(fullFilePath);
            var fileRelativePath = Path.Combine(packageRelativePath, fileName);

            var newFileInfo = new InternalFileInfo
            {
                FullFilePath = fullFilePath,
                ArchiveRelativeFilePath = fileRelativePath,
                ArchiveRelativeDirectory = packageRelativePath,
                FileInfo = new FileInfo(fullFilePath),
            };
            _allFilesInfo.Add(newFileInfo);
            TotalFilesCount++;
            TotalFilesSize += newFileInfo.FileInfo.Length;
        }

        public void AddDirectory(string fullDirectoryPath, string packageRelativePath)
        {
            QuickLeap.OperateInterlockedFlag(ref _wasPackageOperationStarted, null, false);

            // TODO [RM]: check if files/directories exists?
            // TODO [RM]: check if already added (covered)? (check full path and relative path)

            var fullFilePaths = Directory.GetFiles(fullDirectoryPath, "*", SearchOption.AllDirectories);
            foreach (var fullFilePath in fullFilePaths)
            {
                var fileRelativePath = QuickLeap.GetRelativePath(fullFilePath, fullDirectoryPath);
                var archiveRelativePath = Path.Combine(packageRelativePath, fileRelativePath);
                var archiveRelativeDir = Path.GetDirectoryName(archiveRelativePath);

                var newFileInfo = new InternalFileInfo
                {
                    FullFilePath = fullFilePath,
                    ArchiveRelativeFilePath = archiveRelativePath,
                    ArchiveRelativeDirectory = archiveRelativeDir,
                    FileInfo = new FileInfo(fullFilePath),
                };
                _allFilesInfo.Add(newFileInfo);
                TotalFilesCount++;
                TotalFilesSize += newFileInfo.FileInfo.Length;
            }
        }

        internal long WriteToStream(Stream stream)
        {
            long result;
            try
            {
                QuickLeap.OperateInterlockedFlag(ref _isPackageOperationEnded, null, false);
                QuickLeap.OperateInterlockedFlag(ref _wasPackageOperationStarted, true, false);

                NotifyStarted();

                var startPos = stream.Position;
                using (var zip = new Ionic.Zip.ZipFile())
                {
                    zip.AlternateEncoding = Encoding.UTF8;
                    zip.AlternateEncodingUsage = ZipOption.Always;
                    zip.CompressionLevel = CompressionLevel;
                    zip.ParallelDeflateThreshold = ParallelDeflateThresholdBytes;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.SaveProgress += Zip_OnSaveProgress;
                    foreach (var file in _allFilesInfo)
                    {
                        zip.AddFile(file.FullFilePath, file.ArchiveRelativeDirectory);
                    }
                    zip.Save(stream);
                }
                var endPos = stream.Position;
                result = endPos - startPos;
            }
            catch (Exception e)
            {
                QuickLeap.OperateInterlockedFlag(ref _isPackageOperationEnded, true);
                NotifyEnded(e);
                throw;
            }

            QuickLeap.OperateInterlockedFlag(ref _isPackageOperationEnded, true);
            NotifyEnded(null);
            return result;
        }

        private long _entryLastBytesTransfered;
        private long _bytesSinceLastNotify;
        private void Zip_OnSaveProgress(object sender, SaveProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Saving_BeforeWriteEntry:
                    _entryLastBytesTransfered = 0;
                    break;
                case ZipProgressEventType.Saving_EntryBytesRead:
                    var bytesDelta = e.BytesTransferred - _entryLastBytesTransfered;
                    _entryLastBytesTransfered = e.BytesTransferred;
                    DoneFilesSize += bytesDelta;
                    _bytesSinceLastNotify += bytesDelta;
                    if (_bytesSinceLastNotify >= ProgressNotifyEveryBytes)
                    {
                        _bytesSinceLastNotify = 0;
                        NotifyChanged();
                    }
                    break;
                case ZipProgressEventType.Saving_AfterWriteEntry:
                    DoneFilesCount++;
                    _bytesSinceLastNotify = 0;
                    NotifyChanged();
                    break;
            }
        }

        private void NotifyStarted()
        {
            _whenPackageOperationStartedSubject.OnNext(Empty.Get);
            _whenPackageOperationStartedSubject.OnCompleted();
        }

        private void NotifyChanged()
        {
            _whenProgressChangedSubject.OnNext(Empty.Get);
        }

        private void NotifyEnded(Exception e)
        {
            _whenProgressChangedSubject.OnCompleted();

            PackageOperationException = e;
            _whenPackageOperationEndedSubject.OnNext(Empty.Get);
            _whenPackageOperationEndedSubject.OnCompleted();
        }

        #endregion Methods
    }
}
