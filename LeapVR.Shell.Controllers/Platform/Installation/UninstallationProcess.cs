#region Licence
/****************************************************************
 *  Filename: UninstallationProcess.cs
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
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Platform;
using NLog;

namespace LeapVR.Shell.Controllers.Platform.Installation
{

    internal class UninstallationProcess : IUninstallationProcess
    {
        #region Properties & Fields

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private int _isEnded; // 0 = false, 1 = true
        private volatile Exception _exception;
        private readonly ReplaySubject<Shared.Lib.Empty> _whenUninstallationEndedSubject;
        private int _isStarted; // 0 = false, 1 = true
        private readonly UninstallProcessData _uninstallProcessData;
        private InstallationManager _installationManager;
        private readonly IDiskController _diskController;

        public Guid ApplicationGuid => _uninstallProcessData.ApplicationGuid;
        public string ApplicationName => _uninstallProcessData.ApplicationName;
        public byte[] ApplicationThumbnail => _uninstallProcessData.ApplicationThumbnail;
        public bool IsEnded => _isEnded != 0;
        public Exception Exception => _exception;
        public IObservable<Shared.Lib.Empty> WhenUninstallationEnded { get; }
        #endregion Properties & Fields

        #region Constructors

        internal UninstallationProcess(UninstallProcessData uninstallProcessData, IDiskController diskController)
        {
            _uninstallProcessData = uninstallProcessData;
            _diskController = diskController;
            _whenUninstallationEndedSubject = new ReplaySubject<Shared.Lib.Empty>();
            WhenUninstallationEnded = _whenUninstallationEndedSubject.AsObservable();
        }

        #endregion Constructors

        #region Methods

        public void BeginUninstall(InstallationManager installationManager, Action<UninstallProcessData> finalizeAction)
        {
            try
            {
                Logger.Debug($"{nameof(BeginUninstall)} Method about to start");
                //Its important to set the finalize action as there could message get lost in case of error
                _uninstallProcessData.AddFinalizeAction(finalizeAction);

                QuickLeap.AssertNotNull(installationManager);
                _installationManager = installationManager;

                QuickLeap.OperateInterlockedFlag(ref _isStarted, true, false);
                Task.Run(() => UninstallLogic());
            }
            catch (Exception exception)
            {
                Logger.Error(exception,$"Exception occured during {nameof(BeginUninstall)}!");

                _exception = exception;
                _uninstallProcessData.AddException(exception);

                //Reset the Lock in case of an exception
                QuickLeap.OperateInterlockedFlag(ref _isEnded, true);

                //In any case we need to notify about the state
                NotifyUninstallationProcessEnded();
            }
        }

        private void UninstallLogic()
        {
            Logger.Debug($"{nameof(UninstallLogic)} Task start");
            try
            {
                _installationManager.PreDelete(_uninstallProcessData);
                _diskController.RemoveAllApplicationData(ApplicationGuid);
            }
            catch (Exception exception)
            {
                Logger.Error(exception,$"Exception during {nameof(UninstallLogic)} Task");
                _exception = exception;
                _uninstallProcessData.AddException(_exception);
            }
            finally
            {
                QuickLeap.OperateInterlockedFlag(ref _isEnded, true);
            }

            //In any case we need to notify about the state
            NotifyUninstallationProcessEnded();
        }

        /// <summary>
        /// Notifies all subcribers that the uninstallation process ended.
        /// </summary>
        private void NotifyUninstallationProcessEnded()
        {
            Logger.Debug($"{nameof(NotifyUninstallationProcessEnded)} Method started");
            _installationManager.PostDelete(_uninstallProcessData);
            _whenUninstallationEndedSubject.OnNext(Shared.Lib.Empty.Get);
            Logger.Debug($"Notification for {nameof(NotifyUninstallationProcessEnded)} Method done");
            _whenUninstallationEndedSubject.OnCompleted();
            Logger.Debug($"{nameof(NotifyUninstallationProcessEnded)} Method ended");
        }
        #endregion Methods
    }
}
