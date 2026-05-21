#region Licence
/****************************************************************
 *  Filename: ReadyToUpdateViewModel.cs
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
    public class ReadyToUpdateViewModel: UpdateProcedureBaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields & Properties

        #endregion

        #region Constructors
        public ReadyToUpdateViewModel(IUpdateController updateController) : base(updateController)
        {

        }

        #endregion

        #region Methods
        /// <summary>
        /// Perform update when package is already downloaded.
        /// </summary>
        public async void Update()
        {
            try
            {
                await UpdateProcess.PerformUpdateAsync();
                Logger.Info("Perform update request done.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to download updates.");
            }
        }
        #endregion
    }
}
