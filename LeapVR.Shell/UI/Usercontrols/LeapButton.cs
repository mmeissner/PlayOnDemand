#region Licence
/****************************************************************
 *  Filename: LeapButton.cs
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LeapVR.Shell.UI.Usercontrols
{
    /// <summary>
    /// Extented button control with its <see cref="BorderRadius"/> adjustable.
    /// </summary>
    public class LeapButton : Button
    {
        static LeapButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LeapButton), new FrameworkPropertyMetadata(typeof(LeapButton)));
        }
        public CornerRadius BorderRadius
        {
            get { return (CornerRadius)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }
        // Using a DependencyProperty as the backing store for BorderRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(CornerRadius), typeof(LeapButton));

        public Brush HoverForeground
        {
            get { return (Brush)GetValue(HoverForegroundProperty); }
            set { SetValue(HoverForegroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for HoverForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverForegroundProperty =
            DependencyProperty.Register("HoverForeground", typeof(Brush), typeof(LeapButton));

        public Brush HoverBackground
        {
            get { return (Brush)GetValue(HoverBackgroundProperty); }
            set { SetValue(HoverBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for HoverBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(LeapButton));

        public Brush HoverBorderBrush
        {
            get { return (Brush)GetValue(HoverBorderBrushProperty); }
            set { SetValue(HoverBorderBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for HoverBorderColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HoverBorderBrushProperty =
            DependencyProperty.Register("HoverBorderBrush", typeof(Brush), typeof(LeapButton));

        public Brush PressedForeground
        {
            get { return (Brush)GetValue(PressedForegroundProperty); }
            set { SetValue(PressedForegroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for PressedForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedForegroundProperty =
            DependencyProperty.Register("PressedForeground", typeof(Brush), typeof(LeapButton));

        public Brush PressedBackground
        {
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for PressedBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register("PressedBackground", typeof(Brush), typeof(LeapButton));

        public Brush PressedBorderBrush
        {
            get { return (Brush)GetValue(PressedBorderBrushProperty); }
            set { SetValue(PressedBorderBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for PressedBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedBorderBrushProperty =
            DependencyProperty.Register("PressedBorderBrush", typeof(Brush), typeof(LeapButton));

        public Brush DisabledForeground
        {
            get { return (Brush)GetValue(DisabledForegroundProperty); }
            set { SetValue(DisabledForegroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DisabledForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisabledForegroundProperty =
            DependencyProperty.Register("DisabledForeground", typeof(Brush), typeof(LeapButton));

        public Brush DisabledBackground
        {
            get { return (Brush)GetValue(DisabledBackgroundProperty); }
            set { SetValue(DisabledBackgroundProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DisabledBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.Register("DisabledBackground", typeof(Brush), typeof(LeapButton));


        public Brush DisabledBorderBrush
        {
            get { return (Brush)GetValue(DisabledBorderBrushProperty); }
            set { SetValue(DisabledBorderBrushProperty, value); }
        }
        // Using a DependencyProperty as the backing store for DisabledBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisabledBorderBrushProperty =
            DependencyProperty.Register("DisabledBorderBrush", typeof(Brush), typeof(LeapButton));

    }
}
