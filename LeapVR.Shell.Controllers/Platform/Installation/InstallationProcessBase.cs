#region Licence
/****************************************************************
 *  Filename: InstallationProcessBase.cs
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
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Platform;
using NLog;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    internal abstract class InstallationProcessBase<TContainer> : IInstallationProcess where TContainer : IAppInstallationContainer<IContainerPackage>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region Properties & Fields
        private readonly HashSet<IProgressAwarePackage> _donePackages = new HashSet<IProgressAwarePackage>();
        private IProgressAwarePackage _lastAwarePackage = null;
        private long _allPackagesTotalDoneFileSize =0;

        private readonly ReplaySubject<InstallationProgress> _whenInstallationProgressChangedSubject;
        private readonly Subject<int> _whenPercentageDoneChangedSubject;
        private readonly IDiskController _diskController;
        private readonly long _totalBytesToExtract;
        private volatile int _percentageDone;
        private InstallProcessData InstallData { get; set; }

        public Guid ApplicationGuid { get; }
        public string ApplicationName { get; }
        public byte[] ApplicationThumbnail { get; }
        public int PercentageDone => _percentageDone;
        public IObservable<InstallationProgress> WhenInstallationProgressChanged { get; }
        public IObservable<int> WhenPercentageDoneChanged { get; }
        public Exception Exception => InstallData.Exception;

        protected TContainer Container { get; }
        protected IEnumerable<IContainerPackage> Packages { get; }
        protected IContainerPackage CurrentPackage { get; set; }
        #endregion Properties & Fields

        #region Constructors
        internal InstallationProcessBase(TContainer container, IDiskController diskController)
        {
            QuickLeap.AssertNotNull(container, diskController);
            Container = container;
            Packages = container.GetPackages();
            ApplicationGuid = container.ApplicationGuid;
            ApplicationName = container.DisplayName;
            ApplicationThumbnail = container.ThumbnailAsBytes;

            _whenInstallationProgressChangedSubject = new ReplaySubject<InstallationProgress>();
            WhenInstallationProgressChanged = _whenInstallationProgressChangedSubject.AsObservable();

            _whenPercentageDoneChangedSubject = new Subject<int>();
            WhenPercentageDoneChanged = _whenPercentageDoneChangedSubject.AsObservable();

            _totalBytesToExtract = Container.TotalFilesSize;
            _diskController = diskController;
        }

        #endregion Constructors

        #region Methods

        public void BeginInstall(InstallationManager installationManager, Action<InstallProcessData> finalizeAction)
        {
            QuickLeap.AssertNotNull(installationManager);

            //Get Pre Install Data from Container
            //Set Finalizer from Instructor to Data
            InstallData = new InstallProcessData(Container) {FinalizeAction = finalizeAction};
            Task.Run(() =>
            {
                try
                {
                    Install(installationManager);
                }
                catch
                {
                    //Swallow
                }
            });
        }

        private void Install(InstallationManager installationManager)
        {
            try
            {
                try
                {
                    _whenInstallationProgressChangedSubject.OnNext(
                        ConstructInstallationProgress(InstallationPhases.Started, CurrentPackage));

                    //Check if we can Install
                    var canInstallResult = installationManager.CanInstall(Container);
                    switch (canInstallResult)
                    {
                        case CanInstallStatus.Unknown:
                            throw new InvalidOperationException(
                                "Cannot install application; Controller did not respond with sufficent result.");
                        case CanInstallStatus.ReadyToInstall:
                            break;
                        case CanInstallStatus.AlreadyInstalled:
                        case CanInstallStatus.NotEnoughSpace:
                            throw new InvalidOperationException("Cannot install application; Insufficient HardDisk Space!");
                        case CanInstallStatus.ContainerBroken:
                            throw new InvalidOperationException("Cannot install application; Container seems broken!");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    //Provide this data to the Installation Manager to let him know about the container data
                    installationManager.PreInstallation(InstallData);

                    //Call Install on the concrete class to let it do its thing
                    InstallLogic(_diskController);
                }
                //Catch any exception that can occur on the way and set it to InstallData
                catch (Exception e)
                {
                    InstallData.Exception = e;
                }
                if (InstallData.Exception != null)
                {
                    //If we got an Exception we notify about that the Installation has Errored
                    _whenInstallationProgressChangedSubject.OnNext(
                        ConstructInstallationProgress(InstallationPhases.Error, CurrentPackage));
                }
                else
                {
                    try
                    {
                        //Now we notify that we are Done, there will be no progress updates anymore
                        _whenInstallationProgressChangedSubject.OnNext(ConstructInstallationProgress(InstallationPhases.Finished, CurrentPackage));
                        _whenInstallationProgressChangedSubject.OnCompleted();
                    }
                    catch (Exception exception)
                    {
                        //Oh noo a stupid Subscriber throwed, so lets log that as they are not allowed to!
                        Logger.Error(exception, "A subscriber of the Installation Progress throwed! Bad subscriber!");
                    }
                    //If everything went well we try to get the post Install data 
                    try
                    {
                        InstallData = SetPostInstallData(InstallData);
                    }
                    catch (Exception exception)
                    {
                        //Ohhh no we failed again to get the final data to make the app proper, lets set exception and inform about the error
                        InstallData.Exception = exception;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception,"We encountered a fatal error during installation!");
            }
            //We need to inform in any case that we are done!
            installationManager.PostInstallation(InstallData);
        }

        protected abstract InstallProcessData SetPostInstallData(InstallProcessData data);
        protected abstract void InstallLogic(IDiskController diskController);

        protected void OnPackageProgressChanged(IProgressAwarePackage package)
        {
            if(!_donePackages.Contains(package))
            {
                _donePackages.Add(package);
                if(_lastAwarePackage != null)
                    _allPackagesTotalDoneFileSize = _allPackagesTotalDoneFileSize + _lastAwarePackage.TotalFilesSize;
                _lastAwarePackage = package;
            }
            _percentageDone = Convert.ToInt32((_allPackagesTotalDoneFileSize + package.DoneFilesSize) * 100 / _totalBytesToExtract);
            _whenPercentageDoneChangedSubject.OnNext(_percentageDone);

            var installationProgress = ConstructInstallationProgress(InstallationPhases.InProgress, package);
            _whenInstallationProgressChangedSubject.OnNext(installationProgress);
        }

        private InstallationProgress ConstructInstallationProgress(InstallationPhases phase, IProgressAwarePackage currentInstallingPackage)
        {
            var progress = new InstallationProgress(
                ApplicationGuid,
                ApplicationName,
                phase,
                _percentageDone,
                InstallData.Exception,
                currentInstallingPackage,
                Packages.ToArray<IProgressAwarePackage>()
            );
            return progress;
        }
        #endregion Methods
    }
}
