#region Licence
/****************************************************************
 *  Filename: SwitchButton.cs
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
using System.Windows.Media;

namespace LeapVR.Shell.UI.Usercontrols
{
    [TemplatePart(Name = "PART_Root", Type = typeof(FrameworkElement))]
    public class SwitchButton : ButtonBase
    {

        #region Fields & Properties

        public static readonly DependencyProperty IsSwitchOnProperty =
            DependencyProperty.Register("IsSwitchOn", typeof(bool), typeof(SwitchButton), new FrameworkPropertyMetadata(false, (sender, e) =>{}));

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof(Brush), typeof(SwitchButton));

        public static readonly DependencyProperty SwitchOffTextProperty =
            DependencyProperty.Register("SwitchOffText", typeof(string), typeof(SwitchButton));

        public static readonly DependencyProperty SwitchOnTextProperty =
            DependencyProperty.Register("SwitchOnText", typeof(string), typeof(SwitchButton));

        /// <summary>
        /// 获取/设置开关状态
        /// </summary>
        public bool IsSwitchOn
        {
            get { return (bool)GetValue(IsSwitchOnProperty); }
            set { SetValue(IsSwitchOnProperty, value); }
        }

        /// <summary>
        /// 获取/设置文本颜色
        /// </summary>
        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }
        /// <summary>
        /// 获取/设置关闭状态文本
        /// </summary>
        public string SwitchOffText
        {
            get { return (string)GetValue(SwitchOffTextProperty); }
            set { SetValue(SwitchOffTextProperty, value); }
        }
        /// <summary>
        /// 获取/设置开启状态文本
        /// </summary>
        public string SwitchOnText
        {
            get { return (string)GetValue(SwitchOnTextProperty); }
            set { SetValue(SwitchOnTextProperty, value); }
        }

        #endregion

        #region Constructors
        static SwitchButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchButton), new FrameworkPropertyMetadata(typeof(SwitchButton)));
        }

        #endregion

        #region Methods

        public sealed override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        /// <summary>Called when a control is clicked by the mouse or the keyboard. </summary>
        protected override void OnClick()
        {
            base.OnClick();
            IsSwitchOn = !IsSwitchOn;
        }
        #endregion

    }

}
