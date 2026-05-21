#region Licence
/****************************************************************
 *  Filename: DpiUtil.cs
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
using System.Windows.Threading;
using LeapVR.Shared.Lib.Win.Structs;

namespace LeapVR.Shared.Lib.Win.WinApi
{
    public static class DpiUtil
    {
        public static DpiInfo GetPrimaryScreenDpiSafeResolution()
        {
            if (Application.Current.CheckAccess())
            {
                PresentationSource presentationSource =
                    PresentationSource.FromVisual(Application.Current.MainWindow);
                Matrix matix = presentationSource.CompositionTarget.TransformToDevice;

                return new DpiInfo()
                {
                    ScreenSize =
                        new Size(SystemParameters.PrimaryScreenWidth * matix.M22,
                            SystemParameters.PrimaryScreenHeight * matix.M11),
                    FactorX = matix.M22,
                    FactorY = matix.M11
                };
            }
            return Application.Current.Dispatcher.Invoke(GetPrimaryScreenDpiSafeResolution,
                DispatcherPriority.Normal);
        }
    }
}
