#region Licence
/****************************************************************
 *  Filename: NewAppInstallationContainer.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using LeapVR.Shared.Lib;
using LeapVR.Shared.Lib.Objects;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using IAppInstallationHeaderSerializer = LeapVR.Shell.Domain.Models.Container.IAppInstallationHeaderSerializer;

namespace LeapVR.Shell.Modules.Container
{
    public class NewAppInstallationContainer : INewApplicationInstallationContainer
    {
        private readonly List<NewPackage> _addedPackages;
        private readonly IAppInstallationHeaderSerializer _headerSerializer;
        private readonly ReplaySubject<Empty> _whenContainerCreationStartedSubject;
        private readonly Subject<Empty> _whenProgressChangedSubject;
        private readonly ReplaySubject<Empty> _whenContainerCreationEndedSubject;

        private int _wasContainerCreationStarted; //0 = false, 1 = true
        private int _isContainerCreationEnded; // 0 = false, 1 = true

        internal ContainerModule ContainerModule { get; }


        #region Properties & Fields
        public Guid ApplicationGuid { get; }
        public int Version { get; set; }
        public string DisplayName { get; set; } = null;
        public byte[] ThumbnailAsBytes { get; set; }
        public bool WasContainerCreationStarted => _wasContainerCreationStarted != 0;
        public bool IsContainerCreationEnded => _isContainerCreationEnded != 0;
        public Exception ContainerCreationException { get; private set; }
        public int TotalFilesCount => _addedPackages.Sum(q => q.TotalFilesCount);
        public int DoneFilesCount => _addedPackages.Sum(q => q.DoneFilesCount);
        public long TotalFilesSize => _addedPackages.Sum(q => q.TotalFilesSize);
        public long DoneFilesSize => _addedPackages.Sum(q => q.DoneFilesSize);
        public IObservable<Empty> WhenContainerCreationStarted { get; }
        public IObservable<Empty> WhenProgressChanged { get; }
        public IObservable<Empty> WhenContainerCreationEnded { get; }
        #endregion Properties & Fields

        #region Constructors
        internal NewAppInstallationContainer(ContainerModule containerModule, IAppInstallationHeaderSerializer headerSerializer, Guid applicationGuid) // TODO [RM]: generate Guid inside instead of provided from outside?
        {
            QuickLeap.AssertNotNull(headerSerializer, containerModule);
            _headerSerializer = headerSerializer;
            ApplicationGuid = applicationGuid;
            ContainerModule = containerModule;

            _whenContainerCreationStartedSubject = new ReplaySubject<Empty>();
            WhenContainerCreationStarted = _whenContainerCreationStartedSubject.AsObservable();

            _whenProgressChangedSubject = new Subject<Empty>();
            WhenProgressChanged = _whenProgressChangedSubject.AsObservable();

            _whenContainerCreationEndedSubject = new ReplaySubject<Empty>();
            WhenContainerCreationEnded = _whenContainerCreationEndedSubject.AsObservable();

            _addedPackages = new List<NewPackage>();
        }
        #endregion Constructors

        #region Methods
        public void Initialize(){}

        public void AssertCoherence(){}

        public INewPackage AddNewPackage(ContentType contentType)
        {
            var newPackage = new NewPackage(this, ApplicationGuid, contentType);
            _addedPackages.Add(newPackage);

            return newPackage;
        }

        public IEnumerable<INewPackage> GetPackages()
        {
            return new ReadOnlyCollection<NewPackage>(_addedPackages);
        }

        public void SaveToFiles(string headerFilePath)
        {
            QuickLeap.AssertNotNull(headerFilePath, ThumbnailAsBytes, DisplayName);
            var containerFilePath = headerFilePath;

            QuickLeap.OperateInterlockedFlag(ref _wasContainerCreationStarted, true, false);
            try
            {
                NotifyStarted();

                if (!containerFilePath.ToLowerInvariant().EndsWith(ContainerModule.HeaderFileExtension))
                {
                    containerFilePath = containerFilePath + ContainerModule.HeaderFileExtension;
                }

                using (var fs = File.Open(containerFilePath, FileMode.CreateNew, FileAccess.Write))
                {
                    // Serializes packages data to file stream, return dictionary with offsets for each package
                    var packageDataFileOffsets = SerializePackages(fs, _addedPackages);

                    // Saves position in stream where header data starts and serializes header into file stream
                    long headerStartPosition = fs.Position;
                    var header = new NewAppInstallationHeader
                    {
                        ApplicationGuid = ApplicationGuid,
                        Version = Version,
                        TotalFilesSize = TotalFilesSize,
                        TotalFilesCount = TotalFilesCount,
                        PackageDataFileOffsets = packageDataFileOffsets,

                        DisplayName = DisplayName,
                        ThumbnailAsBytes = ThumbnailAsBytes,
                    };
                    _headerSerializer.SaveToStream(fs, header);

                    // Saves position where header data starts as last 8 bytes (long) of file
                    using (var bw = new BinaryWriter(fs, Encoding.Default, true))
                    {
                        bw.Write(headerStartPosition);
                    }
                }
            }
            catch (Exception e)
            {
                QuickLeap.OperateInterlockedFlag(ref _isContainerCreationEnded, true);
                NotifyEnded(e);
                throw;
            }

            QuickLeap.OperateInterlockedFlag(ref _isContainerCreationEnded, true);
            NotifyEnded(null);
        }

        private Dictionary<IPackageData, long> SerializePackages(Stream destination, IEnumerable<NewPackage> packages)
        {
            int longSize = sizeof(long);

            var packageDataFileOffsets = new Dictionary<IPackageData, long>();
            foreach (var package in _addedPackages)
            {
                using (package.WhenProgressChanged.Subscribe(q => NotifyChanged()))
                {
                    var beginningPosition = destination.Position;
                    destination.Write(Enumerable.Repeat<byte>(0xFF, longSize).ToArray(), 0, longSize); // placeholder for packageDataLength (8bytes = long); will be overwriten

                    long packageDataLength;
                    using (var ss = new SubStream(destination, null, true))
                    {
                        packageDataLength = package.WriteToStream(ss);
                    }
                    var endingPosition = destination.Position;

                    destination.Seek(beginningPosition, SeekOrigin.Begin);
                    using (var bw = new BinaryWriter(destination, Encoding.Default, true))
                    {
                        bw.Write(packageDataLength); // overwrite placeholder with actual packageDataLength
                    }

                    destination.Seek(endingPosition, SeekOrigin.Begin);
                    packageDataFileOffsets.Add(package, beginningPosition);
                }
            }

            return packageDataFileOffsets;
        }

        private void NotifyStarted()
        {
            _whenContainerCreationStartedSubject.OnNext(Empty.Get);
            _whenContainerCreationStartedSubject.OnCompleted();
        }

        private void NotifyChanged()
        {
            _whenProgressChangedSubject.OnNext(Empty.Get);
        }

        private void NotifyEnded(Exception e)
        {
            _whenProgressChangedSubject.OnCompleted();

            if (e == null)
            {
                _whenContainerCreationEndedSubject.OnNext(Empty.Get);
                _whenContainerCreationEndedSubject.OnCompleted();
                return;
            }

            ContainerCreationException = e;
            _whenContainerCreationEndedSubject.OnError(e);
        }

        #endregion Methods
    }
}
