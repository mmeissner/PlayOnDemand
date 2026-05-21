#region Licence
/****************************************************************
 *  Filename: InstallationManager.cs
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
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using NLog;

namespace LeapVR.Shell.Controllers.Platform.Installation
{
    internal class InstallationManager
    {
        #region Properties and Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAppInstallationRepository _appInstallationRepository;
        private readonly IDiskController _diskController;
        #endregion

        #region Constructor
        internal InstallationManager(
            IAppInstallationRepository appInstallationRepository,
            IDiskController diskControllers)
        {
            QuickLeap.AssertNotNull(appInstallationRepository, diskControllers);
            _diskController = diskControllers;
            _appInstallationRepository = appInstallationRepository;
        }
        #endregion

        #region Container Methods
        /// <summary>
        /// Creates the installation process to be provided to the Manager to start an installation.
        /// </summary>
        /// <param name="container">The container for the installation.</param>
        /// <param name="installationProcess">The installation process.</param>
        /// <returns></returns>
        internal bool CreateInstallationProcess(IAppInstallationContainer<IContainerPackage> container, out IInstallationProcess installationProcess)
        {
            QuickLeap.AssertNotNull(container);

            Logger.Info( $"Application installation requested for container: Type = `{container.GetType()}`, ApplicationGuid = `{container.ApplicationGuid}`.");
            installationProcess = null;
            switch (container)
            {
                case IAppInstallationContainer<IContainerPackage> containerV2:
                    installationProcess = new InstallationProcess(containerV2, _diskController);
                    break;
                default:
                    Logger.Error($"Unsupported type of container; Type = `{container?.GetType()}`.");
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether this container can be install.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        internal CanInstallStatus CanInstall(IAppInstallationContainer<IContainerPackage> container)
        {
            QuickLeap.AssertNotNull(container);
            var retval = InternalCanInstall(container.ApplicationGuid);
            if (retval != CanInstallStatus.ReadyToInstall)
            {
                return retval;
            }
            try
            {
                container.AssertCoherence();
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Container seems to be broken!");
                return CanInstallStatus.ContainerBroken;
            }

            var packages = container.GetPackages();
            if (!_diskController.CanStorePackages(packages)) return CanInstallStatus.NotEnoughSpace;
            return retval;
        }
        #endregion

        #region Installation Manager Methods
         /// <summary>
        /// Must be called by the Installation Process before Extraction/Platform Install
        /// </summary>
        /// <param name="installationData">The install process data.</param>
        internal void PreInstallation(InstallProcessData installationData)
         {
             var canInstallState = CanInstall(installationData.InstallationData.ApplicationGuid);
             if(canInstallState != CanInstallStatus.ReadyToInstall)
             {
                 throw new Exception($"Application State is {canInstallState} but needs to be {CanInstallStatus.ReadyToInstall}");
             }
            _appInstallationRepository.Store(installationData.InstallationData);
        }

        /// <summary>
        /// Must be called by the Installation Process after the extraction.
        /// </summary>
        /// <param name="installationData">The installation data.</param>
        /// <returns></returns>
        internal InstallationState PostInstallation(InstallProcessData installationData)
        {
            var installationDataDb = _appInstallationRepository.Get(installationData.InstallationData.ApplicationGuid);
            InstallationState retval = InstallationState.Unknown;
            if (installationDataDb == null)
            {
                Logger.Error(
                    $"Could not find any Installation Data for ApplicationGuid={installationData.InstallationData.ApplicationGuid}");
                retval = InstallationState.InstallationFailed;
            }
            else
            {
                installationDataDb.InstallationState = installationData.Exception == null ? InstallationState.Installed : InstallationState.InstallationFailed;

                //Update the PluginGuid as VBox Container cant evaluate PluginGuid before installlation
                installationDataDb.PlatformPluginGuid = installationData.PlatformData.PlatformPluginId;
                _appInstallationRepository.Store(installationDataDb);

                if (installationDataDb.InstallationState != InstallationState.Installed)
                {
                    retval = installationDataDb.InstallationState;
                }
            }
            
            //Finialize Installation
            installationData.FinalizeAction?.Invoke(installationData);
            return retval;
        }

        /// <summary>
        /// Must be called by the Uninstallation Process befor any files get deleted.
        /// </summary>
        /// <param name="uninstallProcessData">The uninstall process data.</param>
        internal void PreDelete(UninstallProcessData uninstallProcessData)
        {
            var installationData = _appInstallationRepository.Get(uninstallProcessData.ApplicationGuid);
            installationData.InstallationState = InstallationState.UninstallationInProgress;
            _appInstallationRepository.Store(installationData);
        }

        /// <summary>
        /// Must be called by the Uninstallation Processes after files got deleted
        /// </summary>
        /// <param name="uninstallProcessData">The uninstall process data.</param>
        /// <returns></returns>
        internal InstallationState PostDelete(UninstallProcessData uninstallProcessData)
        {
            InstallationState retval = InstallationState.UninstallationFailed;

            if (uninstallProcessData.Exception == null)
            {
                _appInstallationRepository.Delete(uninstallProcessData.ApplicationGuid);
                retval = InstallationState.UninstallationSucceeded;
            }
            else
            {
                var uninstallationData =  _appInstallationRepository.Get(uninstallProcessData.ApplicationGuid);
                uninstallationData.InstallationState = InstallationState.UninstallationFailed;
                _appInstallationRepository.Store(uninstallationData);
            }
            //Finialize Installation
            uninstallProcessData.FinalizeAction?.Invoke(uninstallProcessData);
            return retval;
        }
        #endregion

        #region Container Installation Methods
        /// <summary>
        /// Begins the installation process.
        /// </summary>
        /// <param name="installationProcess">The installation process.</param>
        /// <param name="finializeAction">The post installation action.</param>
        internal void StartInstallationProcess(
                IInstallationProcess installationProcess,
                Action<InstallProcessData> finializeAction)
        {
            installationProcess.BeginInstall(this, finializeAction);
        }

        /// <summary>
        /// Starts the uninstallation process.
        /// </summary>
        /// <param name="uninstallationProcess">The uninstallation process.</param>
        /// <param name="finalizeAction">The finalize action.</param>
        internal void StartUninstallationProcess(
                IUninstallationProcess uninstallationProcess,
                Action<UninstallProcessData> finalizeAction)
        {
            uninstallationProcess.BeginUninstall(this, finalizeAction);
        }
        #endregion

        #region Platform Installation Methods
        /// <summary>
        /// Creates the Directory Structure for the Platform App and Persists
        /// the Files from PlatformDisplayDataPackage 
        /// </summary>
        /// <returns>The Updated Object with local persistance references</returns>
        internal void InstallPlatformApp(PlatformDisplayDataPackage mainPackage)
        {
           // Call Store Package on DiskController
            _diskController.StorePackage(mainPackage);
        } 
        #endregion

        #region Uninstallation Methods
        /// <summary>
        /// Creates the uninstallation process.
        /// </summary>
        /// <param name="displayInfo"></param>
        /// <param name="uninstallationProcess">The uninstallation process.</param>
        /// <returns></returns>
        internal bool CreateUninstallationProcess(IAppDisplayInfo displayInfo,AppUninstallationType uninstallationType, out IUninstallationProcess uninstallationProcess)
        {
            return InternalCreateUninstallationProcess(displayInfo,uninstallationType, out uninstallationProcess);
        }

        /// <summary>
        /// Creates the uninstallation process with limited displayInformation
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <param name="uninstallationProcess">The uninstallation process.</param>
        /// <returns></returns>
        internal bool CreateUninstallationProcess(Guid applicationGuid,AppUninstallationType uninstallationType, out IUninstallationProcess uninstallationProcess)
        {
            var installationData = _appInstallationRepository.Get(applicationGuid);
            uninstallationProcess = null;
            if (installationData == null) return false;
            return InternalCreateUninstallationProcess(
                    new AppDisplayInfo(applicationGuid) {Name = installationData.DisplayName},uninstallationType, out uninstallationProcess);
        }        
        #endregion

        #region Get States Methods
        /// <summary>
        /// Gets the all installed applications for a platform.
        /// </summary>
        /// <param name="platformId">The platform identifier.</param>
        /// <returns></returns>
        internal IEnumerable<IAppInstallationData> GetInstalledByPlatform(Guid platformId)
        {
            return _appInstallationRepository.GetAllByPlatformId(platformId);
        }

        /// <summary>
        /// Gets the installation state of the application.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns></returns>
        internal InstallationState GetInstallationState(Guid applicationGuid)
        {
            var state = _appInstallationRepository.Get(applicationGuid);
            return state?.InstallationState ?? InstallationState.NotInstalled;
        }

        /// <summary>
        /// Determines whether this instance can install the specified application unique identifier.
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns></returns>
        internal CanInstallStatus CanInstall(Guid applicationGuid)
        {
            return InternalCanInstall(applicationGuid);
        }

        /// <summary>
        /// Determines whether this application with that Guid can be uninstalled
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <returns></returns>
        internal CanUninstallStatus CanUninstall(Guid applicationGuid)
        {
            var installationData = _appInstallationRepository.Get(applicationGuid);
            if (installationData == null)return CanUninstallStatus.NotInstalled;
            if (installationData.InstallationState != InstallationState.Installed)return CanUninstallStatus.BrokenCanUninstall;
            return CanUninstallStatus.ReadyToUninstall;
        }
        #endregion

        /// <summary>
        /// Tries to remove broken/failed installations.
        /// </summary>
        internal void CleanUpInstallations()
        {
            var brokenInstallations = _appInstallationRepository.GetAll().Where(q => q.InstallationState != InstallationState.Installed).ToArray();
            foreach (var brokenInstall in brokenInstallations.Select(q => $"ApplicationGuid={q.ApplicationGuid} , DisplayName={q.DisplayName}"))
            {
                Logger.Warn($"Found broken Installation: {brokenInstall}");
            }

            foreach (var brokenInstallation in brokenInstallations)
            {
                Logger.Warn($"Broken Installation Cleanup for: ApplicationGuid={brokenInstallation.ApplicationGuid} , DisplayName={brokenInstallation.DisplayName}");
                try
                {
                    _diskController.RemoveAllApplicationData(brokenInstallation.ApplicationGuid);
                    _appInstallationRepository.Delete(brokenInstallation.ApplicationGuid);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Problem occured during cleanup.");
                }
            }
        }

        #region Private Methods
        private CanInstallStatus InternalCanInstall(Guid applicationGuid)
        {
            if(applicationGuid == Guid.Empty)
            {
                Logger.Warn("Tried to Install an Application with Empty Guid!");
                return CanInstallStatus.ContainerBroken;
            }
            var installationData = _appInstallationRepository.Get(applicationGuid);
            if (installationData != null)
            {
                return CanInstallStatus.AlreadyInstalled;
            }
            return CanInstallStatus.ReadyToInstall;
        }
        private bool InternalCreateUninstallationProcess(IAppDisplayInfo displayInfo,AppUninstallationType uninstallationType, out IUninstallationProcess uninstallationProcess)
        {
            uninstallationProcess = null;
            if(displayInfo == null) return false;
            var installationData = _appInstallationRepository.Get(displayInfo.ApplicationGuid);

            CanUninstallStatus canUninstallResult;
            if (installationData == null)canUninstallResult= CanUninstallStatus.NotInstalled;
            else if (installationData.InstallationState != InstallationState.Installed)canUninstallResult= CanUninstallStatus.BrokenCanUninstall;
            else canUninstallResult= CanUninstallStatus.ReadyToUninstall;

            if (canUninstallResult == CanUninstallStatus.ReadyToUninstall || canUninstallResult == CanUninstallStatus.BrokenCanUninstall)
            {
                UninstallProcessData uninstallData = new UninstallProcessData(displayInfo,installationData, uninstallationType);
                uninstallationProcess = new UninstallationProcess(uninstallData, _diskController);
                return true;
            }
            return false;
        }
        #endregion
    }
}
