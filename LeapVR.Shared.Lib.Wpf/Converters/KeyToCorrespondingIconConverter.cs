#region Licence
/****************************************************************
 *  Filename: KeyToCorrespondingIconConverter.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LeapVR.Shared.Lib.Wpf.Converters
{
    /// <summary>
    /// Represents a converter that takes in a resources key <see cref="string"/> value and return the corresponding resource.
    /// </summary>
    [ValueConversion(sourceType: typeof(string), targetType: typeof(object))]
    public class KeyToCorrespondingIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var keyStr = value.ToString();
            return string.IsNullOrEmpty(keyStr) ? null : Application.Current.Resources[keyStr];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
