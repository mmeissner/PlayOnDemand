#region Licence
/****************************************************************
 *  Filename: ContainerModule.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container;
using IAppInstallationHeaderSerializer = LeapVR.Shell.Domain.Models.Container.IAppInstallationHeaderSerializer;

namespace LeapVR.Shell.Modules.Container
{
    public class ContainerModule : IContainerModule
    {
        #region Properties & Fields

        // All must be lowercase:
        public const string HeaderFileExtension = ".vbox";
        private readonly IAppInstallationHeaderSerializer _serializer;

        #endregion Properties & Fields

        #region Constructors

        public ContainerModule(IAppInstallationHeaderSerializer serializer)
        {
            QuickLeap.AssertNotNull(serializer);

            _serializer = serializer;
        }

        #endregion Constructors

        #region Methods

        public IAppInstallationContainer<IContainerPackage> GetAppInstallationContainer(string headerFilePath)
        {
            IAppInstallationContainer<IContainerPackage> container;
            var headerExtension = Path.GetExtension(headerFilePath)?.ToLowerInvariant();

            switch (headerExtension)
            {
                case HeaderFileExtension:
                    container = new AppInstallationContainer(_serializer, headerFilePath);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported headerExtension = `{headerExtension}`.");
            }

            container.Initialize();
            return container;
        }

        public INewApplicationInstallationContainer CreateNewApplicationInstallationContainer(Guid applicationGuid)
        {
            // TODO [RM]: generate new Guid here?
            // New containers use newest serializer, i.e. _v2Serializer
            return new NewAppInstallationContainer(this, _serializer, applicationGuid);
        }

        /// <summary>
        /// Opens an existing .vbox file for partial-edit. Returns an
        /// EditableAppInstallationContainer with the header already loaded;
        /// caller must call Open() before reading/writing.
        /// </summary>
        public EditableAppInstallationContainer OpenForEdit(string headerFilePath)
        {
            QuickLeap.AssertNotNull(headerFilePath);
            var headerExtension = Path.GetExtension(headerFilePath)?.ToLowerInvariant();
            if (headerExtension != HeaderFileExtension)
            {
                throw new InvalidOperationException(
                    $"Unsupported headerExtension = `{headerExtension}`.");
            }
            var editor = new EditableAppInstallationContainer(headerFilePath, _serializer);
            editor.Open();
            return editor;
        }

        #endregion Methods
    }
}
