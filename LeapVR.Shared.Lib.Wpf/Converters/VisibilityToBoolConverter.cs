#region Licence
/****************************************************************
 *  Filename: VisibilityToBoolConverter.cs
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
    /// Represents a converter that converts a <see cref="Visibility"/> value to a <see cref="bool"/>.
    /// When parameter is given to true, the whole behavior will be the other way around.
    /// </summary>
    [ValueConversion(typeof(Visibility), typeof(bool), ParameterType = typeof(bool))]
    public class VisibilityToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            var valueStr = value.ToString();
            if (!Enum.TryParse(valueStr, out Visibility visibility))
            {
                return false;
            }

            bool.TryParse(parameter?.ToString(), out var isInverted);

            if (isInverted)
            {
                return visibility != Visibility.Visible;
            }
            return visibility == Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
