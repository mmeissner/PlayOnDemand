#region Licence
/****************************************************************
 *  Filename: ShellClientHelper.cs
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
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using LeapVR.Shell.Domain.Models.Billing;
using NLog;
using QRCoder;

namespace LeapVR.Shell.UI.Core
{
    static class ShellClientHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Static Methods
        public static string GetTimeDisplay(TimeSpan duration)
        {
            var unitDisplayStringBuilder = new StringBuilder();

            if (duration.TotalSeconds < 60)
            {
                unitDisplayStringBuilder.Append(
                        $"{duration.TotalSeconds} {Language.Resources.Global_Unit_Sec}");
            }
            else if (duration.TotalMinutes < 60)
            {
                unitDisplayStringBuilder.Append(
                        $"{(int)duration.TotalMinutes} {Language.Resources.Global_Unit_Min}");
                if (duration.Seconds != 0)
                {
                    unitDisplayStringBuilder.Append(" ");
                    unitDisplayStringBuilder.Append(
                            $"{duration.Seconds} {Language.Resources.Global_Unit_Sec}");
                }
            }
            else
            {
                unitDisplayStringBuilder.Append(
                        $"{(int)duration.TotalHours} {Language.Resources.Global_Unit_Hour}");
                if (duration.Minutes != 0)
                {
                    unitDisplayStringBuilder.Append(" ");
                    unitDisplayStringBuilder.Append(
                            $"{duration.Minutes} {Language.Resources.Global_Unit_Min}");
                }
            }

            return unitDisplayStringBuilder.ToString();
        }

        /// <summary>
        /// Generate a string according to <paramref name="duration"/> with conventions.
        /// </summary>
        /// <param name="duration">input time span</param>
        /// <returns></returns>
        public static string GetTimeSpanDisplay(TimeSpan duration)
        {
            var sb = new StringBuilder();
            if(duration.TotalSeconds < 60)
            {
                sb.Append($"{duration.TotalSeconds} {Language.Resources.Global_Unit_Sec}");
            }
            else if(duration.TotalMinutes < 60)
            {
                sb.Append($"{(int)duration.TotalMinutes} {Language.Resources.Global_Unit_Min}");
                if(duration.Seconds != 0)
                {
                    sb.Append(" ");
                    sb.Append($"{duration.Seconds} {Language.Resources.Global_Unit_Sec}");
                }
            }
            else
            {
                sb.Append($"{(int)duration.TotalHours} {Language.Resources.Global_Unit_Hour}");
                if(duration.Minutes != 0)
                {
                    sb.Append(" ");
                    sb.Append($"{duration.Minutes} {Language.Resources.Global_Unit_Min}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates the qr code.
        /// </summary>
        /// <param name="value">The value for the QRCode.</param>
        /// <param name="eccLevel">The Error Correction Level for the QR-Code.</param>
        /// <param name="forceUtf8">if set to <c>true</c> [force UTF8].</param>
        /// <param name="forceUtfBom">if set to <c>true</c> [force utf bom].</param>
        /// <returns></returns>
        public static ImageSource GenerateQrCode(
                string value,
                QRCodeGenerator.ECCLevel eccLevel = QRCodeGenerator.ECCLevel.M,
                bool forceUtf8 = false,
                bool forceUtfBom = false)
        {
            Logger.Info($"Generating QR-Code for string={value}, with settings");
            Bitmap icon = null;
            try
            {
                icon = !(Application.Current.Resources["IconLogo"] is BitmapSource logo) ? null :
                        UIHelper.BitmapSourceToBitmap(logo);

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(value, eccLevel, forceUtf8, forceUtfBom);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(
                        40,
                        System.Drawing.Color.Black,
                        System.Drawing.Color.White,
                        icon,
                        15,
                        20,
                        true);
                var imageSource = UIHelper.BitmapToBitmapSource(qrCodeImage);
                return imageSource;
            }
            finally
            {
                icon?.Dispose();
            }
        }
        public static ImageSource GenerateNewBackgroundImageFrom(
                CutOffData cutOffData,
                TransparancyArea transparancyArea)
        {
            //As Image might be streched we need a ratio, however this needs to be calculated by the images container.
            //MainWindow only works for full screen image covering the whole mainwindow
            var ratioOfWidth = cutOffData.DisplayContainerInfo.ContainerWidth / cutOffData.SourceImage.PixelWidth;
            var ratioOfHeight = cutOffData.DisplayContainerInfo.ContainerHeight / cutOffData.SourceImage.PixelHeight;
            var intersect = transparancyArea.GetIntersection(cutOffData.DisplayContainerInfo, out var intersection);


            //No Intersection
            if(!intersect) return cutOffData.SourceImage;

            //DPI Info
            //var dpiInfo = transparancyArea.GetDpiInfo();

            //Calculate Scaled Values
            return UIHelper.MakeImageWithRectangleAlphaArea(
                    cutOffData.SourceImage,
                    Convert.ToInt32(transparancyArea.Width / ratioOfWidth),
                    Convert.ToInt32(transparancyArea.Height / ratioOfHeight),
                    Convert.ToInt32(intersection.X / ratioOfWidth),
                    Convert.ToInt32(intersection.Y / ratioOfHeight),
                    1);
        }

        public static ImageSource GenerateNewBackgroundImageFrom(
                BitmapImage sourceImage,
                System.Windows.Size sizeOfOpenVrWindow,
                int heightOfOpenVrBanner,
                double containerHeight,
                double containerWidth,
                int marginToEdge,
                bool isBottomAlignment = false)
        {
            if(Application.Current.MainWindow == null)
            {
                return null;
            }

            //As Image might be streched we need a ratio, however this needs to be calculated by the images container.
            //MainWindow only works for full screen image covering the whole mainwindow
            var ratioOfWidth = sourceImage.PixelWidth / containerWidth;
            var ratioOfHeight = sourceImage.PixelHeight / containerHeight;

            var widthOfViewport = (int)(sizeOfOpenVrWindow.Width * ratioOfWidth);
            var heightOfViewPort = (int)((sizeOfOpenVrWindow.Height - heightOfOpenVrBanner) * ratioOfHeight);


            var positionXOfViewport = (int)(marginToEdge * ratioOfWidth);
            int positionYOfViewport;
            //This calculation only works as we move the openvr window to left buttom side by our own and all time assume this position
            if(isBottomAlignment)
            {
                positionYOfViewport = (int)((0 + sizeOfOpenVrWindow.Height - heightOfOpenVrBanner) * ratioOfHeight);
            }
            else
            {
                positionYOfViewport =
                        (int)((Application.Current.MainWindow.Height -
                               sizeOfOpenVrWindow.Height +
                               heightOfOpenVrBanner) *
                              ratioOfHeight);
            }


            return UIHelper.MakeImageWithRectangleAlphaArea(
                    sourceImage,
                    widthOfViewport,
                    heightOfViewPort,
                    positionXOfViewport,
                    positionYOfViewport,
                    10);
        }

        public class CutOffData
        {
            public CutOffData(
                    UIElement imageContainerCtrl,
                    BitmapImage sourceImage,
                    MainWindow mainWindow)
            {
                DisplayContainerInfo = new DisplayContainer(imageContainerCtrl, mainWindow.WindowBase);
                SourceImage = sourceImage;
                MainWindowInfo = mainWindow;
            }
            public CutOffData(
                    UIElement imageContainerCtrl,
                    Window mainWindow,
                    BitmapImage sourceImage)
            {
                DisplayContainerInfo = new DisplayContainer(imageContainerCtrl, mainWindow);
                SourceImage = sourceImage;
                MainWindowInfo = new MainWindow(mainWindow);
            }
            public DisplayContainer DisplayContainerInfo { get; }
            public MainWindow MainWindowInfo { get; }
            public BitmapImage SourceImage { get; }
        }

        public static IDictionary<string, object> GetUniversalDialogSettings(
                double? windowWidth = null,
                double? windHeight = null)
        {
            var settings = new Dictionary<string, object>
                           {
                                   {"WindowStyle", WindowStyle.None},
                                   {"AllowsTransparency", true},
                                   {"Background", new SolidColorBrush(Colors.Transparent)},
                                   {"WindowState", WindowState.Normal},
                                   {"WindowStartupLocation", WindowStartupLocation.CenterOwner},
                                   {"ShowInTaskbar", false},
                           };

            if(windowWidth != null)
            {
                settings.Add("Width", windowWidth.Value);
            }

            if(windHeight != null)
            {
                settings.Add("Height", windHeight.Value);
            }

            if(windHeight != null || windowWidth != null)
            {
                settings.Add("SizeToContent", SizeToContent.Manual);
            }

            return settings;
        }
        #endregion
    }
}