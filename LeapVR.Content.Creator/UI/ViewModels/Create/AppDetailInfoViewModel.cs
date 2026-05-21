#region Licence
/****************************************************************
 *  Filename: AppDetailInfoViewModel.cs
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
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Humanizer;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Creator;
using LeapVR.Content.Creator.Language;
using LeapVR.Content.Creator.UI.ViewModels.Category;
using LeapVR.Shared.Lib;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.Customization;
using Microsoft.Win32;
using NLog;

namespace LeapVR.Content.Creator.UI.ViewModels
{
    public class AppDetailInfoViewModel : ValidatingScreen, IStepScreenCreate, IDisposable
    {
        #region Fields & Properties

        private readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IConfigFileRepository<ContentCreatorConfig> _configRepository;
        private ContentCreatorConfig _config;


        public IStepScreenWizard Previous { get; set; }
        public IStepScreenWizard Next { get; set; }

        private string _steamId;
        public string SteamId
        {
            get => _steamId;
            set
            {
                _steamId = value;
                _isSteamIdChangedByUser = true;
                PackageCreation.SteamId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanReacquireSteamData));
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                PackageCreation.DisplayName = value;
                ValidateTitle();
                NotifyOfPropertyChange();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                PackageCreation.Description = value;
                ValidateDescription();
                NotifyOfPropertyChange();
            }
        }

        private BindableCollection<CategoryViewModel> _categories = new BindableCollection<CategoryViewModel>();
        public BindableCollection<CategoryViewModel> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                NotifyOfPropertyChange();
            }
        }

        private CategoryViewModel _selectedCategory;
        public CategoryViewModel SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if(value == null) value = Categories.FirstOrDefault();
                _selectedCategory = value;
                PackageCreation.Category = value.ResourceKey;
                ValidateSelectedCategory();
                NotifyOfPropertyChange();
            }
        }

        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set
            {
                _image = value;
                ValidateImagePath();
                NotifyOfPropertyChange(nameof(ImagePath));
                NotifyOfPropertyChange();
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                PackageCreation.MainPictureFilePath = value;
                LoadImageFromImagePath(_imagePath);
                NotifyOfPropertyChange();
            }
        }

        public string Tags
        {
            get => _tags;
            set
            {
                if (value == _tags) return;
                _tags = value;
                PackageCreation.Tags = value;
                NotifyOfPropertyChange(() => Tags);
            }
        }

        private bool _isDataLoadingFromSteamId;
        public bool IsDataLoadingFromSteamId => _isDataLoadingFromSteamId;

        public ContainerCreation PackageCreation { get; }

        public bool CanGoNext => IsValid;
        public bool CanGoPrevious => true;
        public bool CanGoExit => true;

        private bool _isSteamIdChangedByUser;
        public bool CanReacquireSteamData => !_isDataLoadingFromSteamId
                                             && !string.IsNullOrEmpty(SteamId);

        private readonly Subject<BusyCancelableViewModel> _whenBusyRequestedSubject;
        public IObservable<BusyCancelableViewModel> WhenBusyRequested { get; }

        protected override bool IsAllRequiredDataFilled => !string.IsNullOrEmpty(Title)
                                                           && SelectedCategory != null
                                                           && !string.IsNullOrEmpty(ImagePath)
                                                           && Image != null;

        private string _steamIdOfLastLoadedData;
        private SteamApiDataAcquisition _steamAcq;
        private BusyCancelableViewModel _steamAcqBusy;
        private volatile bool _isSteamAcqCanceled;
        private IDisposable _dataAcqEndedSubscription;
        private string _tags;

        #endregion

        #region Constructors

        public AppDetailInfoViewModel(ContainerCreation packageCreation, IConfigFileRepository<ContentCreatorConfig> configRepository)
        {
            QuickLeap.AssertNotNull(packageCreation, configRepository);
            PackageCreation = packageCreation;
            _configRepository = configRepository;
            _whenBusyRequestedSubject = new Subject<BusyCancelableViewModel>();
            WhenBusyRequested = _whenBusyRequestedSubject.AsObservable();
            PopulateCategories();
        }

        #endregion

        #region Methods

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
            _config = _configRepository.Get();

            Title = _config.LastApplicationTitle;
            if(_config.LastCategory != null)
            {
                var foundCategory = Categories.FirstOrDefault(x => x.ResourceKey.Equals(_config.LastCategory));
                if(foundCategory != null) SelectedCategory = foundCategory;
                else SelectedCategory = Categories.FirstOrDefault();
            }
            else SelectedCategory = Categories.FirstOrDefault();

            _steamId = PackageCreation.SteamId;
            NotifyOfPropertyChange(nameof(SteamId));
            if (!_isDataLoadingFromSteamId && _steamIdOfLastLoadedData != SteamId && !_isSteamIdChangedByUser)
            {
                StartDataAcquisition();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            SaveToConfigFile();
        }


        public void ImportImageFile()
        {
            var ofd = new OpenFileDialog
            {
                Filter = $"{Resources.Global_Browse_ImageFiles} (*.jpg,*.png,*.bmp)|*.jpg;*.png;*.bmp;",
                InitialDirectory = _config.LastImageDirectory,
            };
            if (ofd.ShowDialog() != true)
            {
                return;
            }
            ImagePath = ofd.FileName;
        }

        public void ReacquireSteamData()
        {
            _isSteamIdChangedByUser = false;
            NotifyOfPropertyChange(nameof(CanReacquireSteamData));

            if (!_isDataLoadingFromSteamId)
            {
                StartDataAcquisition();
            }
        }

        public void OnDrop(DragEventArgs e)
        {
            var data = e.Data as DataObject;
            if (data == null || !data.ContainsFileDropList())
                return;

            var files = data.GetFileDropList();

            if (files.Count <= 0)
                return;

            var imageFilePath = files[0];
            ImagePath = imageFilePath;
        }

        protected override void OnRevalidated(bool isValid)
        {
            NotifyOfPropertyChange(nameof(CanGoNext));
        }

        private void StartDataAcquisition()
        {
            if (_isDataLoadingFromSteamId)
            {
                throw new InvalidOperationException("Data acq is already running. Cannot start again until finished or canceled.");
            }

            Title = null;
            Description = null;
            ImagePath = null;

            // surpress validation errors while acquiring data:
            UpdateValidationError(nameof(Title), null);
            NotifyOfPropertyChange(nameof(Title));
            UpdateValidationError(nameof(Description), null);
            NotifyOfPropertyChange(nameof(Description));
            UpdateValidationError(nameof(ImagePath), null);
            NotifyOfPropertyChange(nameof(ImagePath));

            _isDataLoadingFromSteamId = true;
            _steamIdOfLastLoadedData = SteamId;
            NotifyOfPropertyChange(nameof(IsDataLoadingFromSteamId));
            NotifyOfPropertyChange(nameof(CanReacquireSteamData));

            var config = _configRepository.Get();

            var codesToTry = new List<string>();
            if (config.UseSelectedLanguageCountrycodeFirst && config.LanguageCountrycodeMap.ContainsKey(config.Language))
            {
                codesToTry.Add(config.LanguageCountrycodeMap[config.Language]);
            }
            codesToTry.AddRange(config.CountrycodesToUse);
            codesToTry = codesToTry.Distinct().ToList();

            _steamAcq = SteamApiDataAcquisition.Acquire(SteamId, config.Language, codesToTry);
            _steamAcqBusy = new BusyCancelableViewModel();
            _isSteamAcqCanceled = false;
            _steamAcqBusy.WhenCancelRequested.Subscribe(q => OnSteamAcqCancelRequested());
            _whenBusyRequestedSubject.OnNext(_steamAcqBusy);
            _dataAcqEndedSubscription = _steamAcq.WhenEnded.ObserveOnDispatcher().Subscribe(q => OnDataAcquisitionEnded(_steamAcq));
        }

        private void OnDataAcquisitionEnded(SteamApiDataAcquisition steamAcq)
        {
            _dataAcqEndedSubscription.Dispose();

            if (steamAcq.Exception != null)
            {
                Logger.Warn(steamAcq.Exception, $"SteamAPI data acquisition raised an exception ({steamAcq.Exception}).");
            }

            if (!_isSteamAcqCanceled)
            {
                Title = steamAcq.Title;
               // Description = steamAcq.Description;
                ImagePath = steamAcq.ImagePath;
            }
            else
            {
                Title = null;
                Description = null;
                ImagePath = null;
            }

            NotifyAcqFinished();
        }

        private void ValidateTitle()
        {
            if (string.IsNullOrEmpty(Title))
            {
                UpdateValidationError(nameof(Title), Resources.Global_Validation_CannotBeEmpty);
                return;
            }

            UpdateValidationError(nameof(Title), null);
        }

        private void ValidateDescription()
        {
            UpdateValidationError(nameof(Description), null);
        }

        private void ValidateSelectedCategory()
        {
            if (SelectedCategory== null)
            {
                UpdateValidationError(nameof(SelectedCategory), Resources.Global_Validation_CannotBeEmpty);
                return;
            }
            UpdateValidationError(nameof(SelectedCategory), null);
        }

        private void ValidateImagePath()
        {
            // TODO [RM] Receive the exact size from somewhere else and display it when the size is beyond the limit.
            var maxSize = _config.MaximunAppImageKilobytes.Kilobytes();

            if (string.IsNullOrEmpty(ImagePath))
            {
                UpdateValidationError(nameof(ImagePath), Resources.DetailInfo_InvalidPictureFile);
                return;
            }
            if (!File.Exists(ImagePath))
            {
                UpdateValidationError(nameof(ImagePath), Resources.DetailInfo_InvalidPictureFile);
                return;
            }

            var imageFile = new FileInfo(ImagePath);
            var imageSize = imageFile.Length.Bytes();
            if (imageSize > maxSize)
            {
                UpdateValidationError(nameof(ImagePath), string.Format(Resources.DetailInfo_BeyondMaximumFileSizeFormat, maxSize.ToString("KB")));
                return;
            }
            UpdateValidationError(nameof(ImagePath), null);

            //var bitmapImage = (BitmapImage)Image;

            //if (bitmapImage.PixelWidth > maxSize || bitmapImage.PixelHeight > maxSize)
            //{
            //    UpdateValidationError(nameof(ImagePath), string.Format(Resources.DetailInfo_InvalidPictureDimensions, maxSize));
            //    return;
            //}

            //UpdateValidationError(nameof(ImagePath), null);
        }


        private void PopulateCategories()
        {
            var categories = GetCategories();
            var catViewModels = new List<CategoryViewModel>();
            foreach(var category in categories)
            {
                catViewModels.Add(new CategoryViewModel(category.icon,category.key));
            }
            Categories.AddRange(catViewModels.OrderBy(x=> x.Translation));
        }

        private List<(string key, ImageSource icon)> GetCategories()
        {
            var retval = new List<(string key, ImageSource icon)>();
            var allcategoryKeys = Application.Current.Resources.MergedDictionaries[3].Keys;
            foreach(object allcategoryKey in allcategoryKeys)
            {
                if(allcategoryKey == null)continue;
                var key = allcategoryKey.ToString();
                var icon = Application.Current.Resources[key] as ImageSource;
                retval.Add(new ValueTuple<string, ImageSource>(key,icon));
            }
            return retval;
        }

        private void SaveToConfigFile()
        {
            _config.LastApplicationTitle = Title;
            _config.LastCategory = SelectedCategory.ResourceKey;
            _config.LastImageDirectory = Path.GetDirectoryName(ImagePath);
            _configRepository.Store(_config);
        }

        private void LoadImageFromImagePath(string imageFilePath)
        {
            try
            {
                if (imageFilePath == null)
                {
                    Image = null;
                    return;
                }

                Image = UIHelper.FilePathToImageSource(imageFilePath);
            }
            catch (Exception e)
            {
                Image = null;
                MessageBox.Show(e.ToString(), "ImportImageFile(): EXCEPTION"); // TODO [RM]: temp only
            }
        }

        private void OnSteamAcqCancelRequested()
        {
            _isSteamAcqCanceled = true;
            NotifyAcqFinished();

            Task.Run(() =>
            {
                _steamAcq?.Cancel();
            });
        }

        private void NotifyAcqFinished()
        {
            _isDataLoadingFromSteamId = false;
            NotifyOfPropertyChange(nameof(IsDataLoadingFromSteamId));
            NotifyOfPropertyChange(nameof(CanReacquireSteamData));
            _steamAcqBusy.NotifyOperationEnded();
        }
        #endregion
    }
}
