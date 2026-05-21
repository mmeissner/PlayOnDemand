#region Licence
/****************************************************************
 *  Filename: CheckUpdatesViewModel.cs
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
using LogManager = NLog.LogManager;


namespace LeapVR.Shell.UI.Shell.SystemAdministration.Updates.ViewModels
{
    public class CheckUpdatesViewModel : UpdateProcedureBaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region Fields & Properties

        private string _information;
        /// <summary>
        /// Get or set the information to display in the textual area.
        /// </summary>
        public string Information
        {
            get => _information;
            set
            {
                _information = value;
                NotifyOfPropertyChange();
            }
        }

        private string _checkUpdateStr;
        /// <summary>
        /// Get or set the text for check update button.
        /// </summary>
        public string CheckUpdateStr
        {
            get => _checkUpdateStr;
            set
            {
                _checkUpdateStr = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors
        public CheckUpdatesViewModel(IUpdateController updateController) : base(updateController)
        {
            Information = string.Empty;
            CheckUpdateStr = Language.Resources.System_Updates_CheckUpdates;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Check updates from remote server and compare it with local version.
        /// </summary>
        public async void CheckUpdates()
        {
            try
            {
                await UpdateProcess.CheckNewestVersionAsync();
                Logger.Info("Perform check updates done.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex,"Failed to check updates, exception occured");
            }
        }

        #endregion
    }
}
