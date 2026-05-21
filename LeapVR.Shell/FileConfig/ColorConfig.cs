#region Licence
/****************************************************************
 *  Filename: ColorConfig.cs
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
using System.Reflection;
using System.Windows.Media;

namespace LeapVR.Shell.FileConfig
{
    
    public class ColorConfig
    {
        public SolidColorBrush ThemeMainColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCDCDCD"));
        public SolidColorBrush ThemeHighlightBackgroundColor { get; set; } = new SolidColorBrush(Colors.White);
        public SolidColorBrush ThemeHoverBackgroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#190077CC"));
        public SolidColorBrush ThemeSelectedBackgroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#660077CC"));
        public SolidColorBrush ThemeMainTextColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
        public SolidColorBrush ThemeSecondaryTextColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCDCDCD"));
        public SolidColorBrush ThemeDisabledTextColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF999999"));
        public SolidColorBrush ThemeBorderColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
        public SolidColorBrush ThemeMainTextReverseColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
        public SolidColorBrush ThemeItemActiveBackgroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Orange"));
        public SolidColorBrush ThemeItemInactiveBackgroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
        public SolidColorBrush LoginViewPrimaryTipTextForegroundColor { get; set; } = new SolidColorBrush(Colors.Black);
        public SolidColorBrush LoginViewPrimaryTipTextBackgroundColor { get; set; } = new SolidColorBrush(Colors.Transparent);
        public SolidColorBrush InstallViewsPrimaryPercentageTextForegroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#83A262"));
    }
}
