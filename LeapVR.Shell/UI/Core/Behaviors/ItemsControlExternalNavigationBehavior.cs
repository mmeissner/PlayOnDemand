#region Licence
/****************************************************************
 *  Filename: ItemsControlExternalNavigationBehavior.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-13
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using LeapVR.Shared.Lib.Wpf.UIHelpers;
using NLog;
using Caliburn.Micro;
using LogManager = NLog.LogManager;

namespace LeapVR.Shell.UI.Core.Behaviors
{
    public class ItemsControlExternalNavigationBehavior : Behavior<ItemsControl>
    {

        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ButtonBase ScrollLeftButton
        {
            get => (ButtonBase)GetValue(ScrollLeftButtonProperty);
            set => SetValue(ScrollLeftButtonProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollLeftButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollLeftButtonProperty =
            DependencyProperty.Register("ScrollLeftButton", typeof(ButtonBase), typeof(ItemsControlExternalNavigationBehavior));

        public ButtonBase ScrollRightButton
        {
            get => (ButtonBase)GetValue(ScrollRightButtonProperty);
            set => SetValue(ScrollRightButtonProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollRightButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollRightButtonProperty =
            DependencyProperty.Register("ScrollRightButton", typeof(ButtonBase), typeof(ItemsControlExternalNavigationBehavior));


        public ButtonBase ScrollUpButton
        {
            get => (ButtonBase)GetValue(ScrollUpButtonProperty);
            set => SetValue(ScrollUpButtonProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollUpButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollUpButtonProperty =
            DependencyProperty.Register("ScrollUpButton", typeof(ButtonBase), typeof(ItemsControlExternalNavigationBehavior));


        public ButtonBase ScrollDownButton
        {
            get => (ButtonBase)GetValue(ScrollDownButtonProperty);
            set => SetValue(ScrollDownButtonProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollDownButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollDownButtonProperty =
            DependencyProperty.Register("ScrollDownButton", typeof(ButtonBase), typeof(ItemsControlExternalNavigationBehavior));

        private ScrollViewer _innerScrollViewer;
        #endregion


        #region Methods

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            _innerScrollViewer.ScrollChanged -= _innerScrollViewer_ScrollChanged;
            if (ScrollLeftButton != null) ScrollLeftButton.Click -= ScrollLeftButton_Click;
            if (ScrollRightButton != null) ScrollRightButton.Click -= ScrollRightButton_Click;
            if (ScrollUpButton != null) ScrollUpButton.Click -= ScrollUpButton_Click;
            if (ScrollDownButton != null) ScrollDownButton.Click -= ScrollDownButton_Click;
        }
        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)

        {
            _innerScrollViewer = UIHelper.FindVisualChild<ScrollViewer>(AssociatedObject);
            _innerScrollViewer.ScrollChanged += _innerScrollViewer_ScrollChanged;

            PrepareHorizontalButtons();
            PrepareVerticalButtons();


            if (ScrollLeftButton != null) ScrollLeftButton.Click += ScrollLeftButton_Click;
            if (ScrollRightButton != null) ScrollRightButton.Click += ScrollRightButton_Click;
            if (ScrollUpButton != null) ScrollUpButton.Click += ScrollUpButton_Click;
            if (ScrollDownButton != null) ScrollDownButton.Click += ScrollDownButton_Click;

        }

        private void _innerScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            PrepareHorizontalButtons();
            PrepareVerticalButtons();
        }

        private void ScrollLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(AssociatedObject.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement element)) return;
            var scrollWidth = element.ActualWidth;

            var targetOffset = _innerScrollViewer.HorizontalOffset - scrollWidth;
            _innerScrollViewer.ScrollToHorizontalOffset(targetOffset);
            Logger.Debug( $"{nameof(ScrollLeftButton)} clicked. One item shifted to left.");
        }

        private void ScrollRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(AssociatedObject.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement element)) return;
            var scrollWidth = element.ActualWidth;

            var targetOffset = _innerScrollViewer.HorizontalOffset + scrollWidth;
            _innerScrollViewer.ScrollToHorizontalOffset(targetOffset);
            Logger.Debug( $"{nameof(ScrollRightButton)} button clicked. One item shifted to right.");
        }

        private void ScrollUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(AssociatedObject.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement element)) return;
            var scrollHeight = element.ActualHeight;

            var targetOffset = _innerScrollViewer.VerticalOffset - scrollHeight;
            _innerScrollViewer.ScrollToVerticalOffset(targetOffset);

            Logger.Debug(  $"{nameof(ScrollUpButton)} button clicked. One item shifted to up.");
        }
        private void ScrollDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(AssociatedObject.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement element)) return;
            var scrollHeight = element.ActualHeight;

            var targetOffset = _innerScrollViewer.VerticalOffset + scrollHeight;
            _innerScrollViewer.ScrollToVerticalOffset(targetOffset);

            Logger.Debug( $"{nameof(ScrollDownButton)} button clicked. One item shifted to down.");
        }

        private void PrepareHorizontalButtons()
        {
            if (_innerScrollViewer == null || ScrollLeftButton == null || ScrollRightButton == null)
            {
                return;
            }
            var visibility = _innerScrollViewer.ScrollableWidth <= 0 ? Visibility.Collapsed : Visibility.Visible;
            ScrollLeftButton.Visibility = visibility;
            ScrollRightButton.Visibility = visibility;

            if (visibility != Visibility.Visible) return;

            ScrollLeftButton.IsEnabled = !(Math.Abs(_innerScrollViewer.HorizontalOffset) <= 0);
            ScrollRightButton.IsEnabled = !(_innerScrollViewer.HorizontalOffset + _innerScrollViewer.ViewportWidth >= _innerScrollViewer.ExtentWidth);
        }

        private void PrepareVerticalButtons()
        {
            if (_innerScrollViewer == null || ScrollUpButton == null || ScrollDownButton == null)
            {
                return;
            }
            var visibility = _innerScrollViewer.ScrollableHeight <= 0 ? Visibility.Collapsed : Visibility.Visible;
            ScrollUpButton.Visibility = visibility;
            ScrollDownButton.Visibility = visibility;

            if (visibility != Visibility.Visible) return;

            ScrollUpButton.IsEnabled = !(Math.Abs(_innerScrollViewer.VerticalOffset) <= 0);
            ScrollDownButton.IsEnabled = !(_innerScrollViewer.VerticalOffset + _innerScrollViewer.ViewportHeight >= _innerScrollViewer.ExtentHeight);

        }
        #endregion
    }
}
