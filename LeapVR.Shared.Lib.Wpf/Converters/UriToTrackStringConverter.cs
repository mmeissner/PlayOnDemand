#region Licence
/****************************************************************
 *  Filename: UriToTrackStringConverter.cs
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
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace LeapVR.Shared.Lib.Wpf.Converters {
    public class UriToTrackStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if(value == null) return "";
                if(value is Uri uri)
                {
                    if(!String.IsNullOrWhiteSpace(uri.LocalPath))
                    {
                        var fileInfo = new FileInfo(uri.LocalPath);
                        if(fileInfo.Exists) return fileInfo.Name;
                        return $"Not Found: {fileInfo.FullName}";
                    }
                }
                return "Unknown Format";
            }
            catch(Exception e)
            {
                return $"Error: {e.Message}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}