#region Licence
/****************************************************************
 *  Filename: PlatformAppLoadingErrorToVisibilityConverter.cs
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
using System.Windows;
using System.Windows.Data;

namespace LeapVR.Shared.Lib.Wpf.Converters {
    public class PlatformAppLoadingErrorToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isLoaded = (bool)values[0];
            bool isError = (bool)values[1];

            if(!isLoaded && isError) return Visibility.Collapsed;
            if(!isLoaded) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MenueItemAndBusyToVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //We expect two values 0: IsActive 1: IsBusy
            bool isActive = (bool)values[0];
            bool isBusy = (bool)values[1];
            
            //And one parameter Selected Image
            bool.TryParse(parameter.ToString(), out var isSelectedImage);

            if(!isBusy)
            {
                //Default Case
                if(isActive && isSelectedImage) return Visibility.Visible;
                if(!isActive && !isSelectedImage) return Visibility.Visible;
                return Visibility.Hidden;
            }
            //Its Busy, we want only to show the Default if its Busy
            if(isSelectedImage) return Visibility.Visible;
            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}