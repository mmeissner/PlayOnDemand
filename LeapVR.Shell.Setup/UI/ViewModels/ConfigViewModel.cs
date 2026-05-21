#region Licence
/****************************************************************
 *  Filename: ConfigViewModel.cs
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Setup.UI.Views;
using MaterialDesignThemes.Wpf.Transitions;
using NLog;
using LogManager = NLog.LogManager;

namespace LeapVR.Shell.Setup.UI.ViewModels
{
    public class ConfigViewModel : Screen
    {
        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ILanguageSelector _languageSelector;
        private int _currentIndex;
        private Visibility _finished = Visibility.Collapsed;
        private bool _isPreviousBlocked;
        private Visibility _nextPrevious = Visibility.Visible;
        private string _selectedAvailibleLanguage;
        private int _totalItems;
        #endregion

        #region Properties
        public Visibility NextPrevious
        {
            get => _nextPrevious;
            set
            {
                if(value == _nextPrevious) return;
                _nextPrevious = value;
                NotifyOfPropertyChange();
            }
        }
        public Visibility Finished
        {
            get => _finished;
            set
            {
                if(value == _finished) return;
                _finished = value;
                NotifyOfPropertyChange();
            }
        }
        public SettingsWizViewModel PathsPage { get; set; }
        public ApplyChangesViewModel ApplyChangesPage { get; set; }
        public RegisterAccountViewModel RegisterAccountPage { get; set; }
        public string SelectedAvailibleLanguage
        {
            get => _selectedAvailibleLanguage;
            set
            {
                if(value == _selectedAvailibleLanguage) return;
                _selectedAvailibleLanguage = value;
                _languageSelector.ActivateCultureInfo(_selectedAvailibleLanguage);
                NotifyOfPropertyChange();
            }
        }
        public IObservableCollection<string> AvailibleLanguages { get; set; } = new BindableCollection<string>();
        public bool IsPreviousBlocked
        {
            get => _isPreviousBlocked;
            set
            {
                if(value == _isPreviousBlocked) return;
                _isPreviousBlocked = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanMovePrevious));
            }
        }
        public bool CanMoveNext => CurrentIndex < _totalItems - 1 && AllowNextPage();
        public bool CanMovePrevious => CurrentIndex > 0 && !IsPreviousBlocked;
        private Transitioner TransitionModule { get; set; }
        private int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value;
                NotifyOfPropertyChange(() => CanMoveNext);
                NotifyOfPropertyChange(() => CanMovePrevious);
            }
        }
        #endregion

        #region Constructors
        public ConfigViewModel(
                SettingsWizViewModel pathsPage,
            RegisterAccountViewModel registerPage,
            ILanguageSelector languageSelector)
        {
            _languageSelector = languageSelector;
            foreach(var culture in _languageSelector.SupportedCultures)
                AvailibleLanguages.Add(culture.Name);
            SelectedAvailibleLanguage = _languageSelector.DefaultCulture.Name;
            PathsPage = pathsPage;
            RegisterAccountPage = registerPage;
            PathsPage.PropertyChanged += SubPage_PropertyChanged;
        }
        #endregion

        public async Task MoveNext()
        {
            var doWork = false;
            var nextIndex = _currentIndex + 1;
            //If we are about to apply Changes
            if(nextIndex == 1)
            {
                //Check if there is really work to do
                var workTasks = GetAllWorkTasks();
                if(workTasks.Any())
                {
                    ApplyChangesPage = new ApplyChangesViewModel(workTasks);
                    ApplyChangesPage.PropertyChanged += SubPage_PropertyChanged;
                    NotifyOfPropertyChange(nameof(ApplyChangesPage));
                    doWork = true;
                }
                else
                {
                    nextIndex = nextIndex + 1;
                }
            }
            TransitionModule.SelectedIndex = nextIndex;
            CurrentIndex = TransitionModule.SelectedIndex;
            IsPreviousBlocked = IsCurrentItemBlockingPrevious();
            if (CurrentIndex == TransitionModule.Items.Count - 1) SetToFinished();
            if(doWork) await ApplyChangesPage.DoWorkTasks();
        }
        public void MovePrevious()
        {
            TransitionModule.SelectedIndex = _currentIndex - 1;
            CurrentIndex = TransitionModule.SelectedIndex;
            IsPreviousBlocked = IsCurrentItemBlockingPrevious();
        }
        public void CloseConfig() { TryClose(); }
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if(view is ConfigView configView)
            {
                TransitionModule = configView.Wizard;
                _totalItems = TransitionModule.Items.Count;
                CurrentIndex = TransitionModule.SelectedIndex;
            }
        }
        private void SubPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IWizardPage.PageValid)) NotifyOfPropertyChange(nameof(CanMoveNext));
            if(e.PropertyName == nameof(IWizardPage.BlockPrevious))
            {
                if(TransitionModule.SelectedItem is ContentControl control)
                {
                    if(GetWizardPage(control, out var page))
                    {
                        if(page.BlockPrevious) IsPreviousBlocked = IsCurrentItemBlockingPrevious();
                    }
                }
            }
            ;
        }
        private bool AllowNextPage()
        {
            if(CurrentIndex == 0)
                return PathsPage.PageValid;
            if(CurrentIndex == 1 && ApplyChangesPage != null)
                return ApplyChangesPage.PageValid;
            return true;
        }
        private void SetToFinished()
        {
            NextPrevious = Visibility.Collapsed;
            Finished = Visibility.Visible;
        }

        private bool IsCurrentItemBlockingPrevious()
        {
            if (TransitionModule.SelectedItem is ContentControl control)
            {
                if (GetWizardPage(control, out var page))
                {
                    if (page.BlockPrevious) return true;
                }
            }
            return false;
        }
        private bool GetWizardPage(ContentControl control, out IWizardPage page)
        {
            page = null;
            if(control.Content is FrameworkElement wizard)
                if(wizard.DataContext is IWizardPage wizardmodel)
                {
                    page = wizardmodel;
                    return true;
                }
            return false;
        }
        private List<WizardWorkTasks> GetAllWorkTasks()
        {
            var retval = new List<WizardWorkTasks>();
            retval.AddRange(PathsPage.WorkTasks());
            return retval;
        }
    }
}