#region Licence
/****************************************************************
 *  Filename: DashboardViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-5
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.UI.Base;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Universal.Dialog;
using NLog;
using LogManager = NLog.LogManager;

namespace LeapVR.Shell.UI.Shell.Dashboard.ViewModels
{

    public class DashboardViewModel : InputControllerConductorOneActive<IApplicationNavigatorViewModel<ApplicationExecutableViewModel>>, IDashboardViewModel, IDisposable
        , IHandle<IUISessionStartedEvent>
        , IHandle<IUISessionStopedEvent>
        , IHandle<IUIAppEnabledStateChangedEvent>
        , IHandle<IUIAppDisplayInfoChanged>
        , IHandle<IUIPlatformAccountChanged>
    {
        #region Private Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IWindowManager _windowManager;
        private readonly IPlatformController _platformController;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly IStatusBarViewModel _statusBarViewModel;
        private readonly IUIMessageBroker _messageBroker;
        private readonly IViewInputHandler _inputHandler;
        private readonly UiConfig _uiConfig;
        private readonly object _logoutLock = new object();
        private readonly Dictionary<Guid,ApplicationExecutableViewModel> _appViewModels = new Dictionary<Guid, ApplicationExecutableViewModel>();
        private BindableCollection<CategoryHeaderViewModel> _categories;
        private WpfDirectionalNavigation<CategoryHeaderViewModel> _directionalNavigation;
        private CategoryHeaderViewModel _activatedCategory;
        private IScreen _sessionViewModel;
        private IUISession _session;
        private bool _showApplications;
        private bool _showNoApplicationsMessage;
        private bool _showButtomBar;
        private bool _canLogout;
        private bool _resortCategories;
        #endregion

        #region Public Properties
        public bool ShowButtomBar
        {
            get { return _showButtomBar; }
            set
            {
                if(value == _showButtomBar) return;
                _showButtomBar = value;
                NotifyOfPropertyChange(() => ShowButtomBar);
            }
        }
        public IScreen SessionViewModel
        {
            get => _sessionViewModel;
            set
            {
                _sessionViewModel = value;
                NotifyOfPropertyChange(() => SessionViewModel);
            }
        }
        public bool ShowApplications    
        {
            get { return _showApplications; }
            set
            {
                if (value == _showApplications) return;
                _showApplications = value;
                NotifyOfPropertyChange();
            }
        }
        public bool ShowNoApplicationsMessage   
        {
            get { return _showNoApplicationsMessage; }
            set
            {
                if (value == _showNoApplicationsMessage) return;
                _showNoApplicationsMessage = value;
                NotifyOfPropertyChange();
            }
        }
        public IStatusBarViewModel StatusBarViewModel { get; set; }
        public BindableCollection<CategoryHeaderViewModel> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                NotifyOfPropertyChange();
            }
        }
        public CategoryHeaderViewModel ActivatedCategory
        {
            get => _activatedCategory;
            set
            {
                _activatedCategory = value;
                NotifyOfPropertyChange();
                ActivateItem(Items.FirstOrDefault(item => item.Category.Equals(ActivatedCategory?.AppCategory)));
            }
        }
        #endregion

        #region Constructors
        public DashboardViewModel(
            IUIMessageBroker messageBroker,
            IWindowManager windowManager,
            IPlatformController platformController,
            IViewInputHandler inputHandler,
            IStatusBarViewModel statusBarViewModel,
            ViewModelFactory viewModelFactory,
            UiConfig uiConfig,
            IGlobalConfiguration globalConfiguration,
            ICategoryProvider categoryProvider
            ):base(inputHandler)
        {
            QuickLeap.AssertNotNull(
                windowManager,
                platformController,
                viewModelFactory, 
                messageBroker,
                inputHandler,
                uiConfig,
                globalConfiguration,
                    categoryProvider
                );
            _windowManager = windowManager;
            _platformController = platformController;
            _messageBroker = messageBroker;
            _viewModelFactory = viewModelFactory;
            _statusBarViewModel = statusBarViewModel;
            _inputHandler = inputHandler;
            _uiConfig = uiConfig;

            AddControllerInputAction(ControllerInput.NextOne, ()=> NavigateItem(1));
            AddControllerInputAction(ControllerInput.PreviousOne, () => NavigateItem(-1));
            AddControllerInputAction(ControllerInput.Cancel, AttemptToLogout);
            InitializeAvailableApplications();
            _messageBroker.Subscribe(this);
        }
        #endregion

        #region Public Methods
        public bool CanAttemptToLogout
        {
            get { return _canLogout; }
            set
            {
                if(value == _canLogout) return;
                _canLogout = value;
                Logger.Trace($"Can Logout Button = {_canLogout}");
                NotifyOfPropertyChange();
            }
        }
        public void Logout()
        {
            _session?.RequestStopSession(SessionStopReason.StationLogout);
        }
        public void AttemptToLogout()
        {
            try
            {
                if (!CanAttemptToLogout) return;
                lock (_logoutLock)
                {
                    if (!CanAttemptToLogout) return;
                    CanAttemptToLogout = false;
                }
                Logger.Info("Logout Attempt is to be processed.");
                var userWantLogout = _windowManager.ShowDialog(
                    _viewModelFactory.Build(DialogType.AttemptToLogout), null, ShellClientHelper.GetUniversalDialogSettings());
                if (userWantLogout == true)
                {
                    Logout();
                    Logger.Info("Logout confirmed by user.");
                }
                else
                {
                    CanAttemptToLogout = true;
                    Logger.Info("Logout canceled by user.");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }
        #endregion

        #region Handlers
        public async void Handle(IUISessionStartedEvent message)
        {
            try
            {
                Logger.Info($"Handle Session Started for Session of Type={message.GetType()}");
                ShowButtomBar = true;
                PrepareStatusBar();
                PrepareSessionDetail(message.Session);
                ShowContent();
                ShowApplications = true;
                Logger.Info("Handle Session finished, waiting for releasing Logout Button");
                _session = message.Session;
                await Task.Delay(1500).ConfigureAwait(true);
                CanAttemptToLogout = true;
            }
            catch(Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }
        public void Handle(IUISessionStopedEvent message)
        {
            try
            {
                Logger.Info("Handle session stopped.");
                ShowButtomBar = false;
                ReleaseStatusBar();
                ReleaseSessionDetail();
                HideContent();
                DeactivateItem(ActiveItem, false);
                CanAttemptToLogout = false;
                _session = null;
                Logger.Info("Finished Handle session stopped.");
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
        public void Handle(IUIAppEnabledStateChangedEvent message)
        {
            try
            {
                //App was Added
                if (message.IsEnabled)
                {
                    if(_platformController.TryGetAvailableApplication(message.ApplicationGuid, out var platformApp))
                    {
                        var newAppViewModel = _viewModelFactory.Build(platformApp);
                        _appViewModels.Add(message.ApplicationGuid, newAppViewModel);
                        CreateOrUpdateHeaderViewModel(newAppViewModel);
                    }
                }
                //App was Removed
                else
                {
                    if (_appViewModels.TryGetValue(message.ApplicationGuid, out var appViewModel))
                    {
                        RemoveOrUpdateHeaderViewModel(appViewModel);
                    }
                    else
                    {
                        Logger.Warn($"Could not Remove App with Guid={message.ApplicationGuid} as it is not registered in the Dictionary");
                    }
                    _appViewModels.Remove(message.ApplicationGuid);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public void Handle(IUIPlatformAccountChanged message)
        {
            ApplicationExecutableViewModel appViewModel;
            switch(message.Type)
            {
                case AccountEventType.AddApps:
                    //Check if this app might have become available if not already in list
                    if(message.ApplicationId.HasValue)
                    {
                        //Its not yet in the list, so we will add it as we got notified that now
                        if(! _appViewModels.TryGetValue(message.ApplicationId.Value, out appViewModel))
                        {
                            if(_platformController.TryGetAvailableApplication(message.ApplicationId.Value, out var platformAppInfo))
                            {
                                var newAppViewModel = _viewModelFactory.Build(platformAppInfo);
                                _appViewModels.Add(newAppViewModel.ApplicationGuid, newAppViewModel);
                                CreateOrUpdateHeaderViewModel(newAppViewModel);
                            }
                        }
                    }
                    break;
                case AccountEventType.RemoveApps:
                    if(message.ApplicationId.HasValue && _appViewModels.TryGetValue(message.ApplicationId.Value, out appViewModel))
                    {
                        //Check if there is an account left, or in general if app is still considered as available
                        if(!_platformController.IsAvailible(appViewModel.ApplicationGuid))
                        {
                            RemoveOrUpdateHeaderViewModel(appViewModel);
                            _appViewModels.Remove(appViewModel.ApplicationGuid);
                        }
                    }
                    break;
            }
        }

        public void Handle(IUIAppDisplayInfoChanged message)
        {
            if(message.UpdateInfo.CategoryChanged)
            {
                //Check if we track this app
                if(_appViewModels.TryGetValue(message.DisplayInfo.ApplicationGuid, out var appViewModel))
                {
                    UpdateHeaderViewModels(appViewModel);
                }
            }
        }
        #endregion

        #region Private Methods
        private void NavigateItem(int steps)
        {
            if(steps == 0 || Categories.Count == 0)return;
            var currentCategoryIndex = Categories.IndexOf(ActivatedCategory);
            var indexToJumpto = currentCategoryIndex;
            if (steps > 0)
            {
                for (int i = 0; i < steps; i++)
                {
                    indexToJumpto++;
                    if (indexToJumpto > Categories.Count - 1) indexToJumpto = 0;
                }
            }
            else
            {
                for (int i = 0; i < Math.Abs(steps); i++)
                {
                    indexToJumpto--;
                    if (indexToJumpto < 0) indexToJumpto = Categories.Count-1;
                }
            }
            Logger.Debug($"Navigating to item with index '{indexToJumpto}'.");
            ActivatedCategory = Categories[indexToJumpto];
            _directionalNavigation.BringIntoView(ActivatedCategory);
        }
        private void PrepareStatusBar()
        {
            //Gets not Activated by the Framework as we are a ConductorOneActive<IApplicationNavigatorViewModel
            StatusBarViewModel = _statusBarViewModel;
            NotifyOfPropertyChange(() => StatusBarViewModel);
            ScreenExtensions.TryActivate(StatusBarViewModel);
            Logger.Trace($"Preparing {nameof(StatusBarViewModel)}.");
        }
        private void ReleaseStatusBar()
        {
            //Gets not Deactivated by the Framework as we are a ConductorOneActive<IApplicationNavigatorViewModel
            ScreenExtensions.TryDeactivate(StatusBarViewModel, false);
            StatusBarViewModel = null;
            NotifyOfPropertyChange(() => StatusBarViewModel);
            Logger.Trace($"Releasing {nameof(StatusBarViewModel)}.");
        }
        private void PrepareSessionDetail(IUISession session)
        {
            //Gets not Activated by the Framework as we are a ConductorOneActive<IApplicationNavigatorViewModel
            SessionViewModel = new SessionViewModel(_messageBroker, session);
            ScreenExtensions.TryActivate(SessionViewModel);
            Logger.Trace($"Preparing {nameof(SessionViewModel)}.");
        }
        private void ReleaseSessionDetail()
        {
            //Gets not Deactivated by the Framework as we are a ConductorOneActive<IApplicationNavigatorViewModel
            ScreenExtensions.TryDeactivate(SessionViewModel,true);
            SessionViewModel = null;
            Logger.Trace($"Releasing {nameof(SessionViewModel)}.");
        }
        private void ShowContent()
        {
            ShowApplications = true;
            ShowNoApplicationsMessage = Categories != null && Categories.Any();
            //Sort Categories
            if(_resortCategories && Categories != null)
            {
                var categories = Categories.ToList();
                _categories.Clear();
                foreach(var headerViewModel in categories.OrderByDescending(x=> x.DisplayOrder))
                {
                    _categories.Add(headerViewModel);
                }
                NotifyOfPropertyChange(() => Categories);
            }
            ActivatedCategory = Categories?.FirstOrDefault();
            Logger.Info($"Showing Contents in {Categories?.Count} Categories.");
        }
        private void HideContent()
        {
            Logger.Debug("Hidding Contents!");
            ShowApplications = false;
            ShowNoApplicationsMessage = true;
        }
        private void InitializeAvailableApplications()
        {
            try
            {
                _resortCategories = true;
                ApplicationLoadingResult result = new ApplicationLoadingResult();
                Logger.Debug($"Try to load applications from {nameof(IPlatformController)}.");

                foreach (IAppPlatformInfo availableApplication in _platformController.GetAvailableApplications())
                {
                    _appViewModels.Add(availableApplication.ApplicationGuid, _viewModelFactory.Build(availableApplication));
                }
                var grouped = _appViewModels.Values.GroupBy(x => x.Category);
                result.CategoryApplicationsMapping = grouped;
                var headerViewModels = new List<CategoryHeaderViewModel>();
                foreach (IGrouping<IAppCategory, ApplicationExecutableViewModel> viewModels in grouped)
                {
                    var appsAmount = viewModels.Count();
                    //Categories with most apps gets displayed first
                    headerViewModels.Add(CreateCategoryHeaderViewModel(viewModels.Key, appsAmount, appsAmount));
                }
                result.CategoryMetadata = headerViewModels;
                Categories = new BindableCollection<CategoryHeaderViewModel>(result.CategoryMetadata);
                foreach (var group in result.CategoryApplicationsMapping)
                {
                    Items.Add(new ApplicationNavigatorViewModel(_inputHandler, group.Key, group));
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        private void RemoveOrUpdateHeaderViewModel(ApplicationExecutableViewModel executableView)
        {
            try
            {
                var applicationNavigatorViewModel = Items.FirstOrDefault(x => x.Category.Equals(executableView.Category));
                bool removeCategory = false;
                if (applicationNavigatorViewModel == null)
                {
                    Logger.Warn($"Could not find ApplicationNavigatorViewModel for Category={executableView.Category}");
                    removeCategory = true;
                }
                else
                {
                    if(!applicationNavigatorViewModel.Items.Remove(executableView))
                    {
                        Logger.Warn($"Could not remove executableView with Id={executableView.ApplicationGuid},Category={executableView.Category} from ApplicationNavigatorViewModel with Category={applicationNavigatorViewModel.Category}");
                    };
                    if (!applicationNavigatorViewModel.Items.Any())
                    {
                        removeCategory = true;
                    }
                }
                var category = Categories.FirstOrDefault(x => x.AppCategory.Equals(executableView.Category));
                if (category == null)
                {
                    Logger.Warn($"Could not find Category with Name={executableView.Category}");
                    return;
                }
                if(removeCategory)
                {
                   
                    Categories.Remove(category);
                    if(applicationNavigatorViewModel != null) Items.Remove(applicationNavigatorViewModel);
                }
                else
                {
                    category.AmountOfApplications = category.AmountOfApplications - 1;
                    category.DisplayOrder = category.AmountOfApplications;
                }
                //Set Flag to resort Categories on Login
                _resortCategories = true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
           
        }
        private void CreateOrUpdateHeaderViewModel(ApplicationExecutableViewModel viewModel)
        {
            try
            {
                var categoryAppNavigator = Items?.FirstOrDefault(x => x.Category.Equals(viewModel.Category));

                //New Category
                if (categoryAppNavigator == null)
                {
                    var newHeaderViewModel = CreateCategoryHeaderViewModel(viewModel.Category, 1, 1);
                    Categories.Add(newHeaderViewModel);
                    Items.Add(new ApplicationNavigatorViewModel(_inputHandler, viewModel.Category, new[] { viewModel }));
                }
                //Update Existing Category
                else
                {
                    categoryAppNavigator.Items.Add(viewModel);
                    var updateCategory = Categories?.FirstOrDefault(x => x.AppCategory.Equals(viewModel.Category));
                    if (updateCategory != null)
                    {
                        updateCategory.AmountOfApplications = updateCategory.AmountOfApplications + 1;
                        updateCategory.DisplayOrder = updateCategory.AmountOfApplications;
                    }
                }
                //Set Flag to resort Categories on Login
                _resortCategories = true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
        private void UpdateHeaderViewModels(ApplicationExecutableViewModel viewModel)
        {
            //Removal happens on the ApplicationNavigatorViewModel, we just need to add here
            CreateOrUpdateHeaderViewModel(viewModel);

            //Check Categories as the one removed the item might be empty and the count does not fit
            var categoriesToRemove = new List<CategoryHeaderViewModel>();
            foreach(CategoryHeaderViewModel category in Categories)
            {
                //Find the related IApplicationNavigatorViewModel
                var navigatorViewModel = Items.FirstOrDefault(x => x.Category.Equals(category.AppCategory));
                if(navigatorViewModel == null)
                {
                    //No Category ???
                    Logger.Error($"Found no {typeof(IApplicationNavigatorViewModel<>)} for Category with Identifier={category.AppCategory.Identifier}");
                    continue;
                }

                var appsInNavigator = navigatorViewModel.Items.Count;
                if(appsInNavigator == 0)
                {
                    categoriesToRemove.Add(category);
                    Items.Remove(navigatorViewModel);
                    navigatorViewModel.Dispose();
                }
                else if(appsInNavigator != category.AmountOfApplications)
                {
                    category.AmountOfApplications = appsInNavigator;
                    category.DisplayOrder = appsInNavigator;
                }
            }
            Categories.RemoveRange(categoriesToRemove);
        }

        private CategoryHeaderViewModel CreateCategoryHeaderViewModel(IAppCategory category,int applicationsCount, int displayOrder)
        {
            return new CategoryHeaderViewModel(category, displayOrder, applicationsCount);
        }
        #endregion

        #region Overloads  
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if (_directionalNavigation == null && view is FrameworkElement parent)
            {
                var categorySelector = UIHelper.GetChildElementByNameFromVisualTree<Selector>(parent, nameof(Categories));
                _directionalNavigation = new WpfDirectionalNavigation<CategoryHeaderViewModel>(categorySelector);
                Logger.Debug($"WpfDirectionalNavigation initialized for Type={GetType()}");
            }
        }
        #endregion

        #region Classes
        class ApplicationLoadingResult
        {
            public ApplicationLoadingResult()
            {
                CategoryApplicationsMapping = new List<IGrouping<IAppCategory, ApplicationExecutableViewModel>>();
                CategoryMetadata = new List<CategoryHeaderViewModel>();
            }
            public IEnumerable<CategoryHeaderViewModel> CategoryMetadata { get; set; }
            public IEnumerable<IGrouping<IAppCategory, ApplicationExecutableViewModel>> CategoryApplicationsMapping { get;set;}
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _statusBarViewModel?.Dispose();
            _messageBroker.Unsubscribe(this);
        }
    }
}
