#region Licence
/****************************************************************
 *  Filename: UiConfig.cs
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
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Customization;

namespace LeapVR.Shell.FileConfig
{
    
    public class UiConfig : ConfigObject
    {
        public UiConfig()
        {
            Color = new ColorConfig();
            Path = new PathConfig();
            Font = new FontConfig();
            Images = new ImagesConfig();

            GuardViewsThemingConfig = new GuardViewsThemingConfig();
            SessionViewsThemingConfig = new SessionViewsThemingConfig();
        }

        public ColorConfig Color { get; set; }
        public PathConfig Path { get; set; }
        public FontConfig Font { get; set; }
        public ImagesConfig Images { get; set; }
        public GuardViewsThemingConfig GuardViewsThemingConfig { get; set; }
        public SessionViewsThemingConfig SessionViewsThemingConfig { get; set; }
        public double? QrCodeWidth { get; set; } = 400;
        public double LoginButtonFontSize { get; set; } = 48;
        public double LoginButtonIconSize { get; set; } = 60;
        public Thickness StationNamePosition { get; set; } = new Thickness(25, 895, 0, 0);
        public Thickness SelectorItemMargin { get; set; } = new Thickness(0, 5, 0, 5);
        public SolidColorBrush SelectorItemHoverBackgroundColor { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));

        /// <summary>
        /// Get or set time for count-down alarm text appear before auto cancel of login intention.
        /// </summary>
        public TimeSpan TimeBeforeDisplayAutoCancelAlarm { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Get or set the time for all auto-close views to stay before closing.
        /// Timing should be in alignment with Animation of Screen moving up as otherwise the screen
        /// will change during the animation or stay for too long time if user quits session imminently 
        /// </summary>
        public int MillisecondsToStayBeforeAutoCloseViewsClosing { get; set; } = 2000;

        /// <summary>
        /// Get or set the indicator whether to show information dialog when session auto logout for different reasons.
        /// </summary>
        public bool AllowDisplayInfoWhenSessionAutoLogout { get; set; } = true;

        /// <summary>
        /// Loads default configurations or their custom overwrites if present
        /// </summary>
        public override void Initialize()
        {
            ConfigRootResources(this);
            ConfigColor(this);
            ConfigFont(this);
            ConfigImages(GlobalConfig.GetGlobalConfiguration().PersistentDirectory, this);
            ConfigGuardViews(this);
            ConfigSessionViews(this);
        }

        private static void ConfigRootResources(UiConfig config)
        {
            Application.Current.Resources["SelectorItemMargin"] = config.SelectorItemMargin;
            Application.Current.Resources["SelectorItemHoverBackgroundColor"] = config.SelectorItemHoverBackgroundColor;
        }

        private static void ConfigColor(UiConfig config)
        {
            Application.Current.Resources["ThemeMainColor"] = config.Color.ThemeMainColor;
            Application.Current.Resources["ThemeHighlightBackgroundColor"] = config.Color.ThemeHighlightBackgroundColor;
            Application.Current.Resources["ThemeHoverBackgroundColor"] = config.Color.ThemeHoverBackgroundColor;
            Application.Current.Resources["ThemeSelectedBackgroundColor"] = config.Color.ThemeSelectedBackgroundColor;
            Application.Current.Resources["ThemeMainTextColor"] = config.Color.ThemeMainTextColor;
            Application.Current.Resources["ThemeSecondaryTextColor"] = config.Color.ThemeSecondaryTextColor;
            Application.Current.Resources["ThemeDisabledTextColor"] = config.Color.ThemeDisabledTextColor;
            Application.Current.Resources["ThemeBorderColor"] = config.Color.ThemeBorderColor;
            Application.Current.Resources["ThemeMainTextReverseColor"] = config.Color.ThemeMainTextReverseColor;
            Application.Current.Resources["ThemeItemActiveBackgroundColor"] = config.Color.ThemeItemActiveBackgroundColor;
            Application.Current.Resources["ThemeItemInactiveBackgroundColor"] = config.Color.ThemeItemInactiveBackgroundColor;
            Application.Current.Resources["LoginViewsPrimaryTipTextForegroundColor"] = config.Color.LoginViewPrimaryTipTextBackgroundColor;
            Application.Current.Resources["LoginViewsPrimaryTipTextBackgroundColor"] = config.Color.LoginViewPrimaryTipTextBackgroundColor;
            Application.Current.Resources["InstallViewsPrimaryPercentageTextForegroundColor"] = config.Color.InstallViewsPrimaryPercentageTextForegroundColor;
        }

        private static void ConfigFont(UiConfig config)
        {
            Application.Current.Resources["H1"] = config.Font.H1;
            Application.Current.Resources["H2"] = config.Font.H2;
            Application.Current.Resources["H3"] = config.Font.H3;
            Application.Current.Resources["H4"] = config.Font.H4;
            Application.Current.Resources["H5"] = config.Font.H5;
            Application.Current.Resources["H6"] = config.Font.H6;
            Application.Current.Resources["Paragraph"] = config.Font.Paragraph;
        }

        /// <summary>
        /// Map all .png image files from <paramref name="baseDirectory"/> to current application resource dictionary.
        /// </summary>
        /// <param name="baseDirectory"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static void ConfigImages(string baseDirectory, UiConfig config)
        {
            foreach (var pair in config.Images.ConfigurableImagesDictionary)
            {
                var path = System.IO.Path.Combine(System.IO.Path.Combine(baseDirectory, config.Path.ImagesFolder), pair.Value);
                if (File.Exists(path))
                {
                    Application.Current.Resources[pair.Key] = new BitmapImage(new Uri(path));
                }
            }
        }

        private static void ConfigGuardViews(UiConfig config)
        {
            Application.Current.Resources["GuardViewsH1"] = config.GuardViewsThemingConfig.H1;
            Application.Current.Resources["GuardViewsH2"] = config.GuardViewsThemingConfig.H2;
            Application.Current.Resources["GuardViewsH3"] = config.GuardViewsThemingConfig.H3;
            Application.Current.Resources["GuardViewsH4"] = config.GuardViewsThemingConfig.H4;
            Application.Current.Resources["GuardViewsH5"] = config.GuardViewsThemingConfig.H5;
            Application.Current.Resources["GuardViewsH6"] = config.GuardViewsThemingConfig.H6;

            Application.Current.Resources["GuardViewsPrimaryForegroundColor"] = config.GuardViewsThemingConfig.PrimaryForegroundColor;
            Application.Current.Resources["GuardViewsPrimaryBackgroundColor"] = config.GuardViewsThemingConfig.PrimaryBackgroundColor;
            Application.Current.Resources["GuardViewsPrimaryBorderColor"] = config.GuardViewsThemingConfig.PrimaryBorderColor;

            Application.Current.Resources["GuardViewsHoverForegroundColor"] = config.GuardViewsThemingConfig.HoverForegroundColor;
            Application.Current.Resources["GuardViewsHoverBackgroundColor"] = config.GuardViewsThemingConfig.HoverBackgroundColor;
            Application.Current.Resources["GuardViewsHoverBorderColor"] = config.GuardViewsThemingConfig.HoverBorderColor;

            Application.Current.Resources["GuardViewsPressedForegroundColor"] = config.GuardViewsThemingConfig.PressedForegroundColor;
            Application.Current.Resources["GuardViewsPressedBackgroundColor"] = config.GuardViewsThemingConfig.PressedBackgroundColor;
            Application.Current.Resources["GuardViewsPressedBorderColor"] = config.GuardViewsThemingConfig.PressedBorderColor;

            Application.Current.Resources["GuardViewsDisabledForegroundColor"] = config.GuardViewsThemingConfig.DisabledForegroundColor;
            Application.Current.Resources["GuardViewsDisabledBackgroundColor"] = config.GuardViewsThemingConfig.DisabledBackgroundColor;
            Application.Current.Resources["GuardViewsDisabledBorderColor"] = config.GuardViewsThemingConfig.DisabledBorderColor;

            Application.Current.Resources["GuardViewsActiveForegroundColor"] = config.GuardViewsThemingConfig.ActiveForegroundColor;
            Application.Current.Resources["GuardViewsActiveBackgroundColor"] = config.GuardViewsThemingConfig.ActiveBackgroundColor;
            Application.Current.Resources["GuardViewsActiveBorderColor"] = config.GuardViewsThemingConfig.ActiveBorderColor;

            Application.Current.Resources["GuardViewsButtonFontSize"] = config.GuardViewsThemingConfig.GuardViewsButtonConfig.ButtonFontSize;
            Application.Current.Resources["GuardViewsButtonWidth"] = config.GuardViewsThemingConfig.GuardViewsButtonConfig.ButtonWidth;
            Application.Current.Resources["GuardViewsButtonHeight"] = config.GuardViewsThemingConfig.GuardViewsButtonConfig.ButtonHeight;
            Application.Current.Resources["GuardViewsButtonBorderRadius"] = config.GuardViewsThemingConfig.GuardViewsButtonConfig.BorderRadius;
            Application.Current.Resources["GuardViewsButtonBorderThickness"] = config.GuardViewsThemingConfig.GuardViewsButtonConfig.BorderThickness;

            Application.Current.Resources["SettingLikeButtonFontSize"] = config.GuardViewsThemingConfig.SettingLikeButtonConfig.ButtonFontSize;
            Application.Current.Resources["SettingLikeButtonWidth"] = config.GuardViewsThemingConfig.SettingLikeButtonConfig.ButtonWidth;
            Application.Current.Resources["SettingLikeButtonHeight"] = config.GuardViewsThemingConfig.SettingLikeButtonConfig.ButtonHeight;
            Application.Current.Resources["SettingLikeButtonBorderRadius"] = config.GuardViewsThemingConfig.SettingLikeButtonConfig.BorderRadius;
            Application.Current.Resources["SettingLikeButtonBorderThickness"] = config.GuardViewsThemingConfig.SettingLikeButtonConfig.BorderThickness;
        }

        private static void ConfigSessionViews(UiConfig config)
        {
            Application.Current.Resources["SessionViewsH1"] = config.SessionViewsThemingConfig.H1;
            Application.Current.Resources["SessionViewsH2"] = config.SessionViewsThemingConfig.H2;
            Application.Current.Resources["SessionViewsH3"] = config.SessionViewsThemingConfig.H3;
            Application.Current.Resources["SessionViewsH4"] = config.SessionViewsThemingConfig.H4;
            Application.Current.Resources["SessionViewsH5"] = config.SessionViewsThemingConfig.H5;
            Application.Current.Resources["SessionViewsH6"] = config.SessionViewsThemingConfig.H6;

            Application.Current.Resources["SessionViewsPrimaryForegroundColor"] = config.SessionViewsThemingConfig.PrimaryForegroundColor;
            Application.Current.Resources["SessionViewsPrimaryBackgroundColor"] = config.SessionViewsThemingConfig.PrimaryBackgroundColor;
            Application.Current.Resources["SessionViewsPrimaryBorderColor"] = config.SessionViewsThemingConfig.PrimaryBorderColor;

            Application.Current.Resources["SessionViewsHoverForegroundColor"] = config.SessionViewsThemingConfig.HoverForegroundColor;
            Application.Current.Resources["SessionViewsHoverBackgroundColor"] = config.SessionViewsThemingConfig.HoverBackgroundColor;
            Application.Current.Resources["SessionViewsHoverBorderColor"] = config.SessionViewsThemingConfig.HoverBorderColor;

            Application.Current.Resources["SessionViewsPressedForegroundColor"] = config.SessionViewsThemingConfig.PressedForegroundColor;
            Application.Current.Resources["SessionViewsPressedBackgroundColor"] = config.SessionViewsThemingConfig.PressedBackgroundColor;
            Application.Current.Resources["SessionViewsPressedBorderColor"] = config.SessionViewsThemingConfig.PressedBorderColor;

            Application.Current.Resources["SessionViewsDisabledForegroundColor"] = config.SessionViewsThemingConfig.DisabledForegroundColor;
            Application.Current.Resources["SessionViewsDisabledBackgroundColor"] = config.SessionViewsThemingConfig.DisabledBackgroundColor;
            Application.Current.Resources["SessionViewsDisabledBorderColor"] = config.SessionViewsThemingConfig.DisabledBorderColor;

            Application.Current.Resources["SessionViewsActiveForegroundColor"] = config.SessionViewsThemingConfig.ActiveForegroundColor;
            Application.Current.Resources["SessionViewsActiveBackgroundColor"] = config.SessionViewsThemingConfig.ActiveBackgroundColor;
            Application.Current.Resources["SessionViewsActiveBorderColor"] = config.SessionViewsThemingConfig.ActiveBorderColor;

            Application.Current.Resources["SessionViewsButtonFontSize"] = config.SessionViewsThemingConfig.SessionViewsButtonConfig.ButtonFontSize;
            Application.Current.Resources["SessionViewsButtonWidth"] = config.SessionViewsThemingConfig.SessionViewsButtonConfig.ButtonWidth;
            Application.Current.Resources["SessionViewsButtonHeight"] = config.SessionViewsThemingConfig.SessionViewsButtonConfig.ButtonHeight;
            Application.Current.Resources["SessionViewsButtonBorderRadius"] = config.SessionViewsThemingConfig.SessionViewsButtonConfig.BorderRadius;
            Application.Current.Resources["SessionViewsButtonBorderThickness"] = config.SessionViewsThemingConfig.SessionViewsButtonConfig.BorderThickness;

            Application.Current.Resources["SessionViewsTabButtonFontSize"] = config.SessionViewsThemingConfig.TabButtonConfig.ButtonFontSize;
            Application.Current.Resources["SessionViewsTabButtonWidth"] = config.SessionViewsThemingConfig.TabButtonConfig.ButtonWidth;
            Application.Current.Resources["SessionViewsTabButtonHeight"] = config.SessionViewsThemingConfig.TabButtonConfig.ButtonHeight;
            Application.Current.Resources["SessionViewsTabButtonBorderRadius"] = config.SessionViewsThemingConfig.TabButtonConfig.BorderRadius;
            Application.Current.Resources["SessionViewsTabButtonBorderThickness"] = config.SessionViewsThemingConfig.TabButtonConfig.BorderThickness;

        }
    }
}
