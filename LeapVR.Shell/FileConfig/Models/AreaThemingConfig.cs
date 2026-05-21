#region Licence
/****************************************************************
 *  Filename: AreaThemingConfig.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-8-30
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Reflection;
using System.Windows.Media;

namespace LeapVR.Shell.FileConfig.Models
{
    
    public class AreaThemingConfig
    {
        public double H1 { get; set; } = 72;
        public double H2 { get; set; } = 60;
        public double H3 { get; set; } = 48;
        public double H4 { get; set; } = 36;
        public double H5 { get; set; } = 28;
        public double H6 { get; set; } = 20;

        public SolidColorBrush PrimaryForegroundColor { get; set; } = new SolidColorBrush(Colors.White);
        public SolidColorBrush PrimaryBackgroundColor { get; set; } = new SolidColorBrush(Colors.Transparent);
        public SolidColorBrush PrimaryBorderColor { get; set; } = new SolidColorBrush(Colors.White);
        public SolidColorBrush HoverForegroundColor { get; set; } = new SolidColorBrush(Colors.Black);
        public SolidColorBrush HoverBackgroundColor { get; set; } = new SolidColorBrush(Colors.White);
        public SolidColorBrush HoverBorderColor { get; set; } = new SolidColorBrush(Colors.Black);
        public SolidColorBrush PressedForegroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        public SolidColorBrush PressedBackgroundColor { get; set; } = new SolidColorBrush(Colors.Gray);
        public SolidColorBrush PressedBorderColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
        public SolidColorBrush DisabledForegroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF999999"));
        public SolidColorBrush DisabledBackgroundColor { get; set; } = new SolidColorBrush(Colors.Transparent);
        public SolidColorBrush DisabledBorderColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF999999"));
        public SolidColorBrush ActiveForegroundColor { get; set; } = new SolidColorBrush(Colors.White);
        public SolidColorBrush ActiveBackgroundColor { get; set; } = new SolidColorBrush(Colors.Black);
        public SolidColorBrush ActiveBorderColor { get; set; } = new SolidColorBrush(Colors.Black);
    }
}
