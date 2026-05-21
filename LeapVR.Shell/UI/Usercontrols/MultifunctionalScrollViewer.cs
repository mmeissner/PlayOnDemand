#region Licence
/****************************************************************
 *  Filename: MultifunctionalScrollViewer.cs
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
using System.Windows.Controls.Primitives;

namespace LeapVR.Shell.UI.Usercontrols
{
    [TemplatePart(Name = "PART_ScrollLeftButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_ScrollRightButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_ScrollContentPresenter", Type = typeof(ScrollContentPresenter))]

    public class MultifunctionalScrollViewer : ScrollViewer
    {
        static MultifunctionalScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultifunctionalScrollViewer), new FrameworkPropertyMetadata(typeof(MultifunctionalScrollViewer)));
        }


        public DataTemplate ScrollLeftButtonContentTemplate
        {
            get => (DataTemplate)GetValue(ScrollLeftButtonContentTemplateProperty);
            set => SetValue(ScrollLeftButtonContentTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollLeftButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollLeftButtonContentTemplateProperty =
            DependencyProperty.Register("ScrollLeftButtonContentTemplate", typeof(DataTemplate), typeof(MultifunctionalScrollViewer));

        public ControlTemplate ScrollLeftButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollLeftButtonTemplateProperty);
            set => SetValue(ScrollLeftButtonTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollLeftButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollLeftButtonTemplateProperty =
            DependencyProperty.Register("ScrollLeftButtonTemplate", typeof(ControlTemplate), typeof(MultifunctionalScrollViewer));

        public DataTemplate ScrollRightButtonContentTemplate
        {
            get => (DataTemplate)GetValue(ScrollRightButtonContentTemplateProperty);
            set => SetValue(ScrollRightButtonContentTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollRightButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollRightButtonContentTemplateProperty =
            DependencyProperty.Register("ScrollRightButtonContentTemplate", typeof(DataTemplate), typeof(MultifunctionalScrollViewer));


        public ControlTemplate ScrollRightButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollRightButtonTemplateProperty);
            set => SetValue(ScrollRightButtonTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollRightButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollRightButtonTemplateProperty =
            DependencyProperty.Register("ScrollRightButtonTemplate", typeof(ControlTemplate), typeof(MultifunctionalScrollViewer));



        public DataTemplate ScrollUpButtonContentTemplate
        {
            get => (DataTemplate)GetValue(ScrollUpButtonContentTemplateProperty);
            set => SetValue(ScrollUpButtonContentTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollUpButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollUpButtonContentTemplateProperty =
            DependencyProperty.Register("ScrollUpButtonContentTemplate", typeof(DataTemplate), typeof(MultifunctionalScrollViewer));

        public ControlTemplate ScrollUpButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollUpButtonTemplateProperty);
            set => SetValue(ScrollUpButtonTemplateProperty, value);
        }
        // Using a DependencyProperty as the backing store for ScrollUpButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollUpButtonTemplateProperty =
            DependencyProperty.Register("ScrollUpButtonTemplate", typeof(ControlTemplate), typeof(MultifunctionalScrollViewer));

        public DataTemplate ScrollDownButtonContentTemplate
        {
            get => (DataTemplate)GetValue(ScrollDownButtonContentTemplateProperty);
            set => SetValue(ScrollDownButtonContentTemplateProperty, value);
        }

        // Using a DependencyProperty as the backing store for ScrollDownButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollDownButtonContentTemplateProperty =
            DependencyProperty.Register("ScrollDownButtonContentTemplate", typeof(DataTemplate), typeof(MultifunctionalScrollViewer));

        public ControlTemplate ScrollDownButtonTemplate
        {
            get => (ControlTemplate)GetValue(ScrollDownButtonTemplateProperty);
            set => SetValue(ScrollDownButtonTemplateProperty, value);
        }

        // Using a DependencyProperty as the backing store for ScrollDownButtonTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollDownButtonTemplateProperty =
            DependencyProperty.Register("ScrollDownButtonTemplate", typeof(ControlTemplate), typeof(MultifunctionalScrollViewer));


        /// <summary>
        /// Get or set the visibility of the vertical repeat buttons.
        /// </summary>

        public Visibility VerticalVisibility
        {
            get => (Visibility)GetValue(VerticalVisibilityProperty);
            set => SetValue(VerticalVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for VerticalVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalVisibilityProperty =
            DependencyProperty.Register("VerticalVisibility", typeof(Visibility), typeof(MultifunctionalScrollViewer));

        /// <summary>
        /// Get or set the visibility of the horizontal repeat buttons.
        /// </summary>
        public Visibility HorizontalVisibility
        {
            get => (Visibility)GetValue(HorizontalVisibilityProperty);
            set => SetValue(HorizontalVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for HorizontalVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalVisibilityProperty =
            DependencyProperty.Register("HorizontalVisibility", typeof(Visibility), typeof(MultifunctionalScrollViewer));



        public bool EnableColumns
        {
            get => (bool)GetValue(EnableColumnsProperty);
            set => SetValue(EnableColumnsProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableColumnsProperty =
            DependencyProperty.Register("EnableColumns", typeof(bool), typeof(MultifunctionalScrollViewer));




        private RepeatButton _leftRepeatButton;
        private RepeatButton _rightRepeatButton;
        private ScrollContentPresenter _contentPresenter;
        public override void OnApplyTemplate()
        {
            _contentPresenter = GetTemplateChild("PART_ScrollContentPresenter") as ScrollContentPresenter;

            if ((ScrollUnit)GetValue(VirtualizingPanel.ScrollUnitProperty) == ScrollUnit.Item)
            {
                _leftRepeatButton = GetTemplateChild("PART_ScrollLeftButton") as RepeatButton;
                _rightRepeatButton = GetTemplateChild("PART_ScrollRightButton") as RepeatButton;
                if (_leftRepeatButton != null)
                {
                    _leftRepeatButton.Click += _leftRepeatButton_Click;
                }
                if (_rightRepeatButton != null)
                {
                    _rightRepeatButton.Click += _rightRepeatButton_Click;
                }
            }



            base.OnApplyTemplate();
        }

        private void _rightRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            var uniformGrid = _contentPresenter.Content as UniformGrid;
            var childrenCount = uniformGrid?.Children.Count;
            var itemWidth = _contentPresenter.ExtentWidth / childrenCount;
            if (itemWidth != null)
            {
                var offsetValue = _contentPresenter.HorizontalOffset + itemWidth.Value;
                _contentPresenter.SetHorizontalOffset(offsetValue);
            }
        }

        private void _leftRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            var uniformGrid = _contentPresenter.Content as UniformGrid;
            var childrenCount = uniformGrid?.Children.Count;
            var itemWidth = _contentPresenter.ExtentWidth / childrenCount;
            if (itemWidth != null)
            {
                var offsetValue = _contentPresenter.HorizontalOffset - itemWidth.Value;
                _contentPresenter.SetHorizontalOffset(offsetValue);
            }
        }

    }
}
