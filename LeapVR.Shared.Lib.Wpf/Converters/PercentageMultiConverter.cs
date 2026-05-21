#region Licence
/****************************************************************
 *  Filename: PercentageMultiConverter.cs
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
    /// Represents a converter that takes in a portion value and a total value and return percentage.
    /// Parameter indicates how many digits to reserve.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class PercentageMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return null;
            }
            var portionValueStr = values[0].ToString();
            var totalValueStr = values[1].ToString();


            if (double.TryParse(portionValueStr, out var portionValue) && double.TryParse(totalValueStr, out var totalValue))
            {
                if (parameter == null || !int.TryParse(parameter.ToString(), out var digits))
                {
                    return Math.Round((portionValue / totalValue) * 100, 1).ToString(CultureInfo.InvariantCulture);
                }
                return Math.Round((portionValue / totalValue) * 100, digits).ToString(CultureInfo.InvariantCulture);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
