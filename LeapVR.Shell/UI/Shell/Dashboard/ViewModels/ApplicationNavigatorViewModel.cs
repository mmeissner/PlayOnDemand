#region Licence
/****************************************************************
 *  Filename: ApplicationNavigatorViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-12
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using NLog;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{
    public class ApplicationNavigatorViewModel : InputControllerConductorOneActive<ApplicationExecutableViewModel>,
                                                 IApplicationNavigatorViewModel<ApplicationExecutableViewModel>
    {
        #region Fields & Properties
        private bool _isDisposed = false;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private WpfDirectionalNavigation<ApplicationExecutableViewModel> _directionalNavigation;
        public IAppCategory Category { get; }
        private ApplicationExecutableViewModel _selectedItem;
        public ApplicationExecutableViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                ActivateItem(_selectedItem);
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors
        public ApplicationNavigatorViewModel(
                IViewInputHandler inputHandler, IAppCategory category,
                IEnumerable<ApplicationExecutableViewModel> applications) : base(inputHandler)
        {
            Category = category;
            Items.CollectionChanged += Items_CollectionChanged;
            Items.AddRange(applications);
            AddControllerInputAction(ControllerInput.DDown, () => { Navigate(NavigationDirection.Down); });
            AddControllerInputAction(ControllerInput.Down, () => { Navigate(NavigationDirection.Down); });
            AddControllerInputAction(ControllerInput.DUp, () => { Navigate(NavigationDirection.Up); });
            AddControllerInputAction(ControllerInput.Up, () => { Navigate(NavigationDirection.Up); });
            AddControllerInputAction(ControllerInput.Left, () => { Navigate(NavigationDirection.Left); });
            AddControllerInputAction(ControllerInput.DLeft, () => { Navigate(NavigationDirection.Left); });
            AddControllerInputAction(ControllerInput.Right, () => { Navigate(NavigationDirection.Right); });
            AddControllerInputAction(ControllerInput.DRight, () => { Navigate(NavigationDirection.Right); });
            AddControllerInputAction(ControllerInput.Accept, ExecuteSelectedItem);
        }


        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(object newItem in e.NewItems)
                    {
                        SubscribeItem(newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(object oldItem in e.OldItems)
                    {
                        UnsubscribeItem(oldItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:

                    foreach(object newItem in e.NewItems)
                    {
                        SubscribeItem(newItem);
                    }
                    foreach(object oldItem in e.OldItems)
                    {
                        UnsubscribeItem(oldItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    if(sender is IEnumerable<ApplicationExecutableViewModel> viewModels)
                    {
                        foreach(var model in viewModels)
                        {
                            SubscribeItem(model);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion

        #region Methods
        protected override void OnActivate()
        {
            base.OnActivate();
            // TODO [FH] load DashboardViewModel on demand then nothing will grab the focus before this view is ready.
            SelectFirstItemIfNothingSelected();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if(_directionalNavigation == null && view is FrameworkElement parent)
            {
                var itemsSelector = UIHelper.GetChildElementByNameFromVisualTree<Selector>(parent, nameof(Items));
                Logger.Debug($"Got {itemsSelector} on {nameof(OnViewLoaded)}");
                _directionalNavigation = new WpfDirectionalNavigation<ApplicationExecutableViewModel>(itemsSelector);
            }

            SelectFirstItemIfNothingSelected();
        }

        private bool ToApplicationExecutableViewModel(
                object obj, out ApplicationExecutableViewModel applicationExecutableViewModel)
        {
            if(obj is ApplicationExecutableViewModel appViewModel)
            {
                applicationExecutableViewModel = appViewModel;
                return true;
            }

            applicationExecutableViewModel = null;
            return false;
        }

        private void SubscribeItem(object item)
        {
            if(ToApplicationExecutableViewModel(item, out var viewModel))
            {
                SubscribeItem(viewModel);
            }
        }

        private void SubscribeItem(ApplicationExecutableViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void UnsubscribeItem(object item)
        {
            if(ToApplicationExecutableViewModel(item, out var viewModel))
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(ApplicationExecutableViewModel.Category)))
            {
                if(sender is ApplicationExecutableViewModel applicationExecutableViewModel)
                {
                    Items.Remove(applicationExecutableViewModel);
                }
            }
        }

        public void ExecuteSelectedItem()
        {
            Logger.Info("Try to perform start application from UI entry.");
            SelectedItem?.Execute();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing && !_isDisposed)
            {
                foreach(ApplicationExecutableViewModel item in Items)
                {
                    UnsubscribeItem(item);
                }
                Items.CollectionChanged -= Items_CollectionChanged;
                _isDisposed = true;
            }
            base.Dispose(disposing);
        }

        private void Navigate(NavigationDirection direction)
        {
            _directionalNavigation?.Navigate(direction, SelectedItem, x => { SelectedItem = x; });
        }

        public void SelectItem(object dataContext)
        {
            if(dataContext is ApplicationExecutableViewModel viewModel)
            {
                SelectedItem = viewModel;
            }
        }

        private void SelectFirstItemIfNothingSelected()
        {
            if(!IsActive || Items == null || Items.Count <= 0)
            {
                Logger.Debug($"Ignore to selected first item. Is view active: '{IsActive}', Items: {Items?.Count}.");
                return;
            }

            if(SelectedItem == null)
            {
                SelectedItem = Items.FirstOrDefault();
            }
        }
        #endregion
    }
}