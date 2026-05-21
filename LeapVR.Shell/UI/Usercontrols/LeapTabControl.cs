#region Licence
/****************************************************************
 *  Filename: LeapTabControl.cs
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
using System.Windows.Controls;

namespace LeapVR.Shell.UI.Usercontrols
{
    public class LeapTabControl : TabControl
    {
        static LeapTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LeapTabControl), new FrameworkPropertyMetadata(typeof(LeapTabControl)));
        }

        public Visibility HeaderVisibility
        {
            get => (Visibility)GetValue(HeaderVisibilityProperty);
            set => SetValue(HeaderVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for HeaderVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(Visibility), typeof(LeapTabControl));

        public ControlTemplate ScrollLeftButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollLeftButtonTemplateProperty);
            set => SetValue(ScrollLeftButtonTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollLeftButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollLeftButtonTemplateProperty =
            DependencyProperty.Register("ScrollLeftButtonTemplate", typeof(ControlTemplate), typeof(LeapTabControl));
        public ControlTemplate ScrollRightButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollRightButtonTemplateProperty);
            set => SetValue(ScrollRightButtonTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollRightButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollRightButtonTemplateProperty =
            DependencyProperty.Register("ScrollRightButtonTemplate", typeof(ControlTemplate), typeof(LeapTabControl));
        public ControlTemplate ScrollUpButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollUpButtonTemplateProperty);
            set => SetValue(ScrollUpButtonTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollUpButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollUpButtonTemplateProperty =
            DependencyProperty.Register("ScrollUpButtonTemplate", typeof(ControlTemplate), typeof(LeapTabControl));
        public ControlTemplate ScrollDownButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollDownButtonTemplateProperty);
            set => SetValue(ScrollDownButtonTemplateProperty, value);
        }

        // Using a DependencyProperty as the backing store for ScrollDownButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollDownButtonTemplateProperty =
            DependencyProperty.Register("ScrollDownButtonTemplate", typeof(ControlTemplate), typeof(LeapTabControl));



    }
}
