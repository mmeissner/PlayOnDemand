#region Licence
/****************************************************************
 *  Filename: ButtonConfig.cs
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
using System.Windows;

namespace LeapVR.Shell.FileConfig.Models
{
    
    public class ButtonConfig
    {
        public double ButtonFontSize { get; set; }
        public double ButtonWidth { get; set; }
        public double ButtonHeight { get; set; }
        public CornerRadius BorderRadius { get; set; }
        public Thickness BorderThickness { get; set; }
    }
}
