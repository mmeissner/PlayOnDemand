#region Licence
/****************************************************************
 *  Filename: KeyPad.cs
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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shared.Lib.Win.WinApi.Win32;

namespace LeapVR.Shell.UI.Usercontrols
{
    [TemplatePart(Name = PART_KeyPanel, Type = typeof(Border))]
    public class KeyPad : UserControl
    {
        private const string PART_KeyPanel = "PART_KeyPanel";
        private const string PART_InputElement = "PART_InputElement";


        public static readonly RoutedEvent InputCompleteEvent = EventManager.RegisterRoutedEvent("InputComplete",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>),
            typeof(KeyPad));

        public event RoutedPropertyChangedEventHandler<string> InputComplete
        {
            add => AddHandler(InputCompleteEvent, value);
            remove => RemoveHandler(InputCompleteEvent, value);
        }

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>),
            typeof(KeyPad));

        public event RoutedPropertyChangedEventHandler<string> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }
        // Using a DependencyProperty as the backing store for DisplayText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register("DisplayText", typeof(string), typeof(KeyPad), new FrameworkPropertyMetadata(string.Empty));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        // Using a DependencyProperty as the backing store for Result.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(KeyPad), new FrameworkPropertyMetadata(string.Empty));

        public string Result
        {
            get => (string)GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }
        // Using a DependencyProperty as the backing store for Result.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(string), typeof(KeyPad), new FrameworkPropertyMetadata(string.Empty));

        public Visibility DisplayAreaVisibility
        {
            get => (Visibility)GetValue(DisplayAreaVisibilityProperty);
            set => SetValue(DisplayAreaVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for DisplayAreaVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayAreaVisibilityProperty =
            DependencyProperty.Register("DisplayAreaVisibility", typeof(Visibility), typeof(KeyPad));

        private Border _keyPanel;

        public override void OnApplyTemplate()
        {
            Focusable = true;

            _keyPanel = GetTemplateChild(PART_KeyPanel) as Border;
            if (_keyPanel != null)
            {
                _keyPanel.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Button_Click));
                _keyPanel.AddHandler(Border.KeyUpEvent, new KeyEventHandler(Panel_KeyUp));

                InputMethod.SetIsInputMethodEnabled(_keyPanel, false);
                FocusManager.SetIsFocusScope(_keyPanel, true);
                Keyboard.Focus(_keyPanel);
            }
            base.OnApplyTemplate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sourceButton = e.OriginalSource as Button;
            if (sourceButton == null)
            {
                return;
            }
            var tag = sourceButton.Tag?.ToString();

            if (Enum.TryParse<Key>(tag, out var key))
            {
                UpdateInput(key);
            }
        }

        private void Panel_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateInput(e.Key);
            e.Handled = true;
        }

        private void UpdateInput(Key key)
        {
            RoutedPropertyChangedEventArgs<string> eventArgs = null;
            switch (key)
            {
                case Key.NumPad0:
                case Key.NumPad1:
                case Key.NumPad2:
                case Key.NumPad3:
                case Key.NumPad4:
                case Key.NumPad5:
                case Key.NumPad6:
                case Key.NumPad7:
                case Key.NumPad8:
                case Key.NumPad9:
                case Key.D0:
                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                case Key.D8:
                case Key.D9:
                    if (Value.Length < 6)
                    {
                        var character = GetCharFromKey(key);
                        Value += character;
                        eventArgs = new RoutedPropertyChangedEventArgs<string>(Value, Value, ValueChangedEvent);
                        RaiseEvent(eventArgs);
                    }
                    break;
                case Key.Clear:
                case Key.Escape:
                    Value = string.Empty;
                    eventArgs = new RoutedPropertyChangedEventArgs<string>(Value, Value, ValueChangedEvent);
                    RaiseEvent(eventArgs);
                    break;
                case Key.Back:
                case Key.Delete:
                    if (Value.Length > 0)
                    {
                        Value = Value.Remove(Value.Length - 1);
                        eventArgs = new RoutedPropertyChangedEventArgs<string>(Value, Value, ValueChangedEvent);
                        RaiseEvent(eventArgs);
                    }
                    break;
                case Key.Enter:
                    Result = new string(Value.ToCharArray());
                    eventArgs = new RoutedPropertyChangedEventArgs<string>(Result, Result, InputCompleteEvent);
                    Value = string.Empty;
                    RaiseEvent(eventArgs);
                    break;
            }
        }

        private static char GetCharFromKey(Key key)
        {
            var ch = ' ';

            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];
            User32.GetKeyboardState(keyboardState);

            var scanCode = User32.MapVirtualKey((uint)virtualKey, User32.KeyboardKeyMapType.MAPVK_VK_TO_VSC);
            var stringBuilder = new StringBuilder(2);

            var result = User32.ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                {
                    ch = stringBuilder[0];
                    break;
                }
                default:
                {
                    ch = stringBuilder[0];
                    break;
                }
            }
            return ch;
        }
    }
}
