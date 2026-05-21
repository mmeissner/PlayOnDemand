#region Licence
/****************************************************************
 *  Filename: XButtonTrigger.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-20
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using LeapVR.Shell.Domain.Models.Input.XInput;
using LeapVR.Shell.Modules.XInput;
using NLog;

namespace LeapVR.Shell.UI.Core.Triggers
{
    public class XButtonTrigger : TriggerBase<UIElement>
    {
        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public XInputButtons XButton
        {
            get => (XInputButtons)GetValue(XButtonProperty);
            set => SetValue(XButtonProperty, value);
        }
        // Using a DependencyProperty as the backing store for XButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty XButtonProperty =
            DependencyProperty.Register("XButton", typeof(XInputButtons), typeof(XButtonTrigger));

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(XButtonTrigger), new PropertyMetadata(true));


        #endregion

        #region Constructors

        #endregion

        #region Methods

        protected virtual void OnGamepadButtonStateChanged(XInputEventArgs args)
        {
            if (!IsEnabled) return;

            if (!(FocusManager.GetFocusScope(AssociatedObject) is UIElement focusScope))
            {
                Logger.Debug($"Try to trigger actions but failed due to {nameof(AssociatedObject)} is not a focus scope.");
                return;
            }

            if (!focusScope.IsKeyboardFocusWithin)
            {
                Logger.Debug($"Try to trigger actions but failed due to keyboard focus is not within {nameof(AssociatedObject)}.");
                return;
            }

            if (args.XButton == XButton && args.XButtonState == XInputButtonState.Pressed)
            {
                InvokeActions(args);
                Logger.Debug($"Actions triggered by XInput button {args.XButton}, State: {args.XButtonState}");
                return;
            }
        }

        #endregion
    }
}
