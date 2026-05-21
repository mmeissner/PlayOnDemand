#region Licence
/****************************************************************
 *  Filename: ZipContainer.cs
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
using System.Text;
using LeapVR.Shared.Lib.Objects;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Modules.Container
{
    public abstract class ZipContainer : IContainer<IContainerPackage>
    {
        #region Properties & Fields

        public Guid ApplicationGuid { get; private set; }

        public int Version { get; private set; }

        public int TotalFilesCount { get; private set; }

        public long TotalFilesSize { get; private set; }

        /// <summary>
        /// Contains deserialized <see cref="IZipContainerHeader"/>.
        /// Is assigned by <see cref="Initialize"/> method when it calls <see cref="DeserializeHeader"/> abstract method.
        /// </summary>
        protected IZipContainerHeader Header { get; private set; }

        #endregion Properties & Fields

        #region Constructors

        //

        #endregion Constructors

        #region Methods
        
        /// <summary>
        /// Opens Stream to single ZIP archive (containing single package) located at <see cref="fileOffset"/> position in data stream opened by <see cref="OpenDataStreamForRead"/>.
        /// Caller to this method must close Stream he gets from here.
        /// </summary>
        /// <param name="fileOffset"></param>
        /// <returns></returns>
        internal Stream OpenZipStream(long fileOffset)
        {
            var dataStream = OpenDataStreamForRead(); // will be closed by SubStream when SubStream itself is closed
            dataStream.Seek(fileOffset, SeekOrigin.Begin);

            long size;
            using (var br = new BinaryReader(dataStream, Encoding.Default, true))
            {
                size = br.ReadInt64();
            }

            return new SubStream(dataStream, size);
        }

        /// <inheritdoc />
        /// <summary>
        /// Deserializes <see cref="P:LeapVR.Shell.Modules.Container.ZipContainer.Header" /> using <see cref="M:LeapVR.Shell.Modules.Container.ZipContainer.DeserializeHeader" /> and populates itself with data read from it.
        /// When overriden should call base.<see cref="M:LeapVR.Shell.Modules.Container.ZipContainer.Initialize" /> first, then populate inheriting object with additional data from <see cref="P:LeapVR.Shell.Modules.Container.ZipContainer.Header" /> object that is already deserialized there.
        /// </summary>
        public virtual void Initialize()
        {
            Header = DeserializeHeader();
            ApplicationGuid = Header.ApplicationGuid;
            Version = Header.Version;
            TotalFilesCount = Header.TotalFilesCount;
            TotalFilesSize = Header.TotalFilesSize;
        }

        /// <inheritdoc />
        /// <summary>
        /// Asserts that container is is valid state ready to be used. Throws <see cref="T:System.InvalidOperationException" /> when there is a problem.
        /// When overriden should call base.<see cref="M:LeapVR.Shell.Modules.Container.ZipContainer.AssertCoherence" /> first, then perform custom required checks and throw <see cref="T:System.InvalidOperationException" /> with description when problem occured.
        /// </summary>
        public virtual void AssertCoherence()
        {
            if (Header == null)
            {
                throw new InvalidOperationException("_header == null");
            }

            if (ApplicationGuid == Guid.Empty)
            {
                throw new InvalidOperationException("ApplicationGuid == Guid.Empty");
            }

            if (TotalFilesCount == 0)
            {
                throw new InvalidOperationException("TotalFilesCount == 0");
            }

            if (TotalFilesSize == 0)
            {
                throw new InvalidOperationException("TotalFilesSize == 0");
            }
        }

        public IEnumerable<IContainerPackage> GetPackages()
        {
            var result = new List<IContainerPackage>();
            if (Header.PackageDataFileOffsets != null)
            {
                foreach (var kv in Header.PackageDataFileOffsets)
                {
                    var headerPackage = kv.Key;
                    var offset = kv.Value;
                    result.Add(new ZipReadablePackage(this, offset,headerPackage));
                }
            }
            return result;
        }

        /// <summary>
        /// Inheriting class should override this method with logic deserializing data to <see cref="IZipContainerHeader"/> object.
        /// </summary>
        /// <returns></returns>
        protected abstract IZipContainerHeader DeserializeHeader();

        /// <summary>
        /// Inheriting class should override this method to provide Data Stream at start offset ready to be jumped to selected package offset.
        /// </summary>
        /// <returns></returns>
        protected abstract Stream OpenDataStreamForRead();

        #endregion Methods
    }
}
