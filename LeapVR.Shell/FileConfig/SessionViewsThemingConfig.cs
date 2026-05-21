#region Licence
/****************************************************************
 *  Filename: SessionViewsThemingConfig.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-8-31
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
using LeapVR.Shell.FileConfig.Models;

namespace LeapVR.Shell.FileConfig
{
    
    public class SessionViewsThemingConfig : AreaThemingConfig
    {
        public ButtonConfig SessionViewsButtonConfig { get; set; }
        public ButtonConfig TabButtonConfig { get; set; }

        public SessionViewsThemingConfig()
        {
            SessionViewsButtonConfig = new ButtonConfig
            {
                ButtonFontSize = 48,
                ButtonWidth = 300,
                ButtonHeight = 100,
                BorderRadius = new System.Windows.CornerRadius(15),
                BorderThickness = new System.Windows.Thickness(5)
            };

            TabButtonConfig = new ButtonConfig
            {
                ButtonFontSize = 36,
                ButtonWidth = 330,
                ButtonHeight = 100,
                BorderRadius = new System.Windows.CornerRadius(0),
                BorderThickness = new System.Windows.Thickness(0)
            };
        }
    }
}
