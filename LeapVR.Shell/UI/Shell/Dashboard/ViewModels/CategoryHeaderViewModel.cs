#region Licence
/****************************************************************
 *  Filename: CategoryHeaderViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-11
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
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{
    public class CategoryHeaderViewModel : Screen, IHaveDisplayOrder
    {
        private int _amountOfApplications;
        private int _displayOrder;
        private IAppCategory _appCategory;

        #region Fields & Properties
        public int AmountOfApplications
        {
            get { return _amountOfApplications; }
            set
            {
                if(value == _amountOfApplications) return;
                _amountOfApplications = value;
                NotifyOfPropertyChange();
            }
        }
        public int DisplayOrder
        {
            get { return _displayOrder; }
            set
            {
                if(value == _displayOrder) return;
                _displayOrder = value;
                NotifyOfPropertyChange();
            }
        }
        public IAppCategory AppCategory
        {
            get => _appCategory;
            private set
            {
                if(Equals(value, _appCategory)) return;
                _appCategory = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors
        public CategoryHeaderViewModel(
            IAppCategory appCategory,
            int displayOrder,
            int applicationsCount)
        {
            DisplayOrder = displayOrder;
            AmountOfApplications = applicationsCount;
            AppCategory = appCategory;

        }
        #endregion
    }
}