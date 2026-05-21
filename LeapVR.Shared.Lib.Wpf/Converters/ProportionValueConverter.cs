#region Licence
/****************************************************************
 *  Filename: ProportionValueConverter.cs
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
using System.Windows.Data;

namespace LeapVR.Shared.Lib.Wpf.Converters
{
    /// <summary>
    /// Represents a converter that takes in a <see cref="double"/> value and a proportion <see cref="double"/> value to get a processed value.
    /// Parameter is the proportion value.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class ProportionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var inputValueStr = value.ToString();
            double outputValue = 0;

            if (!double.TryParse(inputValueStr, out var inputValue)) return value;

            if (parameter != null && double.TryParse(parameter.ToString(), out var proportion))
            {
                outputValue = inputValue * proportion;
            }
            return outputValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
