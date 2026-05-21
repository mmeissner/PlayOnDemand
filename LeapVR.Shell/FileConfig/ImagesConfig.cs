#region Licence
/****************************************************************
 *  Filename: ImagesConfig.cs
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
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LeapVR.Shell.FileConfig
{
    
    public class ImagesConfig
    {
        private const string ImageSourcePath = "resources/images/package.xaml";
        public Dictionary<string, string> ConfigurableImagesDictionary { get; set; } = new Dictionary<string, string>();

        public ImagesConfig()
        {
            //Try to find merged image resource dictionary
            ResourceDictionary imageDictionary = null;
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary?.Source?.OriginalString != null &&
                        dictionary.Source.OriginalString.StartsWith(ImageSourcePath, true, CultureInfo.InvariantCulture))
                {
                    imageDictionary = dictionary;
                }
            }
            if(imageDictionary == null)
            {
                throw new NullReferenceException($"Could not locate Resource Dictionary with images! Check if the source path '{ImageSourcePath}' is still valid!");
            }
            foreach (string resourceKey in imageDictionary.Keys)
            {
                if (imageDictionary[resourceKey] is BitmapImage imageSource)
                {
                    ConfigurableImagesDictionary.Add(resourceKey, imageSource.UriSource.ToString());
                }
            }
        }

    }
}
