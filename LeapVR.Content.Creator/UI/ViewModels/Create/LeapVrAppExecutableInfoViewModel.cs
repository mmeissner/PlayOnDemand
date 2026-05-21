#region Licence
/****************************************************************
 *  Filename: LeapVrAppExecutableInfoViewModel.cs
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
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Util.Util;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Vr;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    public class LeapVrAppExecutableInfoViewModel : ValidatingScreen, IStepScreenCreate
    {
        #region Fields & Properties

        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConfigFileRepository<ContentCreatorConfig> _contentCreatorRepo;
        private readonly Subject<BusyCancelableViewModel> _whenBusyRequestedSubject = new Subject<BusyCancelableViewModel>();
        private readonly LeapVrContainerCreation _packageCreation;
        private readonly IEnumerable<IVrModule> _vrModules;
        private readonly IWindowManager _windowManager;

        private string _lastGameBaseDirectory;
        private string _applicationRootDirectory;
        private AppExecuteInstructionViewModel _selectedExecutionInstruction;

        public IStepScreenWizard Previous { get; set; }
        public IStepScreenWizard Next { get; set; }
        public bool CanGoNext => ExecutionInstructions.Any() && !String.IsNullOrEmpty(ApplicationRootDirectory);
        public bool CanGoPrevious => true;
        public bool CanGoExit => true;
        public IObservable<BusyCancelableViewModel> WhenBusyRequested { get; }
        public ContainerCreation PackageCreation => _packageCreation;

        public string ApplicationRootDirectory
        {
            get => _applicationRootDirectory;
            set
            {
                _applicationRootDirectory = value;
                _packageCreation.AppBaseDirectory = value;
                ValidateGameBaseDirectory();
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanAddExecutionInstruction));
            }
        }
        public IObservableCollection<AppExecuteInstructionViewModel> ExecutionInstructions { get; }

        public AppExecuteInstructionViewModel SelectedExecutionInstruction
        {
            get => _selectedExecutionInstruction;
            set
            {
                if (Equals(value, _selectedExecutionInstruction)) return;
                _selectedExecutionInstruction = value;
                NotifyOfPropertyChange(() => SelectedExecutionInstruction);
                NotifyOfPropertyChange(() => CanEditExecutionInstruction);
                NotifyOfPropertyChange(() => CanRemoveExecutionInstruction);
            }
        }

        public bool IsPathManualEditEnabled => _contentCreatorRepo.Get().IsPathManualEditEnabled;
        #endregion

        #region Constructors

        public LeapVrAppExecutableInfoViewModel(LeapVrContainerCreation packageCreation, IConfigFileRepository<ContentCreatorConfig> contentCreatorRepo, IEnumerable<IVrModule> vrModules, IWindowManager windowManager)
        {
            QuickLeap.AssertNotNull(packageCreation,contentCreatorRepo, windowManager);
            ExecutionInstructions = new BindableCollection<AppExecuteInstructionViewModel>();
            ExecutionInstructions.CollectionChanged += ExecutionInstructions_CollectionChanged;
            _windowManager = windowManager;
            WhenBusyRequested = _whenBusyRequestedSubject.AsObservable();
            _vrModules = vrModules;
            _packageCreation = packageCreation;
            _contentCreatorRepo = contentCreatorRepo;
        }

        private void ExecutionInstructions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(()=> CanGoNext);
            NotifyOfPropertyChange(()=> CanBrowseApplicationRootDirectory);
            PackageCreation.Executables = new List<IAppExecuteInstruction>(ExecutionInstructions);
        }

        #endregion

        #region Methods

        public bool CanBrowseApplicationRootDirectory => !ExecutionInstructions.Any();
        public void BrowseApplicationRootDirectory()
        {
            var folderSelectorDialog = new CommonOpenFileDialog
            {
                EnsureReadOnly = true,
                IsFolderPicker = true,
                AllowNonFileSystemItems = false,
                Multiselect = false,
                InitialDirectory = _lastGameBaseDirectory,
                Title = "App Directory Location"
            };
            if (folderSelectorDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }

            _packageCreation.SteamId = null;
            ApplicationRootDirectory = folderSelectorDialog.FileName;
            _lastGameBaseDirectory = ApplicationRootDirectory;
            var directoryAnalyzer =
                new DirectoryAnalyzer(null, ApplicationRootDirectory, 1, true, false);
            directoryAnalyzer.AnalyzeFolderType();
            // Steam app id is set by the operator in the next wizard step.
            // Prior versions tried to auto-fill it by sniffing crack-launcher
            // .ini files inside the dropped folder; that whole code path was
            // removed when the SmartSteamEmu / CODEX / SKIDROW / ALI213 /
            // SteamPunks / SGR / 3DM detection was dropped from the v1.0.0
            // open-source release.
        }

        public bool CanAddExecutionInstruction => ApplicationRootDirectory != null;
        public void AddExecutionInstruction()
        {
            var viewModel = new AppExecuteInstructionViewModel(_vrModules, ApplicationRootDirectory);
            var retval = _windowManager.ShowDialog(viewModel,null, GetInstructionWindowSettings());
            if (retval.HasValue && retval.Value)
            {
                viewModel.CancelButtonVisibility = Visibility.Hidden;
                ExecutionInstructions.Add(viewModel);
            }
        }

        public bool CanEditExecutionInstruction => SelectedExecutionInstruction != null;

        public void EditExecutionInstruction()
        {
            _windowManager.ShowDialog(SelectedExecutionInstruction, null, GetInstructionWindowSettings());
        }
        public bool CanRemoveExecutionInstruction => SelectedExecutionInstruction != null;
        public void RemoveExecutionInstruction()
        {
            ExecutionInstructions.Remove(SelectedExecutionInstruction);
        }
        private void ValidateGameBaseDirectory()
        {
            UpdateValidationError(nameof(ApplicationRootDirectory), null);
        }

        private ExpandoObject GetInstructionWindowSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.Height = 600;
            settings.Width = 1280;
            settings.SizeToContent = SizeToContent.Manual;
            settings.ResizeMode = ResizeMode.NoResize;
            settings.Title = "Execution Instructions";
            return settings;
        }
        protected override bool IsAllRequiredDataFilled => !string.IsNullOrEmpty(ApplicationRootDirectory);
        protected override void OnViewLoaded(object view)
        {
            _lastGameBaseDirectory = _contentCreatorRepo.Get().LastBaseDirectory;

            base.OnViewLoaded(view);
        }
        protected override void OnDeactivate(bool close)
        {
            var config = _contentCreatorRepo.Get();
            config.LastBaseDirectory = _lastGameBaseDirectory;
            _contentCreatorRepo.Store(config);
            base.OnDeactivate(close);
        }
        protected override void OnRevalidated(bool isValid)
        {
            NotifyOfPropertyChange(nameof(CanGoNext));
        }

        #endregion
    }
}

