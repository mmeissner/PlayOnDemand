#region Licence
/****************************************************************
 *  Filename: INewApplicationInstallationContainer.cs
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
using LeapVR.Shared.Lib;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Interface to be used in process of creating new <see cref="IAppInstallationContainer{T}"/> releated files asynchronously.
    /// Consists of members used to define container content and to control asynchronous container creation process.
    /// </summary>
    public interface INewApplicationInstallationContainer : IAppInstallationContainer<INewPackage>
    {
        /// <summary>
        /// Version of container. Reserved for further use.
        /// </summary>
        new int Version { get; set; }

        /// <summary>
        /// Thumbnail image stored as bytes. To be displayed e.g. near application name on list of installed applications.
        /// </summary>
        new byte[] ThumbnailAsBytes { get; set; }

        /// <summary>
        /// Display name of Application. To be displayed e.g. on list of installed applications.
        /// </summary>
        new string DisplayName { get; set; }

        /// <summary>
        /// Indicates if container creation process of this container was requested already.
        /// </summary>
        bool WasContainerCreationStarted { get; }

        /// <summary>
        /// Indicates if container creation process of this container has already finished.
        /// </summary>
        bool IsContainerCreationEnded { get; }

        /// <summary>
        /// Indicates exception occured in asynchronous process of creating container. Set after creation process finishes. Not null indicates failure.
        /// </summary>
        Exception ContainerCreationException { get; }

        /// <summary>
        /// Indicates amount of files in all packages that were already processed succesfully. Together with <see cref="IContainer{T}.TotalFilesCount"/> can be used to measure progress of container creation process.
        /// </summary>
        int DoneFilesCount { get; }

        /// <summary>
        /// Indicates size of files in all packages that were already processed succesfully. Together with <see cref="IContainer{T}.TotalFilesSize"/> can be used to measure progress of container creation process.
        /// </summary>
        long DoneFilesSize { get; }

        /// <summary>
        /// Observable fired when container creation process has been started.
        /// When subscribed to after process was started pushes notification imiedietly (like ReplaySubject).
        /// </summary>
        IObservable<Empty> WhenContainerCreationStarted { get; }

        /// <summary>
        /// Observable fired when progress releated data (<see cref="DoneFilesCount"/>, <see cref="DoneFilesSize"/>) has changed.
        /// Hot type observable with no memory (like Subject).
        /// </summary>
        IObservable<Empty> WhenProgressChanged { get; }

        /// <summary>
        /// Called to add new package (see <see cref="INewPackage"/>) to current container.
        /// </summary>
        /// <param name="contentType"><see cref="ContentType"/> of data to be stored in new package</param>
        /// <returns>Object to be used when adding files to newly created package</returns>
        INewPackage AddNewPackage(ContentType contentType);

        /// <summary>
        /// Called to start container creation process after specifying container content and metadata.
        /// Will begin asynchronous process of creating container and writing releated files to specified locations (<see cref="headerFilePath"/> and related data file path).
        /// Creation process releated notifications will be fired when needed, as well as package creation progress properties will be set.
        /// </summary>
        /// <param name="headerFilePath">Location for header file to write to. If provided file extension is NOT as expected, expected extension will be added</param>
        void SaveToFiles(string headerFilePath);
    }
}
