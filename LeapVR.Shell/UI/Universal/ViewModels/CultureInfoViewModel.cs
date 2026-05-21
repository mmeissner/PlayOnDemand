#region Licence
/****************************************************************
 *  Filename: CultureInfoViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-10-11
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    public class CultureInfoViewModel : Screen
    {
        #region Fields & Properties
        public CultureInfo CultureInfo { get; }
        private ImageSource _icon;
        public ImageSource Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Constructors
        public CultureInfoViewModel(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
            ApplyIconAccordingToCulture(CultureInfo);
        }

        #endregion

        #region Methods

        private void ApplyIconAccordingToCulture(CultureInfo culture)
        {
            var iconResourceKey = $"IconFlag{culture.ThreeLetterWindowsLanguageName}";
            Icon = Application.Current.Resources[iconResourceKey] as ImageSource ?? Application.Current.Resources["IconFlagUnknown"] as ImageSource;
        }
        #endregion


    }
}
