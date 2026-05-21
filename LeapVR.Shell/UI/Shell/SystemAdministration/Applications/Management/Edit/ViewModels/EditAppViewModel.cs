#region Licence
/****************************************************************
 *  Filename: EditAppViewModel.cs
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
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Management.Edit.ViewModels
{
    public class EditAppViewModel : Screen
    {
        private readonly IAppDisplayUpdate _appDisplayUpdate;
        private IAppCategory _selectedCategory;
        public EditAppViewModel(IEnumerable<IAppCategory> categories, IAppDisplayUpdate appDisplayUpdate)
        {
            _appDisplayUpdate = appDisplayUpdate;
            Categories = new BindableCollection<IAppCategory>(categories);
            _selectedCategory = appDisplayUpdate.Category;
        }

        public IAppDisplayUpdate AppUpdate => _appDisplayUpdate;
        public IObservableCollection<IAppCategory> Categories { get; }
        public IAppCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if(Equals(value, _selectedCategory)) return;
                _selectedCategory = value;
                _appDisplayUpdate.Category = value;
                NotifyOfPropertyChange();
            }
        }

        public void Apply()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
