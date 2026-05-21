#region Licence
/****************************************************************
 *  Filename: SettingsWizViewModel.cs
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
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Humanizer;
using Humanizer.Bytes;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Language;
using LeapVR.Shell.Setup.UI.ViewModels.Dialog;
using LeapVR.Shell.Setup.UI.Views;
using LeapVR.Shell.Setup.UI.Views.Dialog;
using LeapVR.Utilities.Windows;
using LeapVR.Utilities.Windows.TaskScheduler;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;

namespace LeapVR.Shell.Setup.UI.ViewModels
{
    public class SettingsWizViewModel : Screen, IWizardPage
    {
        private const int SliderDivision = 10000;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConfigFileRepository<DiskConfig> _diskConfigFileRepository;
        private readonly IConfigFileRepository<SystemConfig> _systemConfigFileRepository;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly ILanguageSelector _languageSelector;
        private readonly SetupHelper _setupHelper;
        private readonly ITaskInfo _foundTaskInfo = null;
        private readonly bool _initialWerState;
        private readonly bool _initialAutoStartState;
        private readonly string _initialStorageDir;
        private readonly string _initialSelectedLanguage;
        private readonly double _initialReservedSpaceRatio;
        private readonly List<string> _windowsDefenderExcludedDirs = new List<string>();

        private bool _disableWindowsErrorReporting = true;
        private bool _autostartWithWindows = true;
        private bool _isStorageChange;
        private bool _userAcceptedStorageChange = false;
        private bool _blockPrevious;
        private bool _excludeStorageFromWindowsDefender;
        private bool _isCurrentFolderExcludedFromWinDefender;
        private string _gameStorageFilePath;
        private string _selectedAvailibleLanguage;
        private string _sliderToolTip;
        private string _freeDiskSpace;
        private string _totalDiskSpace;
        private int _freeSpaceSliderValue;
        private long _currentSelectedDriveTotalBytes;
        private DialogHost _dialogHost;
        private Visibility _diskSpaceVisibility = Visibility.Collapsed;


        public bool BlockPrevious
        {
            get { return _blockPrevious; }
            set
            {
                if(value == _blockPrevious) return;
                _blockPrevious = value;
                NotifyOfPropertyChange();
            }
        }
        public bool PageValid => true;
        public IObservableCollection<string> Languages { get; set; } = new BindableCollection<string>();
        public string GameStorageFilePath
        {
            get { return _gameStorageFilePath; }
            set
            {
                if(value == _gameStorageFilePath) return;
                _gameStorageFilePath = value;
                ExcludeStorageFromWindowsDefender = _windowsDefenderExcludedDirs.Any(x => x.ToLowerInvariant().Equals(value.ToLowerInvariant()));
                _isCurrentFolderExcludedFromWinDefender = ExcludeStorageFromWindowsDefender;
                NotifyOfPropertyChange();
            }
        }
        public bool IsStorageChange
        {
            get { return _isStorageChange; }
            set
            {
                if(value == _isStorageChange) return;
                _isStorageChange = value;
                NotifyOfPropertyChange();
            }
        }
        public Visibility DiskSpaceVisibility
        {
            get => _diskSpaceVisibility;
            set
            {
                if(value == _diskSpaceVisibility) return;
                _diskSpaceVisibility = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsPowerShellAvailible { get; }
        public int FreeSpaceSliderValue
        {
            get { return _freeSpaceSliderValue; }
            set
            {
                if(value == _freeSpaceSliderValue) return;
                _freeSpaceSliderValue = value;
                SetSliderToolTip(ConvertFromSliderValue(value));
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanIncreaseFreeSpace));
                NotifyOfPropertyChange(nameof(CanDecreaseFreeSpace));
            }
        }
        public string SliderToolTip
        {
            get { return _sliderToolTip; }
            set
            {
                if(value == _sliderToolTip) return;
                _sliderToolTip = value;
                NotifyOfPropertyChange();
            }
        }
        public string TotalDiskSpace
        {
            get => _totalDiskSpace;
            set
            {
                if(value == _totalDiskSpace) return;
                _totalDiskSpace = value;
                NotifyOfPropertyChange();
            }
        }
        public string FreeDiskSpace
        {
            get => _freeDiskSpace;
            set
            {
                if(value == _freeDiskSpace) return;
                _freeDiskSpace = value;
                NotifyOfPropertyChange();
            }
        }
        public bool AutostartWithWindows
        {
            get { return _autostartWithWindows; }
            set
            {
                if(value == _autostartWithWindows) return;
                _autostartWithWindows = value;
                NotifyOfPropertyChange();
            }
        }
        public bool DisableWindowsErrorReporting
        {
            get { return _disableWindowsErrorReporting; }
            set
            {
                if(value == _disableWindowsErrorReporting) return;
                _disableWindowsErrorReporting = value;
                NotifyOfPropertyChange();
            }
        }
        public bool ExcludeStorageFromWindowsDefender
        {
            get { return _excludeStorageFromWindowsDefender; }
            set
            {
                if(value == _excludeStorageFromWindowsDefender) return;
                _excludeStorageFromWindowsDefender = value;
                NotifyOfPropertyChange();
            }
        }
        public string SelectedLanguage
        {
            get { return _selectedAvailibleLanguage; }
            set
            {
                if(value == _selectedAvailibleLanguage) return;
                _selectedAvailibleLanguage = value;
                _languageSelector.ActivateCultureInfo(_selectedAvailibleLanguage);
                NotifyOfPropertyChange();
            }
        }
        public SettingsWizViewModel(IConfigFileRepository<DiskConfig> diskConfigFileRepository,
            IConfigFileRepository<SystemConfig> systemConfigFileRepository,SetupHelper setupHelper,
            ILanguageSelector languageSelector)
        {
            _globalConfiguration = GlobalConfig.GetGlobalConfiguration();
            _systemConfigFileRepository = systemConfigFileRepository;
            _diskConfigFileRepository = diskConfigFileRepository;
            _languageSelector = languageSelector;
            _setupHelper = setupHelper;
            foreach(var culture in _languageSelector.SupportedCultures)
            {
                Languages.Add(culture.Name);
            }
            var diskConfig = _diskConfigFileRepository.Get();
            var sysConfig = _systemConfigFileRepository.Get();
            _initialStorageDir = diskConfig.StorageBaseDir;
            _initialReservedSpaceRatio = diskConfig.ReservedDiskSpaceRatio;
            _initialWerState = !_setupHelper.IsWerEnabled();
            _initialAutoStartState = TaskSchedulerUtil.GetTaskInfo(sysConfig.LeapVRTaskName, out _foundTaskInfo) &&
                                     _foundTaskInfo.Enabled;
            _initialSelectedLanguage = _languageSelector.DefaultCulture.Name;
            IsPowerShellAvailible = _setupHelper.GetExcludedFoldersFromWinDefender(out _windowsDefenderExcludedDirs);
            if(String.IsNullOrEmpty(_initialStorageDir))
            {
                var suggestion = GetGameDirSuggestion();
                if(!String.IsNullOrEmpty(suggestion))
                {
                    GameStorageFilePath = GetGameDirSuggestion();
                    LoadGameStoragePath(suggestion);
                }
            }
            else
            {
                LoadGameStoragePath(_initialStorageDir);
                IsStorageChange = !IsDirectoryNewOrEmpty(_initialStorageDir);
            }
            DisableWindowsErrorReporting = _initialWerState;
            AutostartWithWindows = _initialAutoStartState;
            SelectedLanguage = _initialSelectedLanguage;
            FreeSpaceSliderValue = ConvertToSliderValue(_initialReservedSpaceRatio);
        }

        public IEnumerable<WizardWorkTasks> WorkTasks()
        {
            var retval = new List<WizardWorkTasks>();
            if(_initialWerState != DisableWindowsErrorReporting)
            {
                //Change WER
                retval.Add(new WizardWorkTasks(async () =>
                await _setupHelper.SetWer(!DisableWindowsErrorReporting), "Config_WER_Updated"));
            }
            if(_initialAutoStartState != AutostartWithWindows)
            {
                //Change AutoStart
                retval.Add(
                    new WizardWorkTasks(async () =>
                    await _setupHelper.ChangeAutoStart(AutostartWithWindows),"Config_StartupTask_Created"));
            }
            if(_initialStorageDir != GameStorageFilePath ||
               Math.Abs(_initialReservedSpaceRatio - ConvertFromSliderValue(FreeSpaceSliderValue)) > 1d/(SliderDivision *100))
            {
                //Save DiskValues
                retval.Add(
                    new WizardWorkTasks(async () =>
                    await _setupHelper.SaveDiskValues(GameStorageFilePath,FreeSpaceSliderValue,SliderDivision), "Config_Storage_Updated"));
            }
            if(_initialSelectedLanguage != SelectedLanguage)
            {
                //Needs to Change Language
                retval.Add(new WizardWorkTasks(async () =>
                await SaveLanguage(), "Config_DefaultLanguage_Changed" ));
            }
            if(_isCurrentFolderExcludedFromWinDefender != _excludeStorageFromWindowsDefender)
            {
                retval.Add(new WizardWorkTasks(async () =>
                await _setupHelper.ExcludeStorageFromWinDefender(_excludeStorageFromWindowsDefender,GameStorageFilePath), "Config_ExcludingStorageFromWD"));
            }
            return retval;
        }
        private Task<bool> SaveLanguage()
        {
            return Task.Run(
                () =>
                _languageSelector.SetDefaultCulture(CultureInfo.GetCultureInfo(SelectedLanguage)));
        }
        
        public async Task SelectGameDirectory()
        {
            if(IsStorageChange && !_userAcceptedStorageChange)
            {
                var dialogViewModel = new WarnStorageChangeViewModel(_dialogHost);
                var dialogView = new WarnStorageChangeView();
                await ShowDialog(dialogView, dialogViewModel);
                if(dialogViewModel.UserUnderstood)
                {
                    _userAcceptedStorageChange = true;
                    await SetGameDirectory();
                }
            }
            else
            {
                await SetGameDirectory();
            }
        }

        public bool CanIncreaseFreeSpace => FreeSpaceSliderValue < 10000;
        public bool CanDecreaseFreeSpace => FreeSpaceSliderValue > 0;
        public void IncreaseFreeSpace()
        {
            if(FreeSpaceSliderValue >= 10000) return;
            FreeSpaceSliderValue = FreeSpaceSliderValue + 1;
        }
        public void DecreaseFreeSpace()
        {
            if(FreeSpaceSliderValue <= 0) return;
            FreeSpaceSliderValue = FreeSpaceSliderValue - 1;
        }

        private async Task SetGameDirectory()
        {
            var folderSelectorDialog = new CommonOpenFileDialog
                                       {
                                           EnsureReadOnly = true,
                                           IsFolderPicker = true,
                                           AllowNonFileSystemItems = false,
                                           Multiselect = false,
                                           InitialDirectory = GameStorageFilePath,
                                           Title = Resources.Config_Select_GameStorage_Dir
                                       };
            if(folderSelectorDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }
            if(!IsDirectoryNewOrEmpty(folderSelectorDialog.FileName))
            {
                var dialogViewModel = new WarnStorageNotEmptyViewModel(_dialogHost);
                var dialogView = new WarnStorageNotEmptyView();
                await ShowDialog(dialogView, dialogViewModel);
                if(dialogViewModel.UserUnderstood)
                {
                    GameStorageFilePath = folderSelectorDialog.FileName;
                    LoadGameStoragePath(GameStorageFilePath);
                }
            }
            else
            {
                GameStorageFilePath = folderSelectorDialog.FileName;
                LoadGameStoragePath(GameStorageFilePath);
            }
        }
        private void LoadGameStoragePath(string storagePath)
        {
            if(String.IsNullOrWhiteSpace(storagePath))
            {
                DiskSpaceVisibility = Visibility.Hidden;
                return;
            }
            else
            {
                GameStorageFilePath = storagePath;
                var driveLetter = new DirectoryInfo(GameStorageFilePath).Root.Name;
                DriveInfo driveInfo = new DriveInfo(driveLetter);
                var freeSpace = driveInfo.AvailableFreeSpace.Bytes();
                var totalSpace = driveInfo.TotalSize.Bytes();
                _currentSelectedDriveTotalBytes = driveInfo.TotalSize;
                FreeDiskSpace = $"{freeSpace.ToString("#.#")}";
                TotalDiskSpace = $"{totalSpace.ToString("#.#")}";
                FreeSpaceSliderValue = ConvertToSliderValue(_initialReservedSpaceRatio);
                SetSliderToolTip(ConvertFromSliderValue(FreeSpaceSliderValue));
                DiskSpaceVisibility = Visibility.Visible;
            }
        }
        private bool IsDirectoryNewOrEmpty(string directoryName)
        {
            if(!String.IsNullOrWhiteSpace(directoryName))
            {
                var dirInfo = new DirectoryInfo(directoryName);
                if(dirInfo.Exists)
                {
                    if(dirInfo.EnumerateDirectories().Any() || dirInfo.EnumerateFiles().Any()) return false;
                }
            }
            return true;
        }

        private int ConvertToSliderValue(double ratioValue) { return Convert.ToInt32(ratioValue * SliderDivision); }
        private double ConvertFromSliderValue(int sliderValue)
        {
            if(sliderValue == 0) return 0;
            var sliderValDouble = Convert.ToDouble(sliderValue);
            return sliderValDouble / SliderDivision;
        }
        private void SetSliderToolTip(double value)
        {
            var bytes = Convert.ToInt64(_currentSelectedDriveTotalBytes * value);
            if(bytes < ByteSize.BytesInMegabyte) SliderToolTip = bytes.Bytes().ToString("0.# MB");
            else SliderToolTip = bytes.Bytes().ToString("#.#");
        }
        private string GetGameDirSuggestion()
        {
            var drives =DriveInfo.GetDrives();
            DriveInfo biggestDrive = null;
            foreach(DriveInfo info in drives)
            {
                if(info.DriveType == DriveType.Fixed)
                {
                    if(biggestDrive == null || biggestDrive.AvailableFreeSpace < info.AvailableFreeSpace)
                        biggestDrive = info;
                }
            }

            if(biggestDrive != null)
            {
                var suggestion = $"{biggestDrive.Name}{DiskConfig.DefaultGamesSubDir}";
                //Suggestion must be new or empty otherwise user has to choice
                if(Directory.Exists(suggestion))
                {
                    foreach(string entry in Directory.EnumerateFileSystemEntries(suggestion))
                    {
                        return "";
                    }
                }

                return suggestion;
            }
            return "";
        }
        private async Task ShowDialog<T, TT>(T view, TT viewModel)
                where T : DependencyObject, System.Windows.Markup.IComponentConnector where TT : Screen
        {
            view.InitializeComponent();
            ViewModelBinder.Bind(viewModel, view, null);
            BlockPrevious = true;
            await DialogHost.Show(view);
            BlockPrevious = false;
        }
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if(view is SettingsWizView settingsWizView)
            {
                _dialogHost = settingsWizView.DialogHost;
            }
        }
    }


    public class WizardWorkTasks
    {
        private Func<Task<bool>> _workTask;
        public WizardWorkTasks(Func<Task<bool>> workTask, string resourceKey)
        {
            ResourceKey = resourceKey;
            _workTask = workTask;
        }
        public string ResourceKey { get; }
        public Func<Task<bool>> WorkTask => _workTask;
    }
}