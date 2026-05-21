#region Licence
/****************************************************************
 *  Filename: TabItemManagementAccessViewModel.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.ViewModels;
using PlatformProvider = LeapVR.Shell.UI.Core.PlatformProvider;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Management.ViewModels
{
    public sealed class TabItemManagementAccessViewModel : TabItemAppManagementScreen
        , IHandle<IUIAppUninstalledEvent>
        , IHandle<IUIAppInstalledEvent>
    , IHandle<IUIAppDisplayInfoChanged>
    {

        #region Fields & Properties
        private readonly IWindowManager _windowManager;
        private readonly IPlatformController _platformController;
        private readonly ICategoryProvider _categoryProvider;
        private readonly IReadOnlyDictionary<Guid,PlatformProvider.PlatformDisplayData> _platformDisplayDataDictionary;
        private BindableCollection<IAppCategory> _categories;
        private BindableCollection<IAppCategory> _activatedCategories;
        private IEnumerable<ApplicationViewModel> _installedApplications;
        private BindableCollection<ApplicationViewModel> _items;

        public override int DisplayOrder => 10;

        public override string DisplayName
        {
            get { return Resources.System_Apps_Management; }
            set { /* ignore */ }
        }

        public bool IsZeroItems => Items?.Count == 0;

        public BindableCollection<IAppCategory> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<IAppCategory> ActivatedCategories
        {
            get => _activatedCategories;
            set
            {
                _activatedCategories = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<ApplicationViewModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        #region Constructors
        public TabItemManagementAccessViewModel(
            IUIMessageBroker messageBroker,
            ICategoryProvider categoryProvider,
            Core.PlatformProvider platformProvider,
            IPlatformController platformController,
            IWindowManager windowManager) : base(messageBroker,"IconManagement")
        {
            QuickLeap.AssertNotNull(platformController,categoryProvider,platformProvider,messageBroker, windowManager);
            _windowManager = windowManager;
            _categoryProvider = categoryProvider;
            _platformDisplayDataDictionary = platformProvider.PlatformDisplayDataDic;
            _platformController = platformController;
            Items = new BindableCollection<ApplicationViewModel>();
            Categories = new BindableCollection<IAppCategory>();
            ActivatedCategories = new BindableCollection<IAppCategory>();
        }
        #endregion

        #region MessageHandlers
        public void Handle(IUIAppUninstalledEvent appUninstalledEvent)
        {
            PrepareApplications();
        }

        public void Handle(IUIAppInstalledEvent appInstalledEvent)
        {
            PrepareApplications();
        }

        public void Handle(IUIAppDisplayInfoChanged message)
        {
            if(message.UpdateInfo.CategoryChanged)
            {
                RefreshCategories();
            }
        }
        #endregion

        #region Methods
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            PrepareApplications();
        }

        public void OnActivatedCategoriesChanged(SelectionChangedEventArgs e)
        {
            foreach (IAppCategory addedItem in e.AddedItems)
            {
                if (ActivatedCategories.Contains(addedItem))
                {
                    continue;
                }
                ActivatedCategories.Add(addedItem);
            }
            foreach (IAppCategory removedItem in e.RemovedItems)
            {
                if (!ActivatedCategories.Contains(removedItem))
                {
                    continue;
                }
                ActivatedCategories.Remove(removedItem);
            }
            RefreshApplications();
        }

        private void PrepareApplications()
        {
            _installedApplications = PullAvailableApplications();
            RefreshCategories();
            RefreshApplications();
        }

        private void RefreshCategories()
        {
            var categories =
                from app in _installedApplications
                group app by app.Category into c
                select c.Key;
            ActivatedCategories.Clear();
            Categories.Clear();
            foreach(IAppCategory category in categories)
            {
                Categories.Add(category);
            }
        }

        private void RefreshApplications()
        {
            IEnumerable<ApplicationViewModel> result;
            if (ActivatedCategories == null || ActivatedCategories.Count == 0)
            {
                result = _installedApplications;
            }
            else
            {
                result =
                    from app in _installedApplications
                    where ActivatedCategories.Contains(app.Category)
                    select app;
            }

            Items.Clear();
            Items.AddRange(result);

            NotifyOfPropertyChange(() => IsZeroItems);
        }
        private IEnumerable<ApplicationViewModel> PullAvailableApplications()
        {
            return from appPlatformInfo in _platformController.GetInstalledApplications()
                   select new ApplicationViewModel(_categoryProvider, _windowManager, appPlatformInfo, _platformDisplayDataDictionary[appPlatformInfo.PlatformGuid].Image);
        }

        protected override void HandleLanguageChange(IUILanguageChangedEvent message){}
        #endregion
    }
}
