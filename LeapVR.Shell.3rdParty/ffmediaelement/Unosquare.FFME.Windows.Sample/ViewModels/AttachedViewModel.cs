#region Licence
/****************************************************************
 *  Filename: AttachedViewModel.cs
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
namespace Unosquare.FFME.Windows.Sample.ViewModels
{
    using Foundation;

    /// <summary>
    /// A base class for Root VM-attached view models
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public abstract class AttachedViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachedViewModel"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        protected AttachedViewModel(RootViewModel root)
        {
            Root = root;
        }

        /// <summary>
        /// Gets the root VM this object belongs to.
        /// </summary>
        public RootViewModel Root { get; }

        /// <summary>
        /// Called by the root ViewModel when the application is loaded and fully available
        /// </summary>
        internal virtual void OnApplicationLoaded()
        {
            // placeholder
        }
    }
}
