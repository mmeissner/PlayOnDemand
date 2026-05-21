#region Licence
/****************************************************************
 *  Filename: UpdateProcedureBaseViewModel.cs
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

using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Updates.ViewModels
{
    /// <summary>
    /// Base class for update procedure view models.
    /// </summary>
    public abstract class UpdateProcedureBaseViewModel : Screen
    {

        #region Fields & Properties
        private IUpdateProcess _updateProcess;
        /// <summary>
        /// Get or set the entity of update process
        /// </summary>
        protected IUpdateProcess UpdateProcess => _updateProcess;

        #endregion

        #region Constructors
        protected UpdateProcedureBaseViewModel(IUpdateController updateController)
        {
            _updateProcess = updateController.UpdateProcess;
        }

        #endregion

        #region Methods

        #endregion
    }
}
