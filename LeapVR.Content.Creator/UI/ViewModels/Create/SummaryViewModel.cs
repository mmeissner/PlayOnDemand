#region Licence
/****************************************************************
 *  Filename: SummaryViewModel.cs
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
using System.Windows.Forms;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Creator.Language;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    public class SummaryViewModel : ValidatingScreen, IStepScreenCreate
    {
        #region Fields & Properties

        private readonly IConfigFileRepository<ContentCreatorConfig> _contentCreatorRepo;
        private ContentCreatorConfig _config;

        private const string VrLeapHeaderExtension = ".vbox";

        public IStepScreenWizard Previous { get; set; }
        public IStepScreenWizard Next { get; set; }

        public bool IsPathManualEditEnabled => _contentCreatorRepo.Get().IsPathManualEditEnabled;

        private string _fileName;
        private string _outputFilePath;
        public string OutputFilePath
        {
            get => _outputFilePath;
            set
            {
                _outputFilePath = value;
                PackageCreation.ContainerOutputFilePath = value;
                ValidateOutputDirectory();
                NotifyOfPropertyChange();
            }
        }

        private int _totalFilesCount;
        public int TotalFilesCount
        {
            get => _totalFilesCount;
            set
            {
                _totalFilesCount = value;
                NotifyOfPropertyChange();
            }
        }

        private int _doneFilesCount;
        public int DoneFilesCount
        {
            get => _doneFilesCount;
            set
            {
                _doneFilesCount = value;
                NotifyOfPropertyChange();
            }
        }

        private long _totalFilesSize;
        public long TotalFilesSize
        {
            get => _totalFilesSize;
            set
            {
                _totalFilesSize = value;
                NotifyOfPropertyChange();
            }
        }

        private long _doneFilesSize;
        public long DoneFilesSize
        {
            get => _doneFilesSize;
            set
            {
                _doneFilesSize = value;
                NotifyOfPropertyChange();
            }
        }

        private int _donePercents;
        public int DonePercents
        {
            get => _donePercents;
            set
            {
                _donePercents = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _wasStarted;
        public bool WasStarted
        {
            get => _wasStarted;
            set
            {
                _wasStarted = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isEnded;
        public bool IsEnded
        {
            get => _isEnded;
            set
            {
                _isEnded = value;
                NotifyOfPropertyChange();
            }
        }

        private Exception _exception;
        public Exception Exception
        {
            get => _exception;
            set
            {
                _exception = value;
                NotifyOfPropertyChange();
            }
        }

        public bool CanGoNext => IsValid && !WasStarted;
        public bool CanGoPrevious => !WasStarted;
        public bool CanGoExit => !WasStarted;
        protected override bool IsAllRequiredDataFilled => !string.IsNullOrEmpty(OutputFilePath);

        public ContainerCreation PackageCreation { get; }

        private readonly Subject<BusyCancelableViewModel> _whenBusyRequestedSubject;
        public IObservable<BusyCancelableViewModel> WhenBusyRequested { get; }

        #endregion

        #region Constructors

        public SummaryViewModel(ContainerCreation packageCreation, IConfigFileRepository<ContentCreatorConfig> contentCreatorRepo)
        {
            QuickLeap.AssertNotNull(packageCreation, contentCreatorRepo);
            _contentCreatorRepo = contentCreatorRepo;
            PackageCreation = packageCreation;
            packageCreation.WhenContainerCreationStarted.Subscribe(q => RecalculateProgress());
            packageCreation.WhenProgressChanged.Subscribe(q => RecalculateProgress());
            packageCreation.WhenContainerCreationEnded.Subscribe(q => RecalculateProgress());

            _whenBusyRequestedSubject = new Subject<BusyCancelableViewModel>();
            WhenBusyRequested = _whenBusyRequestedSubject.AsObservable();
        }

        #endregion

        #region Methods
        protected override void OnActivate()
        {
            try
            {
                RecalculateProgress();
            }
            catch (Exception e) // TODO [RM]: temp only
            {
                MessageBox.Show(e.ToString(), "OnActivate(): EXCEPTION");
            }
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            _config = _contentCreatorRepo.Get();

            if (string.IsNullOrEmpty(_fileName)) _fileName = PackageCreation.DisplayName;

            if (Path.GetInvalidFileNameChars().Any(invalidFileNameChar => _fileName.Contains(invalidFileNameChar)))
            {
                MessageBox.Show(string.Format(Resources.Validation_InvalidFilenameCharFormat, _config.DefaultInvalidFileNameCharReplacement), Resources.Global_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
            {
                if (_fileName.Contains(invalidFileNameChar))
                {
                    _fileName = _fileName.Replace(invalidFileNameChar, _config.DefaultInvalidFileNameCharReplacement);
                }
            }

            var targetPath = Path.Combine(_config.LastOutputPackageDirectory, $"{_fileName}{VrLeapHeaderExtension}");
            OutputFilePath = targetPath;
        }

        public void BrowseOutputFilePath()
        {
            var sfd = new SaveFileDialog
            {
                InitialDirectory = _config.LastOutputPackageDirectory,
                Filter = $@"{Resources.Global_Browse_Container} (*{VrLeapHeaderExtension})|*{VrLeapHeaderExtension};",
                FileName = _fileName,
            };
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var filePath = sfd.FileName;
            _fileName = Path.GetFileNameWithoutExtension(filePath);
            _config.LastOutputPackageDirectory = Path.GetDirectoryName(filePath);
            SaveToConfigFile();
            RenewOutputPath();
        }

        private void RenewOutputPath()
        {
            var targetPath = Path.Combine(_config.LastOutputPackageDirectory, $"{_fileName}{VrLeapHeaderExtension}");
            if (File.Exists(targetPath))
            {
                if (MessageBox.Show(Resources.Summary_Output_FileAlreadyExists, Resources.Global_Warning, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
            }
            OutputFilePath = targetPath;
        }

        protected override void OnRevalidated(bool isValid)
        {
            NotifyOfPropertyChange(nameof(CanGoNext));
        }

        private void SaveToConfigFile()
        {
            _contentCreatorRepo.Store(_config);
        }

        private void ValidateOutputDirectory()
        {
            UpdateValidationError(nameof(OutputFilePath), null);
        }

        private void RecalculateProgress()
        {
            var packageCreation = PackageCreation;

            TotalFilesCount = packageCreation.TotalFilesCount;
            DoneFilesCount = packageCreation.DoneFilesCount;
            TotalFilesSize = packageCreation.TotalFilesSize;
            DoneFilesSize = packageCreation.DoneFilesSize;

            WasStarted = packageCreation.WasContainerCreationStarted;
            IsEnded = packageCreation.IsContainerCreationEnded;
            Exception = packageCreation.OccuredException;

            DonePercents = TotalFilesSize != 0 ? (int)(DoneFilesSize * 100 / TotalFilesSize) : 0;

            NotifyOfPropertyChange(nameof(CanGoNext));
            NotifyOfPropertyChange(nameof(CanGoPrevious));
            NotifyOfPropertyChange(nameof(CanGoExit));

        }

        #endregion
    }
}
