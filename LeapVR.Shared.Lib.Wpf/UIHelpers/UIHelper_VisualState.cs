#region Licence
/****************************************************************
 *  Filename: UIHelper_VisualState.cs
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

namespace LeapVR.Shared.Lib.Wpf.UIHelpers
{
    public static partial class UIHelper
    {
        public static string GetVisualState(DependencyObject obj)
        {
            return (string)obj.GetValue(VisualStateProperty);
        }
        public static void SetVisualState(DependencyObject obj, string value)
        {
            obj.SetValue(VisualStateProperty, value);
        }
        public static readonly DependencyProperty VisualStateProperty =
            DependencyProperty.RegisterAttached(
                "VisualState",
                typeof(string),
                typeof(UIHelper),
                new PropertyMetadata((s, e) =>
                {
                    var propertyName = (string)e.NewValue;
                    var ctrl = s as FrameworkElement;
                    if (ctrl == null)
                        throw new InvalidOperationException("This attached property only supports types derived from FrameworkElement.");
                    VisualStateManager.GoToState(ctrl, propertyName, true);
                }));

    }
}
