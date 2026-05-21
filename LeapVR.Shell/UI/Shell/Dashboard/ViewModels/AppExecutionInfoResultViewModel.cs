#region Licence
/****************************************************************
 *  Filename: AppExecutionInfoResultViewModel.cs
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
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{
    public class AppExecutionInfoResultViewModel : Screen
    {

        #region Fields & Properties

        public new string DisplayName { get; set; }
        public bool IsVirtualRealityRequired { get; set; }
        public bool IsVrRequirementFulfilled { get; set; }
        public bool IsScreenModeSupported { get; set; }
        public IExecuteable Executeable { get; }
        #endregion

        #region Constructors

        public AppExecutionInfoResultViewModel(IExecuteable executeable)
        {
            Executeable = executeable;
            DisplayName = executeable.DisplayName;
            IsVirtualRealityRequired = executeable.IsVirtualRealityRequired;
            IsVrRequirementFulfilled = executeable.IsVrRequirementFullfiled;
            IsScreenModeSupported = executeable.IsScreenModeSupported;
        }
        #endregion
    }
}
