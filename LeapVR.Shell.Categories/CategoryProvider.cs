#region Licence
/****************************************************************
 *  Filename: CategoryProvider.cs
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Categories.Annotations;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using NLog;
using LogManager = NLog.LogManager;

namespace LeapVR.Shell.Categories
{
    public class CategoryProvider : ICategoryProvider
    {
        private const string DefaultCategoryKey = "IconCategoryDefault";
        private const int CategoryResourcesImagesIndex = 4;
        private const string CategoryResourcesDictIdentifier = "/Images/Categories/Categories.xaml";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _customCategoryIconFolder;
        private readonly ConcurrentDictionary<string,AppCategory> _appCategories = new ConcurrentDictionary<string, AppCategory>();
        private readonly IUIMessageBroker _messageBroker;
        public CategoryProvider(IUIMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
            _customCategoryIconFolder = Path.Combine(
                    GlobalConfig.GetGlobalConfiguration().PersistentDirectory,
                    "Images",
                    "Categories");
            LoadDefaultCategories(_messageBroker);
        }


        public IEnumerable<IAppCategory> GetAllCategories => _appCategories.Values;
        public IAppCategory GetOrCreateAppCategory(string identifier)
        {
            if(_appCategories.TryGetValue(identifier, out var retval)) return retval;
            var foundIcon = RegisterResourceByKey(identifier, out var isDictionaryResource);
            ImageSource icon;
            if(foundIcon) icon = Application.Current.Resources[identifier] as ImageSource;
            else icon = Application.Current.Resources[DefaultCategoryKey] as ImageSource;
            _appCategories.AddOrUpdate(identifier,new AppCategory(_messageBroker, identifier, isDictionaryResource,icon),
                    (s, category) => category);
            return _appCategories[identifier];
        } 

        private void LoadDefaultCategories(IUIMessageBroker messageBroker)
        {
            foreach(DictionaryEntry dictionaryEntry in Application.Current.Resources.MergedDictionaries[CategoryResourcesImagesIndex])
            {
                if(RegisterResourceByKey(dictionaryEntry.Key.ToString(), out var isDictionaryResource))
                {
                    _appCategories.TryAdd(
                            dictionaryEntry.Key.ToString(),
                            new AppCategory(
                                    messageBroker,
                                    dictionaryEntry.Key.ToString(),
                                    isDictionaryResource,Application.Current.Resources[dictionaryEntry.Key.ToString()] as ImageSource));
                }
            }
        }

        private bool RegisterResourceByKey(string resourceKey, out bool isDictionaryResource)
        {
            isDictionaryResource = false;
            bool hasCategoryDictionary = false;
            
            foreach(ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if(dictionary?.Source?.OriginalString != null &&
                   dictionary.Source.OriginalString.EndsWith(
                           CategoryResourcesDictIdentifier,
                           StringComparison.InvariantCultureIgnoreCase))
                {
                    hasCategoryDictionary = true;
                    break;
                }
            }
            if (!hasCategoryDictionary)
            {
                Logger.Error($"Resource Dictionary with Categories could not be found, check if you included dictionary with a path like: {CategoryResourcesDictIdentifier}");
                return false;
            }

            if (string.IsNullOrEmpty(resourceKey))
            {
                Logger.Warn("Resource Key can not be null or empty!");
                return false;
            }

            //Path were an override icon or a custom icon image can be found
            var iconPath = Path.Combine(_customCategoryIconFolder, $"{resourceKey.ToLower()}.png");
            isDictionaryResource = Application.Current.Resources[resourceKey] != null;
            bool foundResourceAsFile = false;

            if (isDictionaryResource)
            {
                Logger.Debug($"Found Resource Key={resourceKey} in dictionary, it should be translateable!");
            }

            //Overwrite existing icon images or add custom ones
            if(File.Exists(iconPath))
            {
                foundResourceAsFile = true;
                Logger.Debug($"Found Resource with Key={resourceKey} in directory {iconPath}");
                Application.Current.Resources.Add(resourceKey, UIHelper.FilePathToImageSource(iconPath));
            }

            //Resource can not be allocated
            if(!foundResourceAsFile && !isDictionaryResource)
            {
                Logger.Warn($"Could not find Resource with Key={resourceKey} in any directory or resources");
                return false;
            }
            return true;
        }
    }

    internal class AppCategory : IHandle<IUILanguageChangedEvent>, INotifyPropertyChanged, IDisposable, IAppCategory
    {
        private readonly bool _hasTranslation;
        private readonly IUIMessageBroker _messageBroker;
        private readonly string _translationKey;

        private string _displayName;
        public string DisplayName   
        {
            get => _displayName;
            internal set
            {
                if(value == _displayName) return;
                _displayName = value;
                OnPropertyChanged();
            }
        }
        public string Identifier { get; }
        public ImageSource Icon { get; }

        internal AppCategory(IUIMessageBroker messageBroker, string identifier, bool hasTranslation,ImageSource icon)
        {
            _hasTranslation = hasTranslation;
            Icon = icon;
            Identifier = identifier;
            if(_hasTranslation)
            {
                _messageBroker = messageBroker;
                _messageBroker.Subscribe(this);
                _translationKey = $"Category_{identifier}";
                DisplayName = Resource.ResourceManager.GetString(_translationKey);
            }
            else
            {
                DisplayName = identifier;
            }
        }
        public void Handle(IUILanguageChangedEvent message)
        {
            DisplayName = Resource.ResourceManager.GetString(_translationKey);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Dispose()
        {
            if(_hasTranslation)_messageBroker.Unsubscribe(this);
        }
    }
}
