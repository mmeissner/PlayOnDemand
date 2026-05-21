#region Licence
/****************************************************************
 *  Filename: AppInstallationFile.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Managers.UsbStorage;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using NLog;

namespace LeapVR.Shell.Modules.Container
{
    [FileSearchFilters(new []
    {
        "*" + Container.ContainerModule.HeaderFileExtension,
    })]
    public class AppInstallationFile : IAppInstallationFile
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        #region Properties & Fields

        public string AbsolutePath { get; set; }
        public string FileName { get; set; }
        public object Parent { get; set; }
        public IUsbStorage UsbStorage { get; set; }

        public IContainerModule ContainerModule { get; set; }

        public IAppInstallationContainer<IContainerPackage> AppinstallationContainer { get; private set; }
        public LoadedState LoadedState { get; private set; }

        private readonly ReplaySubject<LoadedState> _whenLoadFinishedSubject;
        public IObservable<LoadedState> WhenLoadFinished { get; }

        #endregion Properties & Fields

        #region Constructors

        public AppInstallationFile()
        {
            _whenLoadFinishedSubject = new ReplaySubject<LoadedState>();
            WhenLoadFinished = _whenLoadFinishedSubject.AsObservable();
        }

        #endregion Constructors

        #region Methods

        public LoadedState LoadFile()
        {
            var loadState = LoadedState.NotLoaded;
            try
            {
                if (ContainerModule == null)
                {
                    throw new InvalidOperationException($"{nameof(ContainerModule)} == null");
                }

                AppinstallationContainer = ContainerModule.GetAppInstallationContainer(AbsolutePath);
                loadState = LoadedState.Success;
            }
            catch (Exception e)
            {
                _logger.Warn(e, $"While loading file ({AbsolutePath}) esception occured ({e}).");
                loadState = LoadedState.Failure;
            }
            finally
            {
                LoadedState = loadState;
                _whenLoadFinishedSubject.OnNext(loadState);
                _whenLoadFinishedSubject.OnCompleted();
            }

            return loadState;
        }

        #endregion Methods
    }
}
