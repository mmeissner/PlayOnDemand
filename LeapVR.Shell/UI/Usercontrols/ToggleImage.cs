#region Licence
/****************************************************************
 *  Filename: ToggleImage.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace LeapVR.Shell.UI.Usercontrols
{
    public class ToggleImage : ToggleButton
    {
        public static DependencyProperty OnImageProperty =
                DependencyProperty.Register("OnImage",
                        typeof(BitmapImage),
                        typeof(ToggleImage),
                        new PropertyMetadata());

        public static readonly DependencyProperty OffImageProperty =
                DependencyProperty.Register("OffImage",
                        typeof(BitmapImage),
                        typeof(ToggleImage),
                        new PropertyMetadata());

        public ToggleImage()
        {
            IsEnabled = false;
        }
        public BitmapImage OnImage
        {
            get { return (BitmapImage)this.GetValue(OnImageProperty); }
            set { this.SetValue(OnImageProperty, (BitmapImage)value); }
        }

        public BitmapImage OffImage
        {
            get { return (BitmapImage)this.GetValue(OffImageProperty); }
            set { this.SetValue(OffImageProperty, (BitmapImage)value); }
        }
    }
}
