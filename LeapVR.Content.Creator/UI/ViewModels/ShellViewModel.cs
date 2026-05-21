#region Licence
/****************************************************************
 *  Filename: ShellViewModel.cs
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
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Content.Creator.Logic;
using LeapVR.ContentCreator;
using LeapVR.Content.Creator.Language;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Vr;
using Microsoft.Win32;
using WPFLocalizeExtension.Engine;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    public sealed class ShellViewModel : Screen, IShell
    {
        #region Fields & Properties

        private readonly IConfigFileRepository<ContentCreatorConfig> _contentCreatorConfigRepo;
        private readonly IEnumerable<IVrModule> _vrModules;
        private readonly IWindowManager _windowManager;
        private static readonly CultureInfo CultureEnglish = CultureInfo.GetCultureInfo("en-US");
        private static readonly CultureInfo CultureChinese = CultureInfo.GetCultureInfo("zh-CN");

        private PlatformType[] _platforms;
        public PlatformType[] Platforms
        {
            get { return _platforms; }
            private set
            {
                _platforms = value;
                NotifyOfPropertyChange();
            }
        }

        private PlatformType _selectedPlatform;
        public PlatformType SelectedPlatform
        {
            get { return _selectedPlatform; }
            set
            {
                _selectedPlatform = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanGoCreate => true;
        public bool CanGoSplit => true;
        public bool CanGoEdit => true;

        private IDisposable _wizzardBusyRequestedSubscription;
        private WizardViewModel _wizard;
        public WizardViewModel Wizard
        {
            get { return _wizard; }
            set
            {
                _wizard = value;
                NotifyOfPropertyChange();
            }
        }

        private IDisposable _busyOperationEndedSubscription;
        private BusyCancelableViewModel _busyViewModel;
        public BusyCancelableViewModel BusyViewModel
        {
            get { return _busyViewModel; }
            private set
            {
                _busyViewModel = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsBusy));
            }
        }
        public string CurrentCultureName => LocalizeDictionary.Instance.Culture?.Name;

        public bool IsBusy => _busyViewModel != null;

        #endregion

        #region Constructors

        public ShellViewModel(IConfigFileRepository<ContentCreatorConfig> contentCreatorConfigRepo, IEnumerable<IVrModule> vrModules, IWindowManager windowManager)
        {
            QuickLeap.AssertNotNull(contentCreatorConfigRepo, windowManager);
            _vrModules = vrModules ?? new IVrModule[] { };
            _windowManager = windowManager;
            DisplayName = Resources.Global_ProductName; // TODO [RM]: handle localization of code access to string localized resources
            LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
            _contentCreatorConfigRepo = contentCreatorConfigRepo;
            var config = _contentCreatorConfigRepo.Get();
            switch (config.Language)
            {
                case "schinese":
                    LocalizeDictionary.Instance.Culture = CultureChinese;
                    break;
                case "english":
                default:
                    LocalizeDictionary.Instance.Culture = CultureEnglish;
                    break;
            }
            NotifyOfPropertyChange(() => CurrentCultureName);
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(LocalizeDictionary.Culture)))
            {
                Platforms = GetAvailablePlatforms();
            }
        }

        #endregion

        #region Methods

        protected override void OnViewLoaded(object view)
        {
            Platforms = GetAvailablePlatforms();

            SelectedPlatform = Platforms.FirstOrDefault();

            base.OnViewLoaded(view);
        }

        public void English()
        {
            LocalizeDictionary.Instance.Culture = CultureEnglish;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = CultureEnglish;
            var config = _contentCreatorConfigRepo.Get();
            config.Language = "english";
            _contentCreatorConfigRepo.Store(config);
        }

        public void Chinese()
        {
            LocalizeDictionary.Instance.Culture = CultureChinese;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = CultureChinese;
            var config = _contentCreatorConfigRepo.Get();
            config.Language = "schinese";
            _contentCreatorConfigRepo.Store(config);
        }

        public void GoCreate()
        {
            IEnumerable<IStepScreenCreate> steps;
            ContainerCreation packageCreation;

            switch (SelectedPlatform)
            {
                case PlatformType.LeapVr:
                    packageCreation = new LeapVrContainerCreation();
                    steps = new IStepScreenCreate[]
                    {
                        new LeapVrAppExecutableInfoViewModel((LeapVrContainerCreation)packageCreation,_contentCreatorConfigRepo,_vrModules,_windowManager),
                        new AppDetailInfoViewModel(packageCreation,_contentCreatorConfigRepo),
                        new SummaryViewModel(packageCreation,_contentCreatorConfigRepo),
                    };
                    break;
                case PlatformType.Steam:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException($"Unknown {nameof(SelectedPlatform)} value `{SelectedPlatform}`.");
            }

            Wizard = new WizardViewModel(packageCreation, steps);
            _wizzardBusyRequestedSubscription = Wizard.WhenBusyRequested.Subscribe(OnWizardBusyRequested);
        }

        public void GoEdit()
        {
            //throw new NotImplementedException("This functionality is under development and not yet implemented!");

            string vBoxHeaderFilePathName;
            var ofd = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = $"{Resources.Global_Browse_Container} (*.vboxapp;*.vbox)|*.vboxapp;*.vbox|{Resources.Global_Browse_AllFiles} (*.*)|*.*"
            };
            if (ofd.ShowDialog() != true)
            {
                return;
            }

            vBoxHeaderFilePathName = ofd.FileName;

            var leapVrContainerEditor = new LeapVrContainerEditor(vBoxHeaderFilePathName);
            if (!leapVrContainerEditor.IsValid)
            {
                MessageBox.Show(Resources.Validation_ErrorVBoxPackageOpenEdit,
                    Resources.Validation_VBoxPackage_Edit_MsgBox_Error_Caption, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Edit wizard exposes only steps with wired XAML + data
            // binding. EditPlatformData / EditPackage / EditExecutionLogic
            // remain as placeholder view-models and stub <Grid /> XAML;
            // listing them in the wizard shows blank pages to the operator.
            // Bring them back here as they grow real UIs.
            IEnumerable<IStepScreenEdit> steps;
            steps = new IStepScreenEdit[]
            {
                new EditAppDetailInfoViewModel(leapVrContainerEditor),
            };
            Wizard = new WizardViewModel(leapVrContainerEditor, steps);

            _wizzardBusyRequestedSubscription = Wizard.WhenBusyRequested.Subscribe(OnWizardBusyRequested);
        }

        public void GoSplit()
        {
            var module = new SplitVBoxFileViewModel();
            IEnumerable<IStepScreenWizard> steps = new IStepScreenWizard[]
            {
                module
            };
            Wizard = new WizardViewModel(module, steps);

            _wizzardBusyRequestedSubscription = Wizard.WhenBusyRequested.Subscribe(OnWizardBusyRequested);
        }

        public void ExitWizard()
        {
            _wizzardBusyRequestedSubscription?.Dispose();
            Wizard = null;
        }

        private PlatformType[] GetAvailablePlatforms()
        {
            var config = _contentCreatorConfigRepo.Get();
            return (from platform in config.AvailablePlatforms select (PlatformType)platform).ToArray();
        }

        private void OnWizardBusyRequested(BusyCancelableViewModel busy)
        {
            if (BusyViewModel != null)
            {
                throw new InvalidOperationException("BusyViewModel != null");
            }

            BusyViewModel = busy;
            _busyOperationEndedSubscription = busy.WhenOperationEnded.ObserveOnDispatcher().Subscribe(q => OnBusyOperationEnded());
        }

        private void OnBusyOperationEnded()
        {
            _busyOperationEndedSubscription.Dispose();
            BusyViewModel = null;
        }

        #endregion
    }
}
