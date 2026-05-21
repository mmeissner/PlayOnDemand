#region Licence
/****************************************************************
 *  Filename: ZipReadablePackage.cs
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
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using Ionic.Zip;
using LeapVR.Shared.Lib;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using NLog;
using ZipFile = Ionic.Zip.ZipFile;

namespace LeapVR.Shell.Modules.Container
{
    public class ZipReadablePackage : IContainerPackage // TODO [RM]: move to infrastructure layer
    {
        #region Private Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ZipContainer _container;
        private readonly long _fileOffset;
        private volatile bool _wasPackageOperationStarted; // 0 = false, 1 = true
        private volatile bool _isPackageOperationEnded; // 0 = false, 1 = true

        private readonly ReplaySubject<PackageProgress> _whenPackageProgressChangedSubject;
        #endregion

        #region Properties & Fields
        public Guid ApplicationGuid { get; set; }
        public ContentType ContentType { get; set; }
        public Guid PackageGuid { get; set; }
        public uint PackageVersion { get; set; }
        public int TotalFilesCount { get; set; }
        public long TotalFilesSize { get; set; }
        public bool WasPackageOperationStarted => _wasPackageOperationStarted;
        public bool IsPackageOperationEnded => _isPackageOperationEnded;
        public int DoneFilesCount { get; private set; } // TODO [RM]: } volatile
        public long DoneFilesSize { get; private set; } // TODO [RM]: } volatile
        public Exception PackageOperationException { get; private set; }
        public IObservable<PackageProgress> WhenPackageProgressChanged { get; }
        #endregion

        #region Constructors
        internal ZipReadablePackage(ZipContainer container, long fileOffset,IPackageData packageData)
        {
            QuickLeap.AssertNotNull(container);
            _container = container;
            _fileOffset = fileOffset;
            ApplicationGuid = packageData.ApplicationGuid;
            ContentType = packageData.ContentType;
            PackageGuid = packageData.PackageGuid;
            PackageVersion = packageData.PackageVersion;
            TotalFilesCount = packageData.TotalFilesCount;
            TotalFilesSize = packageData.TotalFilesSize;
            _whenPackageProgressChangedSubject = new ReplaySubject<PackageProgress>();
            WhenPackageProgressChanged = _whenPackageProgressChangedSubject.AsObservable();
        }

        #endregion Constructors

        #region Methods

        public void ExtractToDirectory(string directoryPath)
        {
            try
            {
                QuickLeap.AssertNotNull(directoryPath);
                Directory.CreateDirectory(directoryPath);

                // First pass: read central directory once to discover entries.
                // Cheap (KB-MB) compared to the actual extraction.
                List<string> entryNames;
                using (var zipStream = _container.OpenZipStream(_fileOffset))
                using (var zip = ZipFile.Read(zipStream))
                {
                    entryNames = zip.Entries.Select(e => e.FileName).ToList();
                    _entriesTotal = entryNames.Count;
                }

                NotifyStarted();
                Logger.Info($"Extracting {entryNames.Count} entries for "
                    + $"{ContentType} package to '{directoryPath}'.");

                // Fan out across CPU cores. Each worker owns its own
                // FileStream + ZipFile so they don't contend on stream
                // position. Round-robin entries across workers so file-size
                // skew doesn't strand the last worker.
                //
                // Workers cap at min(cores, entries) — extracting 3 files
                // across 16 cores would waste 13 file handles.
                int workers = Math.Min(
                    Math.Max(Environment.ProcessorCount, 1),
                    Math.Max(entryNames.Count, 1));

                var workerExceptions = new List<Exception>();
                var workerExceptionsLock = new object();

                Parallel.For(0, workers, new ParallelOptions
                {
                    MaxDegreeOfParallelism = workers,
                }, workerIdx =>
                {
                    try
                    {
                        ExtractSlice(directoryPath, workerIdx, workers);
                    }
                    catch (Exception ex)
                    {
                        lock (workerExceptionsLock) workerExceptions.Add(ex);
                    }
                });

                if (workerExceptions.Count > 0)
                {
                    throw new AggregateException(
                        "One or more workers failed during parallel extract.",
                        workerExceptions);
                }

                Logger.Info($"Extraction complete for {ContentType} package; "
                    + $"{entryNames.Count} entries, {DoneFilesSize} bytes.");
            }
            catch (Exception exception)
            {
                NotifyErrored(exception);
                Logger.Error(exception, "Exception during Extraction of Package!");
            }
            NotifyFinished();
        }

        private void ExtractSlice(string directoryPath, int workerIdx, int workers)
        {
            using (var zipStream = _container.OpenZipStream(_fileOffset))
            using (var zip = ZipFile.Read(zipStream))
            {
                // Round-robin slice: worker w gets entries [w, w+workers,
                // w+2*workers, ...]. This balances total bytes per worker
                // better than contiguous slicing when entry sizes are
                // skewed (a few huge files at the tail is the common case).
                int i = workerIdx;
                while (i < zip.Entries.Count)
                {
                    var entry = zip.Entries.ElementAt(i);
                    entry.Extract(directoryPath,
                        ExtractExistingFileAction.OverwriteSilently);

                    System.Threading.Interlocked.Increment(ref _doneEntries);
                    System.Threading.Interlocked.Add(
                        ref _doneBytes, entry.UncompressedSize);

                    // Progress notify only on MB boundaries to avoid drowning
                    // the WPF dispatcher on packages with many small files.
                    if ((entry.UncompressedSize >= ProgressNotifyEveryBytes)
                        || (_doneEntries % 64 == 0))
                    {
                        DoneFilesCount = _doneEntries;
                        DoneFilesSize = _doneBytes;
                        NotifyChanged(PackagePhases.Extracting);
                    }
                    i += workers;
                }
            }
        }

        // Throttle progress: report at least once per MB extracted OR every
        // 64 entries — whichever comes first.
        private const long ProgressNotifyEveryBytes = 1L * 1024 * 1024;

        private int _doneEntries;
        private long _doneBytes;

        public void CreateZipFromPackage(string filePathName)
        {
            const int BUFFER_SIZE = 1024 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            try
            {
                using (Stream input = _container.OpenZipStream(_fileOffset))
                {
                    while (input.Position < input.Length)
                    {
                        using (Stream output = File.Create(filePathName))
                        {
                            int bytesRead;
                            while ((bytesRead = input.Read(buffer, 0, BUFFER_SIZE)) > 0)
                            {
                                output.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NotifyErrored(e);
            }
        }

        // Set once in ExtractToDirectory's enumeration pass, then read by
        // ConstructPackageProgress.
        private int _entriesTotal;

        private void NotifyStarted()
        {
            _wasPackageOperationStarted = true;

            var pakcageProgress = ConstructPackageProgress(PackagePhases.Started);
            _whenPackageProgressChangedSubject.OnNext(pakcageProgress);
        }

        private void NotifyChanged(PackagePhases phases)
        {
            var pakcageProgress = ConstructPackageProgress(phases);
            _whenPackageProgressChangedSubject.OnNext(pakcageProgress);
        }

        private void NotifyFinished()
        {
            _isPackageOperationEnded = true;

            var packageProgress = ConstructPackageProgress(PackagePhases.Finished);
            _whenPackageProgressChangedSubject.OnNext(packageProgress);
            _whenPackageProgressChangedSubject.OnCompleted();
        }

        private void NotifyErrored(Exception exception)
        {
            _isPackageOperationEnded = true;

            PackageOperationException = exception;
            _whenPackageProgressChangedSubject.OnError(exception);
        }

        private PackageProgress ConstructPackageProgress(PackagePhases phase)
        {
            var packageProgress = new PackageProgress
            {
                CurrentPhase = phase,
                EntriesRead = _doneEntries,
                EntriesTotal = _entriesTotal,
                ContentType = ContentType,
                Name = ContentType.ToString(),
            };
            return packageProgress;
        }

        #endregion Methods
    }
}
