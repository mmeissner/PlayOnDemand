#region Licence
/****************************************************************
 *  Filename: BlockShellBaseViewModel.cs
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
using System.Windows;
using System.Windows.Input;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Shell.Blocker.Abstract
{
    /// <summary>
    /// Basic implementation for <see cref="IBlockShellViewModel"/>. 
    /// </summary>
    public abstract class BlockShellBaseViewModel : InputControllerScreen,IDisposable, IBlockShellViewModel
    {
        private bool _isClosed;
        public bool IsClosed
        {
            get => _isClosed;
            set
            {
                _isClosed = value;
                NotifyOfPropertyChange();
            }
        }

        public BlockShellBaseViewModel(IViewInputHandler inputHandler) : base(inputHandler, InputExclusivity.AllControllerInputs){}

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if (view is UIElement focusableElement)
            {
                Keyboard.Focus(focusableElement);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Close the modal view.
        /// </summary>
        public virtual void Close()
        {
            IsClosed = true;
        }
        public new void Dispose()
        {
            ClearAllControllerInputActions();
            base.Dispose();
        }
    }
}
