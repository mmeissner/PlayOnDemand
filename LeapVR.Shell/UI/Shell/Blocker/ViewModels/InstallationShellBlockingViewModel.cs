#region Licence
/****************************************************************
 *  Filename: InstallationShellBlockingViewModel.cs
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
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Blocker.Abstract;
using NLog;

namespace LeapVR.Shell.UI.Shell.Blocker.ViewModels
{
    public class InstallationShellBlockingViewModel : ApplicationBlockShellViewModel
    {
        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private int _percentageDone;
        private Visibility _percentage = Visibility.Hidden;

        public int PercentageDone
        {
            get => _percentageDone;
            set
            {
                _percentageDone = value;
                NotifyOfPropertyChange();
            }
        }
        public Visibility PercentageVisibility
        {
            get => _percentage;
            set
            {
                if(value == _percentage) return;
                _percentage = value;
                NotifyOfPropertyChange();
            }
        }
        private string _information;
        public string Information
        {
            get { return _information; }
            set
            {
                _information = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors

        public InstallationShellBlockingViewModel(IInstallationProcessInfo process,IViewInputHandler inputHandler) : base(process, inputHandler)
        {
            try
            {
                //If used with OnDispatcher Extension it will not fire
                process.WhenInstallationProgressChanged.Subscribe(OnInstallationProgressChanged);
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Error during creation of InstallationShellBlockingViewModel, we didnt excpeted that");
            }
        }

        #endregion

        #region Methods

        private async void OnInstallationProgressChanged(InstallationProgress installationProgress)
        {
            if(Application.Current.CheckAccess())
            {
                try
                {
                    switch (installationProgress.InstallationPhase)
                    {
                        case InstallationPhases.Started:
                            //If used with OnDispatcher Extension it will not fire
                            installationProgress.Packages.Select(q => q.WhenPackageProgressChanged).Concat().Subscribe(OnPackageProgressChanged, OnError);
                            break;
                        case InstallationPhases.InProgress:
                            PercentageVisibility = Visibility.Visible;
                            if(installationProgress.PercentageDone == 0)break;
                            PercentageDone = installationProgress.PercentageDone;
                            break;
                        case InstallationPhases.Finished:
                            PercentageVisibility = Visibility.Hidden;
                            IsEnded = true;
                            HasError = false;
                            await Task.Delay(2000);
                            Close();
                            break;
                        case InstallationPhases.Error:
                            Information = Resources.Shell_InstallationBlocking_InstallationFailed;
                            PercentageVisibility = Visibility.Hidden;
                            IsEnded = true;
                            HasError = true;
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception,"Exception occured during OnInstallationProgressChanged");
                    Information = Resources.Shell_InstallationBlocking_InstallationFailed;
                    IsEnded = true;
                    HasError = true;
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => OnInstallationProgressChanged(installationProgress));
            }
        }

        private void OnError(Exception exception)
        {
            Logger.Error(exception, "Exception occured during OnInstallationProgressChanged");
            Information = Resources.Shell_InstallationBlocking_InstallationFailed;
            PercentageVisibility = Visibility.Hidden;
            IsEnded = true;
            HasError = true;
        }

        private void OnPackageProgressChanged(PackageProgress packageProgress)
        {
            if(Application.Current.CheckAccess())
            {
                try
                {
                    switch(packageProgress.CurrentPhase)
                    {
                        case PackagePhases.Started:
                            Information = $"{Resources.Shell_InstallationBlocking_Installing} {packageProgress.Name}";
                            break;
                        case PackagePhases.Reading:
                            Information =
                                    $"{Resources.ScanningPackages} {packageProgress.ContentType} - {packageProgress.EntriesRead}";
                            break;
                        case PackagePhases.Extracting:
                            Information =
                                    $"{Resources.Shell_InstallationBlocking_Installing} {packageProgress.ContentType}";
                            break;
                        case PackagePhases.Finished:
                            Information = $"{packageProgress.Name} {Resources.Installation_InstallationComplete}";
                            break;
                        case PackagePhases.Error:
                            break;
                    }
                }
                catch(Exception e)
                {
                    Logger.Error(e, "Exception encountered during OnPackage Progress Changed");
                    throw;
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => OnPackageProgressChanged(packageProgress));
            }
        }

        #endregion
        
    }
}
