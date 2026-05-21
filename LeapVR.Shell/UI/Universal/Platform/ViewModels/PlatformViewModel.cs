#region Licence
/****************************************************************
 *  Filename: PlatformViewModel.cs
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
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Platform;

namespace LeapVR.Shell.UI.Universal.Platform.ViewModels
{
    public class PlatformViewModel : Screen
    {
        public ImageSource Icon { get; }
        public IPlatform Platform { get; }

        public PlatformViewModel(IPlatform platform, ImageSource icon)
        {
            if(icon != null)Icon = icon;
            else Icon = Application.Current.Resources["Icon_UnknownPlatform"] as ImageSource;
            Platform = platform;
        }
    }
}
