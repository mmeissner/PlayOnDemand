#region Licence
/****************************************************************
 *  Filename: BoolToVisibilityConverter.cs
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
    /// Represents a converter that converts a <see cref="bool"/> value to a <see cref="Visibility"/>.
    /// When parameter is given to true, the whole behavior will be the other way around.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility), ParameterType = typeof(bool))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // return Visible whenever value can't be converted to bool.
            if (!bool.TryParse(value?.ToString(), out var boolInuput))
            {
                return Visibility.Visible;
            }

            bool.TryParse(parameter?.ToString(), out var isInverted);

            if (!isInverted)
            {
                return boolInuput ? Visibility.Visible : Visibility.Collapsed;
            }
            return boolInuput ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
