#region Licence
/****************************************************************
 *  Filename: PlatformProvider.cs
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.UI.Universal.Platform.ViewModels;
using NLog;

namespace LeapVR.Shell.UI.Core
{
    public class PlatformProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<Guid, PlatformDisplayData> _platformDisplayInfoDic;
        public PlatformProvider(IPlatformController platformController)
        {
            var customPlatformIconFolder = Path.Combine(
                    GlobalConfig.GetGlobalConfiguration().PersistentDirectory,
                    "Images",
                    "Platforms");
            _platformDisplayInfoDic = GetPlatformDisplayData(platformController, customPlatformIconFolder);
        }

        internal IReadOnlyDictionary<Guid, PlatformDisplayData> PlatformDisplayDataDic => _platformDisplayInfoDic;

        internal IEnumerable<PlatformViewModel> GetPlatformViewModels()
        {
            return from platform in _platformDisplayInfoDic.Values
                   select new PlatformViewModel(platform.Platform, platform.Image);
        }

        internal bool TryGetPlatformViewModel(Guid platformId, out PlatformViewModel platformViewModel)
        {
            if(_platformDisplayInfoDic.ContainsKey(platformId))
            {
                platformViewModel = new PlatformViewModel(
                        _platformDisplayInfoDic[platformId].Platform,
                        _platformDisplayInfoDic[platformId].Image);
                return true;
            }

            platformViewModel = null;
            return false;
        }

        private Dictionary<Guid, PlatformDisplayData> GetPlatformDisplayData(
                IPlatformController platformController, string customPlatformIconFolder)
        {
            var retval = new Dictionary<Guid, PlatformDisplayData>();
            foreach(IPlatform platform in platformController.GetPlatforms())
            {
                var identifier = platform.PlatformGuid.ToString().ToUpperInvariant();
                var resourceExsiting = Application.Current.Resources[identifier];
                ImageSource icon = null;
                if(resourceExsiting != null)
                {
                    icon = Application.Current.Resources[identifier] as ImageSource;
                    Logger.Debug($"Found Resource Key={identifier} in dictionary!");
                }
                else
                {
                    var iconPath = Path.Combine(customPlatformIconFolder, $"{identifier}.png");
                    if(!File.Exists(iconPath))
                    {
                        Logger.Warn($"Could not find Resource with Key={identifier} in any directory or resources");
                    }
                    else
                    {
                        Logger.Debug($"Found Resource with Key={identifier} in directory {iconPath}");
                        icon = UIHelper.FilePathToImageSource(iconPath);
                    }
                }

                retval.Add(platform.PlatformGuid, new PlatformDisplayData(platform, icon));
            }

            return retval;
        }

        public struct PlatformDisplayData
        {
            public IPlatform Platform;
            public ImageSource Image;
            public PlatformDisplayData(IPlatform platform, ImageSource image)
            {
                Platform = platform;
                Image = image;
            }
        }
    }
}