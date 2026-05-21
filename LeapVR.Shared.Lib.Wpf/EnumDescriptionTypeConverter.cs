#region Licence
/****************************************************************
 *  Filename: EnumDescriptionTypeConverter.cs
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
using System.ComponentModel;

namespace LeapVR.Shared.Lib.Wpf
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type)
            : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string)) return base.ConvertTo(context, culture, value, destinationType);

            if (value == null) return string.Empty;

            var fi = value.GetType().GetField(value.ToString());

            if (fi == null) return string.Empty;

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].Description) ? attributes[0].Description : value.ToString();
        }
    }
}
