#region Licence
/****************************************************************
 *  Filename: UpdateAvailableViewModel.cs
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
using LeapVR.Shell.Controllers.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Updates.ViewModels
{
    public class UpdateAvailableViewModel : UpdateProcedureBaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region Fields & Properties

        #endregion

        #region Constructors
        public UpdateAvailableViewModel(IUpdateController updateController) : base(updateController)
        {

        }

        #endregion

        #region Methods
        /// <summary>
        /// Perform download action to download the newest version of shell
        /// </summary>
        public async void Download()
        {
            try
            {
                await UpdateProcess.DownloadNewestVersionAsync();
                Logger.Info("Perform download request done.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to download update.");
            }
        }
        #endregion


    }
}
