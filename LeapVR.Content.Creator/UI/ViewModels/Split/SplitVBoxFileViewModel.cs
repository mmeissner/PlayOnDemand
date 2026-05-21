#region Licence
/****************************************************************
 *  Filename: SplitVBoxFileViewModel.cs
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
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Creator.Language;
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.Container;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    public class SplitVBoxFileViewModel : ValidatingScreen, IWizardModule, IStepScreenWizard
    {
        //TODO[GD]: Refactor Global Systemwide Constants into a Shared Global Class
        private const string vboxV1Extension = ".vboxapp";
        private const string vboxV2Extension = ".vbox";



        public Exception OccuredException { get; }
        public IStepScreenWizard Previous { get; set; }
        public IStepScreenWizard Next { get; set; }

        public bool CanGoNext => _isContainerValid && _isOutputPathValid;

        public bool CanGoPrevious { get; }
        public bool CanGoExit => true;

        private string _splitVBoxFilePathName;
        public string SplitVBoxFilePathName
        {
            get { return _splitVBoxFilePathName; }
            set
            {
                if (value == _splitVBoxFilePathName) return;
                if (EvaluateSelectedVBoxFile(value) == false)
                {
                    _isContainerValid = false;
                    _splitVBoxFilePathName = String.Empty;
                }
                else
                {
                    _splitVBoxFilePathName = value;
                    _isContainerValid = true;
                }
                NotifyOfPropertyChange(() => SplitVBoxFilePathName);
                NotifyOfPropertyChange(()=> CanGoNext);
            }
        }

        private string _zipOutputDirectory;
        public string ZipOutputDirectory
        {
            get { return _zipOutputDirectory; }
            set
            {
                if (value == _zipOutputDirectory) return;
                _zipOutputDirectory = value;
                if(Directory.Exists(value))_isOutputPathValid = true;
                else _isOutputPathValid = false;
                NotifyOfPropertyChange(() => ZipOutputDirectory);
                NotifyOfPropertyChange(() => CanGoNext);

            }
        }

        private readonly Subject<BusyCancelableViewModel> _whenBusyRequestedSubject;
        public IObservable<BusyCancelableViewModel> WhenBusyRequested { get; }

        public bool WorkInProgress
        {
            get => _workInProgress;
            private set
            {
                if (value == _workInProgress) return;
                _workInProgress = value;
                NotifyOfPropertyChange(() => WorkInProgress);
                SpinnerVisibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private Visibility _spinnerVisibility = Visibility.Hidden;
        public Visibility SpinnerVisibility
        {
            get => _spinnerVisibility;
            private set
            {
                if (value == _spinnerVisibility) return;
                _spinnerVisibility = value;
                NotifyOfPropertyChange(() => SpinnerVisibility);
            }
        }

        private readonly IConfigFileRepository<ContentCreatorConfig> _configRepository;
        private IAppInstallationContainer<IContainerPackage> _container;
        private bool _isContainerValid;
        private bool _isOutputPathValid;
        private object lockState = new object();
        private bool _workInProgress;

        // Progress-bar state. Mirrors the create-side wizard (SummaryViewModel)
        // so the XAML can use the same binding shape: WasStarted gates
        // visibility, Done* are the running tally, Total* are the upper bound,
        // DonePercents drives the ProgressBar value 0..100.
        public bool WasStarted
        {
            get => _wasStarted;
            private set { if (_wasStarted != value) { _wasStarted = value; NotifyOfPropertyChange(); } }
        }
        public int TotalFilesCount
        {
            get => _totalFilesCount;
            private set { if (_totalFilesCount != value) { _totalFilesCount = value; NotifyOfPropertyChange(); } }
        }
        public int DoneFilesCount
        {
            get => _doneFilesCount;
            private set { if (_doneFilesCount != value) { _doneFilesCount = value; NotifyOfPropertyChange(); } }
        }
        public long TotalFilesSize
        {
            get => _totalFilesSize;
            private set { if (_totalFilesSize != value) { _totalFilesSize = value; NotifyOfPropertyChange(); } }
        }
        public long DoneFilesSize
        {
            get => _doneFilesSize;
            private set { if (_doneFilesSize != value) { _doneFilesSize = value; NotifyOfPropertyChange(); } }
        }
        public int DonePercents
        {
            get => _donePercents;
            private set { if (_donePercents != value) { _donePercents = value; NotifyOfPropertyChange(); } }
        }
        private bool _wasStarted;
        private int _totalFilesCount;
        private int _doneFilesCount;
        private long _totalFilesSize;
        private long _doneFilesSize;
        private int _donePercents;

        // Updated by the extraction loop; the dispatcher timer reads these to
        // build the aggregate Done* numbers without touching WPF from a worker.
        private IContainerPackage _activePackage;
        private long _completedPackagesBytes;
        private int _completedPackagesFiles;

        public SplitVBoxFileViewModel()
        {
            _whenBusyRequestedSubject = new Subject<BusyCancelableViewModel>();
            WhenBusyRequested = _whenBusyRequestedSubject.AsObservable();
            _configRepository = IoC.Get<IConfigFileRepository<ContentCreatorConfig>>();
        }

        public void SelectSplitVBoxFilePathName()
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = $"{Resources.Global_Browse_Container} (*{vboxV1Extension};*{vboxV2Extension})|*{vboxV1Extension};*{vboxV2Extension}|{Resources.Global_Browse_AllFiles} (*.*)|*.*"
            };
            if (ofd.ShowDialog() != true)
            {
                return;
            }
            SplitVBoxFilePathName = ofd.FileName;
        }
        public void SelectOutPutDirectory()
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = Resources.Global_Select_Directory,
                IsFolderPicker = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };


            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dlg.FileName;
                ZipOutputDirectory = folder;
            }
        }
        public async Task DoWork()
        {
            // "Unpack" semantics: each internal package lands under
            //   <chosen-out-dir>/<basename>/<ContentType>/...files...
            // — same shape the kiosk uses when it installs a .vbox at
            // runtime (ZipReadablePackage.ExtractToDirectory). Producing
            // raw folders rather than per-package .zip files lets the
            // operator inspect/repair files directly without a second
            // unzip step.

            // Pre-compute totals so the progress bar shows the real upper
            // bound from the first tick (rather than ticking up as each
            // package's header is parsed).
            var allPackages = _container.GetPackages().ToList();
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.Invoke(() =>
            {
                TotalFilesCount = allPackages.Sum(p => p.TotalFilesCount);
                TotalFilesSize = allPackages.Sum(p => p.TotalFilesSize);
                DoneFilesCount = 0;
                DoneFilesSize = 0;
                DonePercents = 0;
                WasStarted = true;
                WorkInProgress = true;
            });
            _completedPackagesBytes = 0;
            _completedPackagesFiles = 0;

            // Pull-based progress: a 200 ms UI-thread timer samples the
            // active package's Done counters (which the parallel extract
            // workers update via Interlocked) and republishes them as bound
            // VM properties. Avoids per-event marshalling from worker
            // threads back onto the dispatcher.
            var timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(200),
            };
            timer.Tick += (s, e) => RecalculateProgress();
            timer.Start();

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    var baseName = GetBaseAppName(new FileInfo(SplitVBoxFilePathName));
                    var unpackRoot = Path.Combine(ZipOutputDirectory, baseName);
                    Directory.CreateDirectory(unpackRoot);
                    foreach (var package in allPackages)
                    {
                        _activePackage = package;
                        var packageDir = Path.Combine(unpackRoot,
                            package.ContentType.ToString());
                        Directory.CreateDirectory(packageDir);
                        package.ExtractToDirectory(packageDir);
                        _completedPackagesBytes += package.TotalFilesSize;
                        _completedPackagesFiles += package.TotalFilesCount;
                    }
                    _activePackage = null;
                });
            }
            finally
            {
                timer.Stop();
                dispatcher.Invoke(() =>
                {
                    // Final snapshot — make sure the bar reaches 100% on
                    // success regardless of the timer's last tick.
                    RecalculateProgress();
                    if (_container != null && allPackages.Count > 0)
                    {
                        DoneFilesCount = TotalFilesCount;
                        DoneFilesSize = TotalFilesSize;
                        DonePercents = 100;
                    }
                    WorkInProgress = false;
                });
            }
        }

        private void RecalculateProgress()
        {
            var active = _activePackage;
            long activeBytes = active?.DoneFilesSize ?? 0;
            int activeFiles = active?.DoneFilesCount ?? 0;
            DoneFilesSize = _completedPackagesBytes + activeBytes;
            DoneFilesCount = _completedPackagesFiles + activeFiles;
            DonePercents = TotalFilesSize != 0
                ? (int)Math.Min(100, DoneFilesSize * 100 / TotalFilesSize)
                : 0;
        }

        private bool EvaluateSelectedVBoxFile(string vBoxAppFilePath)
        {
            var vBoxMainFileInfo = new FileInfo(vBoxAppFilePath);
            var containerModule = new ContainerModule(new AppInstallationHeaderSerializer());
            switch (vBoxMainFileInfo.Extension.ToLowerInvariant())
            {
                case vboxV1Extension:
                    var dataFileInfo = new FileInfo(GetAppDataFilePath(vBoxMainFileInfo));
                    if (!dataFileInfo.Exists || !vBoxMainFileInfo.Exists) return false;
                    _container = containerModule.GetAppInstallationContainer(vBoxMainFileInfo.FullName);
                    break;
                case vboxV2Extension:
                    _container = containerModule.GetAppInstallationContainer(vBoxMainFileInfo.FullName);
                    break;
                default:
                    return false;
            }
            return true;
        }
        private string GetAppDataFilePath(FileInfo headerFileInfo)
        {
            return Path.Combine(headerFileInfo.DirectoryName, $"{GetBaseAppName(headerFileInfo)}.vboxdata");
        }
        private string GetBaseAppName(FileInfo headerFileInfo)
        {
            return headerFileInfo.Name.Remove(headerFileInfo.Name.Length - headerFileInfo.Extension.Length);
        }
    }
}
