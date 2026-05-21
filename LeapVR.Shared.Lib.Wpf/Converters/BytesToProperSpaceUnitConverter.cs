#region Licence
/****************************************************************
 *  Filename: BytesToProperSpaceUnitConverter.cs
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
using LeapVR.Shared.Lib.Helper;

namespace LeapVR.Shared.Lib.Wpf.Converters
{
    [ValueConversion(typeof(ulong), typeof(string))]
    public class BytesToProperSpaceUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ulong num;
            if (value != null && ulong.TryParse(value.ToString(), out num))
            {
                return QuickLeap.ToDiskSize(num);
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
