#region Licence
/****************************************************************
 *  Filename: IBlockShellViewModel.cs
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
using System;
using Caliburn.Micro;
using LeapVR.Shared.Lib;

namespace LeapVR.Shell.UI.Interfaces
{
    /// <summary>
    /// Representing a view model for modal views that will block the shell
    /// </summary>
    public interface IBlockShellViewModel : IScreen, IDisposable
    {
        /// <summary>
        /// Get the indicator if the view is visible or not.
        /// </summary>
        bool IsClosed { get; }
        /// <summary>
        /// Close the modal view.
        /// </summary>
        void Close();
    }
}
