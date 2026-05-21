#region Licence
/****************************************************************
 *  Filename: GuardViewsThemingConfig.cs
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
using LeapVR.Shell.FileConfig.Models;

namespace LeapVR.Shell.FileConfig
{
    
    public class GuardViewsThemingConfig : AreaThemingConfig
    {
        public ButtonConfig GuardViewsButtonConfig { get; set; }
        public ButtonConfig SettingLikeButtonConfig { get; set; }
        public double? OverlayBackgroundWidth { get; set; } = null;
        public double? OverlayBackgroundHeight { get; set; } = null;

        public GuardViewsThemingConfig()
        {
            GuardViewsButtonConfig = new ButtonConfig
            {
                ButtonFontSize = 48,
                ButtonWidth = 400,
                ButtonHeight = 100,
                BorderRadius = new System.Windows.CornerRadius(15),
                BorderThickness = new System.Windows.Thickness(5)
            };

            SettingLikeButtonConfig = new ButtonConfig
            {
                ButtonFontSize = 36,
                ButtonWidth = 80,
                ButtonHeight = 80,
                BorderRadius = new System.Windows.CornerRadius(0,15,0,15),
                BorderThickness = new System.Windows.Thickness(2)
            };
        }
    }
}
