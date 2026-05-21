#region Licence
/****************************************************************
 *  Filename: AppInstallationContainer.cs
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
using System.IO;
using System.Text;
using LeapVR.Shared.Lib.Objects;
using LeapVR.Shell.Domain.Models.Container;


namespace LeapVR.Shell.Modules.Container
{
    public class AppInstallationContainer : ZipContainer, IAppInstallationContainer<IContainerPackage>
    {
        #region Properties & Fields
        public string DisplayName { get; private set; }
        public byte[] ThumbnailAsBytes { get; private set; }

        private readonly IAppInstallationHeaderSerializer _headerSerializer;
        private readonly string _containerFilePath;

        #endregion Properties & Fields

        #region Constructors

        internal AppInstallationContainer(IAppInstallationHeaderSerializer headerSerializer, string containerFilePath)
        {
            _headerSerializer = headerSerializer;
            _containerFilePath = containerFilePath;
        }

        #endregion Constructors

        #region Methods

        public override void Initialize()
        {
            base.Initialize();
            var header = (IAppInstallationHeader)Header;
            ThumbnailAsBytes = header.ThumbnailAsBytes;
            DisplayName = header.DisplayName;
        }

        protected override IZipContainerHeader DeserializeHeader()
        {
            // longSize = 8 (bytes)
            const int longSize = sizeof(long);

            // We open _containerFilePath file for read (FileShare.Read so a
            // concurrent reader / extract worker is not blocked).
            using (var fs = new FileStream(_containerFilePath, FileMode.Open,
                FileAccess.Read, FileShare.Read))
            {
                // We reads offset where serialized header data starts:
                long headerStartPosition = ReadHeaderStartPosition(fs);

                // We calculate how long is serialized header data:
                long headerLength = fs.Length - headerStartPosition - longSize;

                // We go to headerStartPosition:
                fs.Seek(headerStartPosition, SeekOrigin.Begin);

                // We limit access to Stream only to precise length of headerLength
                // (so greedy deserializer will not read more data than it should):
                using (var headerStream = new SubStream(fs, headerLength, true))
                {
                    // We give limited stream to deserializer, he returns us the header.
                    return _headerSerializer.LoadFromStream(headerStream);
                }
            }
        }

        protected override Stream OpenDataStreamForRead()
        {
            FileStream fs = null;
            try
            {
                // FileShare.Read so parallel extract workers can each hold
                // their own FileStream + ZipFile on the same .vbox file
                // (see ZipReadablePackage.ExtractToDirectory).
                fs = new FileStream(_containerFilePath, FileMode.Open,
                    FileAccess.Read, FileShare.Read);

                // We reads offset where serialized header data starts:
                long headerStartPosition = ReadHeaderStartPosition(fs);

                // Packages data takes all space until headerStartPosition:
                long packagesDataLength = headerStartPosition;

                // We go to beggining of the file:
                fs.Seek(0, SeekOrigin.Begin);

                // We limit access to Stream only to precise length packages data
                // (so greedy code extracting packages will not read more data than it should):
                var packagesDataStream = new SubStream(fs, packagesDataLength);

                return packagesDataStream;
            }
            catch
            {
                // Dispose only in case of exception in this method's code;
                // Otherwise caller is required to dispose result Stream by himself.

                fs?.Dispose();
                throw;
            }
        }

        public override void AssertCoherence()
        {
            base.AssertCoherence();

            if (!File.Exists(_containerFilePath))
            {
                throw new InvalidOperationException($"_containerFilePath = `{_containerFilePath}` does not exists.");
            }
        }

        private long ReadHeaderStartPosition(FileStream containerFileStream)
        {
            // longSize = 8 (bytes)
            const int longSize = sizeof(long);

            // We save position to restore it later to not cause side effects:
            long initStreamPosition = containerFileStream.Position;

            // headerStartPosition offset is long number stored at the very end of _containerFilePath;
            // We first go to `file_end - sizeof(long)`:
            containerFileStream.Seek(-longSize, SeekOrigin.End);

            // Then we read headerStartPosition from there:
            long headerStartPosition;
            using (var br = new BinaryReader(containerFileStream, Encoding.Default, true))
            {
                headerStartPosition = br.ReadInt64();
            }

            // We reset position so we don't cause side effects:
            containerFileStream.Seek(initStreamPosition, SeekOrigin.Begin);

            return headerStartPosition;
        }

        #endregion Methods
    }
}
