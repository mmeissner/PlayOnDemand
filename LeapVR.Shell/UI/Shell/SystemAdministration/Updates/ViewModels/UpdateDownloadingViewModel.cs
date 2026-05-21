#region Licence
/****************************************************************
 *  Filename: UpdateDownloadingViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-8-17
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
using LeapVR.Shared.Lib;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Updates.ViewModels
{
    public class UpdateDownloadingViewModel : UpdateProcedureBaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region Fields & Properties

        private double _downloadProgressPercent;
        public double DownloadProgressPercent
        {
            get { return _downloadProgressPercent; }
            set
            {
                _downloadProgressPercent = value;
                NotifyOfPropertyChange();
            }
        }


        private string _downloadProgress;
        public string DownloadProgress
        {
            get { return _downloadProgress; }
            set
            {
                _downloadProgress = value;
                NotifyOfPropertyChange();
            }
        }

        private string _downloadSpeed;
        public string DownloadSpeed
        {
            get { return _downloadSpeed; }
            set
            {
                _downloadSpeed = value;
                NotifyOfPropertyChange();
            }
        }


        #endregion

        #region Constructors
        public UpdateDownloadingViewModel(IUpdateController updateController) : base(updateController)
        {
            updateController.UpdateProcess.WhenDownloadProgressChanged.ObserveOnDispatcher().Subscribe(OnDownloadProgressChanged);
        }
        #endregion

        #region Methods

        public async void Cancel()
        {
            try
            {
                await UpdateProcess.CancelAsync();
                Logger.Info("Perform cancel download done.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex,"Failed to download updates");
            }
        }

        private void OnDownloadProgressChanged(Empty empty)
        {
            if (UpdateProcess.DownloadProgressPercent != null)
            {
                DownloadProgressPercent = UpdateProcess.DownloadProgressPercent.Value;
                DownloadProgress = $"{UpdateProcess.DownloadProgressPercent:#0.0}%";
            }
            DownloadSpeed = $"{QuickLeap.ToDiskSize((long)UpdateProcess.DownloadSpeed)}/s";
        }
        #endregion
    }
}
