#region Licence
/****************************************************************
 *  Filename: MessageDisplayViewModel.cs
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

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    /// <summary>
    /// Representing a view model that drives a view to display messages with a <see cref="Title"/> and a <see cref="Description"/>.
    /// </summary>
    public class MessageDisplayViewModel : Screen
    {

        #region Fields & Properties
        private string _title;
        /// <summary>
        /// Get or set title of loading.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        private string _description;
        /// <summary>
        /// Get or set description of loading
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors

        #endregion

        #region Methods

        #endregion
    }
}
