#region Licence
/****************************************************************
 *  Filename: ImageConverter.cs
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
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using LeapVR.Shared.Lib.Wpf.UIHelpers;

namespace LeapVR.Shared.Lib.Wpf.Converters
{
    [ValueConversion(typeof(object), typeof(BitmapImage))]
    [ValueConversion(typeof(string), typeof(BitmapImage))]
    [ValueConversion(typeof(byte[]), typeof(BitmapImage))]
    [ValueConversion(typeof(BitmapImage), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var defaultImage = parameter as BitmapImage;

            // value is a path of a image file.
            if (value is string path)
            {
                try
                {
                    //ABSOLUTE
                    if (path.Length > 0 && path[0] == Path.DirectorySeparatorChar ||
                        path.Length > 1 && path[1] == Path.VolumeSeparatorChar)
                        return new BitmapImage(new Uri(path));
                    //RELATIVE
                    return
                        new BitmapImage(
                            new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar +
                                    path));
                }
                catch (Exception)
                {
                    return defaultImage;
                }
            }

            // value is a byte array of a image file.
            if (value is byte[] buffer)
            {
                return UIHelper.BytesToImageSource(buffer);
            }

            // value is BitmapImage
            if (value is BitmapImage)
            {
                return value;
            }

            return defaultImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
