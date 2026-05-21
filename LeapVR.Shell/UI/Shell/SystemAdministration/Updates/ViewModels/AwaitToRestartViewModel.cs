#region Licence
/****************************************************************
 *  Filename: AwaitToRestartViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-8-30
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using LeapVR.Shell.Controllers.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Updates.ViewModels
{
    public class AwaitToRestartViewModel : UpdateProcedureBaseViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #region Fields & Properties

        private readonly IStationController _stationController;

        #endregion

        #region Constructors
        public AwaitToRestartViewModel(IStationController stationController, IUpdateController updateController) : base(updateController)
        {
            _stationController = stationController;
        }

        #endregion

        #region Methods

        public void Restart()
        {
            _stationController.RequestRestart();
        }
        #endregion
    }
}
