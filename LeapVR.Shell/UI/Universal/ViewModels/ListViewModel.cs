#region Licence
/****************************************************************
 *  Filename: ListViewModel.cs
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
using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;
using NLog;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    public class ListViewModel<T> : Screen
    {
        #region Fields & Properties
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private T _selectedItem;
        private BindableCollection<T> _items = new BindableCollection<T>();
        private bool _showLoading = true;
        public bool ShowNoItems
        {
            get
            {
               if(ShowLoading) return false;
                return !Items.Any();
            }
        }
        public bool ShowLoading
        {
            get => _showLoading;
            set
            {
                if(value == _showLoading) return;
                _showLoading = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ShowNoItems));
            }
        }
        public BindableCollection<T> Items
        {
            get { return _items; }
            set
            {
                if(Equals(value, _items)) return;
                _items = value;
                NotifyOfPropertyChange();
            }
        }
        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                NotifyOfPropertyChange(nameof(SelectedItem));
            }
        }

        public ListViewModel()
        {
            _items.CollectionChanged += _items_CollectionChanged;
        }

        private void _items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    NotifyOfPropertyChange(nameof(ShowNoItems));
                    break;
            }
        }
        #endregion
    }
}