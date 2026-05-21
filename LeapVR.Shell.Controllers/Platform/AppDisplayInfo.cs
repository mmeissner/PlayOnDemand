#region Licence
/****************************************************************
 *  Filename: AppDisplayInfo.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Managers.Annotations;

namespace LeapVR.Shell.Controllers.Platform
{
    internal class AppDisplayInfo : IAppDisplayInfo
    {
        protected readonly PlatformController PlatformController;

        #region Fields & Properties
        public Guid ApplicationGuid { get; }
        public string Name { get; internal set; }
        public IAppCategory Category { get; internal set; }

        public string[] Tags { get; internal set; }
        public string Description { get; internal set; }
        public byte[] Thumbnail { get; internal set; }
        public bool IsSupportScreen => PlatformController!= null && PlatformController.ShowScreenSupport(ApplicationGuid);
        public bool IsSupportVirtualReality => PlatformController!= null && PlatformController.ShowVRSupport(ApplicationGuid);
        #endregion

        #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDisplayInfo"/> class.
        /// Use for Uninstallation only!
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        public AppDisplayInfo(Guid applicationGuid)
        {
            ApplicationGuid = applicationGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDisplayInfo"/> class.
        /// Use for NOT Installed Applications
        /// </summary>
        /// <param name="applicationGuid">The application unique identifier.</param>
        /// <param name="platformController">The platform controller.</param>
        protected AppDisplayInfo(Guid applicationGuid, PlatformController platformController) 
        {
            ApplicationGuid = applicationGuid;
            PlatformController = platformController;
            PlatformController.WhenAppDisplayUpdate.
                                Where(x => x.ApplicationId.Equals(ApplicationGuid)).
                                Subscribe(OnAppDisplayInfoUpdated);
            PlatformController.WhenAppExecutablesUpdate.
                               Where(x => x.ApplicationId.Equals(ApplicationGuid)).
                               Subscribe(OnAppExecutablesUpdated);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDisplayInfo"/> class.
        /// Use for Installed Applications
        /// </summary>
        /// <param name="displayData">The display data.</param>
        /// <param name="categoryProvider">The category provider.</param>
        /// <param name="thumbnail">The thumbnail.</param>
        /// <param name="platformController">The platform controller.</param>
        protected AppDisplayInfo(IAppDisplayData displayData,ICategoryProvider categoryProvider,byte[] thumbnail, PlatformController platformController)
        {
            ApplicationGuid = displayData.ApplicationGuid;
            Name = displayData.Name;
            Tags = displayData.Tags;
            Description = displayData.Description;
            Category = categoryProvider.GetOrCreateAppCategory(displayData.Category);
            Thumbnail = thumbnail;
            PlatformController = platformController;
            PlatformController.WhenAppDisplayUpdate.
                               Where(x => x.ApplicationId.Equals(ApplicationGuid)).
                               Subscribe(OnAppDisplayInfoUpdated);
            PlatformController.WhenAppExecutablesUpdate.
                               Where(x => x.ApplicationId.Equals(ApplicationGuid)).
                               Subscribe(OnAppExecutablesUpdated);
        }

        private void OnAppDisplayInfoUpdated(AppDisplayUpdate obj)
        {
            if(obj.CategoryChanged)
            {
                Category = obj.Category;
                OnPropertyChanged(nameof(Category));
            }

            if(obj.DescriptionChanged)
            {
                Description = obj.Description;
                OnPropertyChanged(nameof(Description));
            }

            if(obj.NameChanged)
            {
                Name = obj.Name;
                OnPropertyChanged(nameof(Name));
            }
            PlatformController.PublishAppInfoChanged(this,obj);
        }

        private void OnAppExecutablesUpdated(AppExecutablesUpdate obj)
        {
            OnPropertyChanged(nameof(IsSupportScreen));
            OnPropertyChanged(nameof(IsSupportVirtualReality));
        }

        public IAppDisplayUpdate GetAppDisplayUpdate()
        {
            if(PlatformController == null) return null;
            return new AppDisplayUpdate(this, PlatformController);
        }


        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
