#region Licence
/****************************************************************
 *  Filename: PlatformInstallationProcess.cs
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;
using NLog;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    class PlatformInstallationProcess : IPlatformInstallationProcessInfo
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AppPlatformInfo _appPlatformInfo;
        private readonly Platform _platform;
        private readonly ReplaySubject<IPlatformInstallationProgress> _whenInstallationProgressChangedSubject;
        private readonly InstallationManager _installationManager;
        private readonly Action<InstallProcessData> _finalizeAction;

        private InstallProcessData InstallData { get; set; }
        public PlatformInstallationProcess(
                AppPlatformInfo appPlatformInfo,
                Platform platform, InstallationManager installationManager,
                Action<InstallProcessData> finalizeAction)
        {
            _installationManager = installationManager;
            _finalizeAction = finalizeAction;
            _whenInstallationProgressChangedSubject = new ReplaySubject<IPlatformInstallationProgress>();
            WhenInstallationProgressChanged = _whenInstallationProgressChangedSubject.AsObservable();
            _appPlatformInfo = appPlatformInfo;
            _platform = platform;
        }
        public Guid ApplicationGuid => _appPlatformInfo.ApplicationGuid;
        public string ApplicationName => _appPlatformInfo.Name;
        public byte[] ApplicationThumbnail => _appPlatformInfo.Thumbnail;
        public IObservable<IPlatformInstallationProgress> WhenInstallationProgressChanged { get; }
        public Exception Exception { get; set; }

        internal void BeginInstall()
        {
            Logger.Debug("Begin of Installation was Requested, going to spawn new Task");
            //Get Pre Install Data
            //Set Finalizer from Instructor to Data
            Task.Run(
                    () =>
                    {
                        try
                        {
                            Install(_installationManager);
                        }
                        catch(Exception ex)
                        {
                            //Swallow
                            Logger.Error(ex, "Exception during Installation!");
                        }
                    }).Forget();
        }

        internal void ReportPlatformInstallation(PlatformInstallationPhase phase)
        {
            //Do here something for reporting the state
        }

        private void Install(InstallationManager installationManager)
        {
            try
            {
                var displayDataPackage = new PlatformDisplayDataPackage(_appPlatformInfo);
                Logger.Debug($"Created new {nameof(PlatformDisplayDataPackage)}= {displayDataPackage}");
                InstallData =
                        new InstallProcessData(_appPlatformInfo, new List<IPackageData> {displayDataPackage})
                        {
                                FinalizeAction = _finalizeAction,
                                DisplayData = displayDataPackage.GetDisplayData()
                        };

                try
                {
                    Logger.Debug($"Publishing new Installation Progress: Progress = {PlatformInstallationPhase.Started}");
                    _whenInstallationProgressChangedSubject.OnNext(
                            ConstructInstallationProgress(PlatformInstallationPhase.Started));

                    // Evaluate Type of Installation
                    IAppPlatformData appPlatformData = null;
                    var platformInstallState = _platform.GetPlatformInstallState(ApplicationGuid);
                    switch(platformInstallState)
                    {
                        case PlatformInstallState.Local:
                            if(!_platform.LocalInstallation(this, out appPlatformData))
                            {
                                throw new Exception("Platform Online Installation Failed!");
                            }
                            break;
                        case PlatformInstallState.Online:
                            if(!_platform.OnlineInstallation(this, out appPlatformData))
                            {
                                throw new Exception("Platform Online Installation Failed!");
                            }
                            break;
                        case PlatformInstallState.Unavailable:
                        case PlatformInstallState.Installing:
                        case PlatformInstallState.Error:
                            throw new Exception($"Application has an PlatformInstallState of {platformInstallState}");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    Logger.Debug("Received new PlatformData",appPlatformData);

                    //Provide this data to the Installation Manager to let him know about the Installation
                    //This will also set the state to installing
                    installationManager.PreInstallation(InstallData);

                    //Assign PlatformData
                    InstallData.PlatformData = appPlatformData;

                    //Let Installation Manager Persist Local required Data
                    installationManager.InstallPlatformApp(displayDataPackage);

                }
                //Catch any exception that can occure on the way and set it to InstallData
                catch(Exception e)
                {
                    InstallData.Exception = e;
                    Logger.Error(e);
                }

                if(InstallData.Exception != null)
                {
                    //If we got an Exception we Notifty about that the Installation has Errored
                    _whenInstallationProgressChangedSubject.OnNext(
                            ConstructInstallationProgress(PlatformInstallationPhase.Error));
                }
                else
                {
                    try
                    {
                        //Now we notify that we are Done, there will be no progress updates anymore
                        _whenInstallationProgressChangedSubject.OnNext(
                                ConstructInstallationProgress(PlatformInstallationPhase.Finished));
                        _whenInstallationProgressChangedSubject.OnCompleted();
                    }
                    catch(Exception exception)
                    {
                        //Oh noo a stupid Subscriber throwed, so lets log that as they are not allowed to!
                        Logger.Error(exception, "A subscriber of the Installation Progress throwed! Bad subscriber!");
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Fatal(exception, "We encountered a fatal error during installation!");
            }

            //We need to inform in any case that we are done!
            installationManager.PostInstallation(InstallData);

            //Inform the object about it's changed state
            _appPlatformInfo.OnStateChanged(PlatformAppUpdate.PlatformInstallation);
        }


        private IPlatformInstallationProgress ConstructInstallationProgress(PlatformInstallationPhase phase)
        {
            return new PlatformInstallationProgress(phase);
        }
    }

    public class PlatformInstallationProgress : IPlatformInstallationProgress
    {
        public PlatformInstallationPhase State { get; }
        public PlatformInstallationProgress(PlatformInstallationPhase state) { State = state; }
    }
}