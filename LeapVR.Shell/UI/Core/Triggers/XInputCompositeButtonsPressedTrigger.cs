#region Licence
/****************************************************************
 *  Filename: XInputCompositeButtonsPressedTrigger.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-2-10
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using LeapVR.Shell.Domain.Models.Input.XInput;

namespace LeapVR.Shell.UI.Core.Triggers
{
    /// <summary>
    /// 
    /// </summary>
    public class XInputCompositeButtonsPressedTrigger : TriggerBase<DependencyObject>
    {

        #region Fields & Properties

        private readonly Stopwatch _throttleTimer = new Stopwatch();
        private readonly Stopwatch _timer = new Stopwatch();
        //private IDisposable _whenXInputButtonStateChangedSubscription;

        private List<XInputButtons> _expetedXButtons = new List<XInputButtons>();
        /// <summary>
        /// A string that contains one or more <see cref="XInputButtons"/> value split by ','.
        /// </summary>
        public string CompositeXButtons
        {
            get => (string)GetValue(CompositeXButtonsProperty);
            set => SetValue(CompositeXButtonsProperty, value);
        }
        // Using a DependencyProperty as the backing store for CompositeXButtons.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CompositeXButtonsProperty =
            DependencyProperty.Register("CompositeXButtons", typeof(string), typeof(XInputCompositeButtonsPressedTrigger), new FrameworkPropertyMetadata(string.Empty, OnCompositeXButtonsPropertyChanged));

        public int TriggerAfter
        {
            get => (int)GetValue(TriggerAfterProperty);
            set => SetValue(TriggerAfterProperty, value);
        }

        // Using a DependencyProperty as the backing store for TriggerAfter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerAfterProperty =
            DependencyProperty.Register("TriggerAfter", typeof(int), typeof(XInputCompositeButtonsPressedTrigger), new PropertyMetadata(5000));

        public int ThrottleTimeout
        {
            get => (int)GetValue(ThrottleTimeoutProperty);
            set => SetValue(ThrottleTimeoutProperty, value);
        }
        // Using a DependencyProperty as the backing store for ThrottleTimeout.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThrottleTimeoutProperty =
            DependencyProperty.Register("ThrottleTimeout", typeof(int), typeof(XInputCompositeButtonsPressedTrigger), new PropertyMetadata(5000));


        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }
        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(XInputCompositeButtonsPressedTrigger), new FrameworkPropertyMetadata(true, OnIsEnabledPropertyChanged));

        #endregion

        #region Constructors

        #endregion

        #region Methods

        private void OnXInputCompositeButtonsPressed(XInputCompositeArgs args)
        {
            if (args.XButtons == null || args.XButtons.Length <= 0)
            {
                _timer.Reset();
                return;
            }

            if (_throttleTimer.IsRunning && _throttleTimer.Elapsed < TimeSpan.FromMilliseconds(ThrottleTimeout))
            {
                _timer.Reset();
                return;
            }

            var triggerAfter = TimeSpan.FromMilliseconds(TriggerAfter);

            if (_expetedXButtons == null || _expetedXButtons.Count <= 0 || !_expetedXButtons.All(e => args.XButtons.Select(q => q.XButton).Contains(e)))
            {
                _timer.Reset();
                return;
            }
            if (!_timer.IsRunning)
            {
                _timer.Start();
            }

            var targetButtons = (from btn in args.XButtons.TakeWhile(a => _expetedXButtons.Contains(a.XButton)) select btn).ToArray();

            if (_timer.Elapsed < triggerAfter) return;

            var collection = (from s in targetButtons select s.XButton.ToString()).ToArray();
            var btnStrings = string.Join(",", collection);
            Console.WriteLine($@"buttons '{btnStrings}' pressed for {_timer.Elapsed}.");

            InvokeActions(null);
            _timer.Reset();
            _throttleTimer.Restart();
        }

        private static void OnCompositeXButtonsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (!(d is XInputCompositeButtonsPressedTrigger trigger)) return;
            if (!(args.NewValue is string compositeButtonsString) || string.IsNullOrEmpty(compositeButtonsString)) return;

            var btnStringArr = compositeButtonsString.Split(',');
            trigger._expetedXButtons = new List<XInputButtons>();
            foreach (var xbuttonString in btnStringArr)
            {
                if (Enum.TryParse<XInputButtons>(xbuttonString, out var xbutton))
                {
                    trigger._expetedXButtons.Add(xbutton);
                }
            }

        }

        private static void OnIsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (!(d is XInputCompositeButtonsPressedTrigger trigger)) return;
            if (args.NewValue is bool isEnabled && isEnabled)
            {
                //trigger._whenXInputButtonStateChangedSubscription?.Dispose();
                //trigger._whenXInputButtonStateChangedSubscription = XInputGuard.WhenXInputCompositeButtonsPressed.ObserveOnDispatcher().Subscribe(trigger.OnXInputCompositeButtonsPressed);
            }
            else
            {
                //trigger._whenXInputButtonStateChangedSubscription?.Dispose();
            }
        }
        #endregion
    }
}
