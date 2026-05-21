#region Licence
/****************************************************************
 *  Filename: AppExecutionSelectViewModel.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{
    public class AppExecutionSelectViewModel : InputControllerConductorOneActive<AppExecutionInfoResultViewModel>
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IUIMessageBroker _messageBroker;
        private WpfDirectionalNavigation<AppExecutionInfoResultViewModel> _directionalNavigation = null;
        private AppExecutionInfoResultViewModel _selectedItem;

        public List<AppExecutionInfoResultViewModel> ExecutionCandidates { get; internal set; } = new List<AppExecutionInfoResultViewModel>();
        public AppExecutionInfoResultViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange();
            }
        }
        public bool EnableGamepadIndicator { get; } = true;
        public bool IsCancelOnly { get; private set; }
        #endregion

        #region Constructors
        public AppExecutionSelectViewModel(
            IViewInputHandler viewInputHandler,
            IUIMessageBroker messageBroker):base(viewInputHandler, InputExclusivity.RegisterdControllerInputs)
        {
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            AddControllerInputAction(ControllerInput.DDown, () => { Navigate(NavigationDirection.Down); });
            AddControllerInputAction(ControllerInput.Down, () => { Navigate(NavigationDirection.Down); });
            AddControllerInputAction(ControllerInput.DUp, () => { Navigate(NavigationDirection.Up); });
            AddControllerInputAction(ControllerInput.Up, () => { Navigate(NavigationDirection.Up); });
            AddControllerInputAction(ControllerInput.Left, () => { Navigate(NavigationDirection.Left); });
            AddControllerInputAction(ControllerInput.DLeft, () => { Navigate(NavigationDirection.Left); });
            AddControllerInputAction(ControllerInput.Right, () => { Navigate(NavigationDirection.Right); });
            AddControllerInputAction(ControllerInput.DRight, () => { Navigate(NavigationDirection.Right); });
            AddControllerInputAction(ControllerInput.Accept, Confirm);
            AddControllerInputAction(ControllerInput.Start, Confirm);
            AddControllerInputAction(ControllerInput.Cancel, Cancel);
            AddControllerInputAction(ControllerInput.Back, Cancel);
        }
        #endregion

        #region Methods
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if (_directionalNavigation == null && view is FrameworkElement parent)
            {
                var itemsSelector = UIHelper.GetChildElementByNameFromVisualTree<Selector>(parent, nameof(ExecutionCandidates));
                _directionalNavigation = new WpfDirectionalNavigation<AppExecutionInfoResultViewModel>(itemsSelector);
            }
            SelectFirstItemIfNothingSelected();
        }
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            CleanupOnClose();
        }

        public void Confirm()
        {
            TryClose(true);
            CleanupOnClose();
        }

        public void Cancel()
        {
            TryClose(false);
            CleanupOnClose();
        }

        private void SelectFirstItemIfNothingSelected()
        {
            if (!IsActive || ExecutionCandidates == null || ExecutionCandidates.Count <= 0)
            {
                Logger.Debug($"Ignore to selected first item. Is view active: '{IsActive}', Items: {Items?.Count}.");
                IsCancelOnly = true;
                return;
            }
            if (SelectedItem == null)
            {
                SelectedItem = ExecutionCandidates.FirstOrDefault();
            }
        }

        private void Navigate(NavigationDirection direction)
        {
            _directionalNavigation?.Navigate(direction, SelectedItem, x => { SelectedItem = x; });
        }

        private void CleanupOnClose()
        {
            _messageBroker.Unsubscribe(this);
        }
        #endregion
    }
}
