#region Licence
/****************************************************************
 *  Filename: XInputBinding.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using LeapVR.Shell.Domain.Models.Input;
using LeapVR.Shell.Domain.Models.Input.XInput;

namespace LeapVR.Shell.Modules.XInput
{
    public class XInputBinding : InputBinding
    {
        public static readonly DependencyProperty ButtonProperty =
            DependencyProperty.Register("Button", typeof(XInputButtons), typeof(XInputBinding), new UIPropertyMetadata(XInputButtons.None, new PropertyChangedCallback(OnButtonPropertyChanged)));

        public XInputButtons Button
        {
            get => (XInputButtons)GetValue(ButtonProperty);
            set => SetValue(ButtonProperty, value);
        }

        public override InputGesture Gesture
        {
            get => base.Gesture as XInputGesture;
            set
            {
                var gesture = value as XInputGesture;
                base.Gesture = gesture;
            }
        }

        private static void OnButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var binding = (XInputBinding)d;
            binding.Gesture = new XInputGesture((XInputButtons)e.NewValue);
        }
    }

}
