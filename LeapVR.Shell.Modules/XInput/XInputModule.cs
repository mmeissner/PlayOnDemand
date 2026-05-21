#region Licence
/****************************************************************
 *  Filename: XInputModule.cs
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Input.XInput;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces.XInput;
using NLog;
using XInputDotNetPure;

namespace LeapVR.Shell.Modules.XInput
{
    
    public class XInputModule : IXInputModule
    {
        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<XInputButtons, ButtonState> _prevStates = new Dictionary<XInputButtons, ButtonState>()
        {
            {  XInputButtons.A, ButtonState.Released },
            {  XInputButtons.B, ButtonState.Released },
            {  XInputButtons.Back, ButtonState.Released },
            {  XInputButtons.DPadDown, ButtonState.Released },
            {  XInputButtons.DPadLeft, ButtonState.Released },
            {  XInputButtons.DPadRight, ButtonState.Released },
            {  XInputButtons.DPadUp, ButtonState.Released },
            {  XInputButtons.Guide, ButtonState.Released },
            {  XInputButtons.LeftShoulder, ButtonState.Released },
            {  XInputButtons.LeftStick, ButtonState.Released },
            {  XInputButtons.LeftStickDown, ButtonState.Released },
            {  XInputButtons.LeftStickLeft, ButtonState.Released },
            {  XInputButtons.LeftStickRight, ButtonState.Released },
            {  XInputButtons.LeftStickUp, ButtonState.Released },
            {  XInputButtons.RightShoulder, ButtonState.Released },
            {  XInputButtons.RightStick, ButtonState.Released },
            {  XInputButtons.RightStickDown, ButtonState.Released },
            {  XInputButtons.RightStickLeft, ButtonState.Released },
            {  XInputButtons.RightStickRight, ButtonState.Released },
            {  XInputButtons.RightStickUp, ButtonState.Released },
            {  XInputButtons.Start, ButtonState.Released },
            {  XInputButtons.TriggerLeft, ButtonState.Released },
            {  XInputButtons.TriggerRight, ButtonState.Released },
            {  XInputButtons.X, ButtonState.Released },
            {  XInputButtons.Y, ButtonState.Released }
        };
        private readonly Dictionary<XInputButtons, InputState> _keyWatches = new Dictionary<XInputButtons, InputState>()
        {
            {  XInputButtons.A, new InputState() },
            {  XInputButtons.B, new InputState() },
            {  XInputButtons.Back, new InputState() },
            {  XInputButtons.DPadDown, new InputState() },
            {  XInputButtons.DPadLeft, new InputState() },
            {  XInputButtons.DPadRight, new InputState() },
            {  XInputButtons.DPadUp, new InputState() },
            {  XInputButtons.Guide, new InputState() },
            {  XInputButtons.LeftShoulder, new InputState() },
            {  XInputButtons.LeftStick, new InputState() },
            {  XInputButtons.LeftStickDown, new InputState() },
            {  XInputButtons.LeftStickLeft, new InputState() },
            {  XInputButtons.LeftStickRight, new InputState() },
            {  XInputButtons.LeftStickUp, new InputState() },
            {  XInputButtons.RightShoulder, new InputState() },
            {  XInputButtons.RightStick, new InputState() },
            {  XInputButtons.RightStickDown, new InputState() },
            {  XInputButtons.RightStickLeft, new InputState() },
            {  XInputButtons.RightStickRight, new InputState() },
            {  XInputButtons.RightStickUp, new InputState() },
            {  XInputButtons.Start, new InputState() },
            {  XInputButtons.TriggerLeft, new InputState() },
            {  XInputButtons.TriggerRight, new InputState() },
            {  XInputButtons.X, new InputState() },
            {  XInputButtons.Y, new InputState() }
        };
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Subject<XInputButtonArgs> _whenXInputButtonStateChangedSubject = new Subject<XInputButtonArgs>();
        private readonly Stopwatch _throttleTimer = new Stopwatch();
        private readonly Stopwatch _timer = new Stopwatch();

        private readonly int _resendDelay;
        private readonly int _resendRate;
        private readonly string _xInputCompositeButtonsForForceQuitingApps;
        private readonly int _millisecondsToWaitBeforeCompositeButtonsThrottleReOpen;
        private readonly int _millisecondsToHoldBeforeCompositeButtonsTakeEffect;
        private int _pollingInterval;
        private volatile bool _isMonitoring;
        private volatile bool _inProgress;
        private readonly object _locker = new object();

        private Thread _workThread;
        private volatile bool _doWork;
        #endregion

        #region Public Methods
        public Guid ModuleId => Guid.Parse("361012D0-6269-4CD4-92CE-F4A8B21E589A");
        public string ModuleName => "XInput Module";
        public IObservable<XInputButtonArgs> WhenXInputButtonStateChanged => _whenXInputButtonStateChangedSubject.AsObservable();
        #endregion

        #region Constructors
        public XInputModule(IConfigFileRepository<XInputModuleConfig> xinputModuleConfig)
        {
            QuickLeap.AssertNotNull(xinputModuleConfig);
            var config = xinputModuleConfig.Get();
            _pollingInterval = config.XInputDevicePollingPerSecond;
            _resendDelay = config.XInputDeviceResendDelay;
            _resendRate = config.XInputDeviceResendEachMs;
            _millisecondsToWaitBeforeCompositeButtonsThrottleReOpen =config.MillisecondsToWaitBeforeCompositeButtonsThrottleReOpen;
            _millisecondsToHoldBeforeCompositeButtonsTakeEffect = config.MillisecondsToHoldBeforeCompositeButtonsTakeEffect;
            _xInputCompositeButtonsForForceQuitingApps = config.XInputCompositeButtonsForForceQuitingApps;
        }
        #endregion

        public bool Enabled
        {
            get
            {
                lock (_locker)return _isMonitoring;
            }
            set
            {
                lock (_locker)
                {
                    if (_isMonitoring == value) return;
                    if (_inProgress)return;
                    _inProgress = true;
                    if(value)
                    {
                        Logger.Debug("Starting XInput Module");
                        Start();
                    }
                    else
                    {
                        Logger.Debug("Stopping XInput Module");
                        Stop();
                    }
                }
            } 
        }

        public Action RequestAllAppTermination { get; set; }

        public Action ResetStationState { get; set; }

        #region Methods
        private void Start()
        {
            try
            {
                _doWork = true;
                _workThread = new Thread(XInputMonitoring);
                _workThread.IsBackground = true;
                _workThread.Name = "XInput Poller";
                _workThread.Start();
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Error during Start of XInputMonitoring");
                throw;
            }
        }
        private void Stop()
        {
            _doWork = false;
            _workThread.Join(TimeSpan.FromSeconds(3));
        }

        private void XInputMonitoring()
        {
            Logger.Trace("XInput Monitoring Entered");
            var possiblePlayerIndexes = new[]
                    {
                    PlayerIndex.One,
                    PlayerIndex.Two,
                    PlayerIndex.Three,
                    PlayerIndex.Four
                 };
            try
            {
                _isMonitoring = true;
                _inProgress = false;
                Logger.Debug("XInput Monitoring Loop Started");
                var noGamePadTimeToWait = TimeSpan.FromSeconds(1);
                if(_pollingInterval <= 0) _pollingInterval = 25;
                var pollingInterval = TimeSpan.FromMilliseconds(Convert.ToInt32(1000 / _pollingInterval));
                while (_doWork)
                {
                    var state = GamePad.GetState(PlayerIndex.One);
                    foreach (var index in possiblePlayerIndexes)
                    {
                        var possibleGamepadState = GamePad.GetState(index);
                        if (!possibleGamepadState.IsConnected) continue;
                        state = possibleGamepadState;
                        break;
                    }

                    if (!state.IsConnected)
                    {
                        Logger.Trace("No Gamepad Connected!");
                        Thread.Sleep(noGamePadTimeToWait);
                        continue;
                    }
                    Logger.Trace("Processing Button States!");
                    ProcessButtonState(state.Buttons.A, XInputButtons.A);
                    ProcessButtonState(state.Buttons.B, XInputButtons.B);
                    ProcessButtonState(state.Buttons.Back, XInputButtons.Back);
                    ProcessButtonState(state.Buttons.Guide, XInputButtons.Guide);
                    ProcessButtonState(state.Buttons.LeftShoulder, XInputButtons.LeftShoulder);
                    ProcessButtonState(state.Buttons.LeftStick, XInputButtons.LeftStick);
                    ProcessButtonState(state.Buttons.RightShoulder, XInputButtons.RightShoulder);
                    ProcessButtonState(state.Buttons.RightStick, XInputButtons.RightStick);
                    ProcessButtonState(state.Buttons.Start, XInputButtons.Start);
                    ProcessButtonState(state.Buttons.X, XInputButtons.X);
                    ProcessButtonState(state.Buttons.Y, XInputButtons.Y);
                    ProcessButtonState(state.DPad.Down, XInputButtons.DPadDown);
                    ProcessButtonState(state.DPad.Left, XInputButtons.DPadLeft);
                    ProcessButtonState(state.DPad.Right, XInputButtons.DPadRight);
                    ProcessButtonState(state.DPad.Up, XInputButtons.DPadUp);
                    ProcessAxisState(state.Triggers.Left, XInputButtons.TriggerLeft, true);
                    ProcessAxisState(state.Triggers.Right, XInputButtons.TriggerRight, true);
                    ProcessAxisState(state.ThumbSticks.Left.X, XInputButtons.LeftStickLeft, false);
                    ProcessAxisState(state.ThumbSticks.Left.X, XInputButtons.LeftStickRight, true);
                    ProcessAxisState(state.ThumbSticks.Left.Y, XInputButtons.LeftStickUp, true);
                    ProcessAxisState(state.ThumbSticks.Left.Y, XInputButtons.LeftStickDown, false);
                    ProcessAxisState(state.ThumbSticks.Right.X, XInputButtons.RightStickLeft, false);
                    ProcessAxisState(state.ThumbSticks.Right.X, XInputButtons.RightStickRight, true);
                    ProcessAxisState(state.ThumbSticks.Right.Y, XInputButtons.RightStickUp, true);
                    ProcessAxisState(state.ThumbSticks.Right.Y, XInputButtons.RightStickDown, false);
                    Thread.Sleep(pollingInterval);
                }
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception,"XInput module faced unrecoverable exception!");
                throw;
            }
            finally
            {
                _inProgress = false;
                _isMonitoring = false;
            }
        }

        /// <summary>
        /// Check whether to send input information again or keep the state as it is.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private bool ShouldResendKey(XInputButtons button)
        {
            var state = _keyWatches[button];
            var shouldResendKey = false;
            var elapsed = state.Watch.ElapsedMilliseconds;
            if (!state.Watch.IsRunning)
            {
                shouldResendKey = true;
                state.Watch.Start();
            }
            else
            {
                if (!state.IsReSending && elapsed > _resendDelay)
                {
                    state.IsReSending = true;
                    state.Watch.Restart();
                    shouldResendKey = true;
                }
                else
                {
                    if (state.IsReSending && elapsed > _resendRate)
                    {
                        state.Watch.Restart();
                        shouldResendKey = true;
                    }
                }
            }

            return shouldResendKey;
        }
        private void ResetButtonResend(XInputButtons button)
        {
            var state = _keyWatches[button];
            state.Watch.Reset();
            state.IsReSending = false;
        }
        private void ProcessButtonState(ButtonState currentState, XInputButtons button)
        {
            switch (currentState)
            {
                case ButtonState.Pressed when ShouldResendKey(button):
                    SendXInput(button, true);
                    _prevStates[button] = ButtonState.Pressed;
                    break;
                case ButtonState.Released when _prevStates[button] == ButtonState.Pressed:
                    ResetButtonResend(button);
                    SendXInput(button, false);
                    _prevStates[button] = ButtonState.Released;
                    break;
            }
            ProcessCompositeButtonState();
        }
        private void ProcessAxisState(float currentState, XInputButtons button, bool positive)
        {
            if (positive)
            {
                if (currentState > 0.5f && ShouldResendKey(button))
                {
                    SendXInput(button, true);
                    _prevStates[button] = ButtonState.Pressed;
                }
                else if (currentState < 0.5f && _prevStates[button] == ButtonState.Pressed)
                {
                    ResetButtonResend(button);
                    SendXInput(button, false);
                    _prevStates[button] = ButtonState.Released;
                }
            }
            else
            {
                if (currentState < -0.5f && ShouldResendKey(button))
                {
                    SendXInput(button, true);
                    _prevStates[button] = ButtonState.Pressed;
                }
                else if (currentState > -0.5f && _prevStates[button] == ButtonState.Pressed)
                {
                    ResetButtonResend(button);
                    SendXInput(button, false);
                    _prevStates[button] = ButtonState.Released;
                }
            }
        }
        private void ProcessCompositeButtonState()
        {
            var pressedButtons = (from buttonState in _prevStates
                                  let isResending = _keyWatches[buttonState.Key].IsReSending
                                  let watch = _keyWatches[buttonState.Key].Watch
                                  where buttonState.Value == ButtonState.Pressed && isResending
                                  select new XInputButtonArgs(buttonState.Key, buttonState.Value == ButtonState.Released ? XInputButtonState.Released : XInputButtonState.Pressed, watch)).ToArray();

            if (pressedButtons.Length <= 0)
            {
                return;
            }
            var args = new XInputCompositeArgs(pressedButtons);
            OnGamepadCompositeButtonsStateChanged(args);
            Logger.Trace($"XInput button '[{string.Join(", ", pressedButtons.Select(b => b.XButton.ToString()))}]' buttons pressed.");
        }

        /// <summary>
        /// Wrap XInput information to <see cref="EventArgs"/> and raise Event.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="pressed"></param>
        private void SendXInput(XInputButtons button, bool pressed)
        {
            var xInputButtonArgs = new XInputButtonArgs(button, pressed ? XInputButtonState.Pressed : XInputButtonState.Released);
            Logger.Trace($"Publishing Changed Gamepad State with Button={xInputButtonArgs.XButton}, State={xInputButtonArgs.XButtonState}!");
            _whenXInputButtonStateChangedSubject.OnNext(xInputButtonArgs);
            var suffix = pressed ? "Pressed" : "Released";
            Logger.Trace( $"XInput button '{button}' {suffix}.");
        }

        private void OnGamepadCompositeButtonsStateChanged(XInputCompositeArgs args)
        {

            if (args.XButtons == null || args.XButtons.Length <= 0)
            {
                _timer.Reset();
                return;
            }

            if (_throttleTimer.IsRunning && _throttleTimer.Elapsed.TotalMilliseconds < _millisecondsToWaitBeforeCompositeButtonsThrottleReOpen)
            {
                _timer.Reset();
                return;
            }

            var expectedXButtons = ResolveExpectedButtons(_xInputCompositeButtonsForForceQuitingApps);

            if (expectedXButtons == null || expectedXButtons.Length <= 0 || !expectedXButtons.All(e => args.XButtons.Select(q => q.XButton).Contains(e)))
            {
                _timer.Reset();
                return;
            }
            if (!_timer.IsRunning)
            {
                _timer.Start();
            }

            var targetButtons = (from btn in args.XButtons.TakeWhile(a => expectedXButtons.Contains(a.XButton)) select btn).ToArray();

            if (_timer.Elapsed.TotalMilliseconds < _millisecondsToHoldBeforeCompositeButtonsTakeEffect) return;

            var collection = (from s in targetButtons select s.XButton.ToString()).ToArray();
            var btnStrings = string.Join(",", collection);

            Logger.Debug($"Buttons '{btnStrings}' pressed for {_timer.Elapsed}.");

            Logger.Debug($"Trying to forcely terminate running applications by holding gamepad buttons '[{btnStrings}]' for {_timer.Elapsed}.");
            RequestAllAppTermination?.Invoke();
            Logger.Info("Running applications forcely terminated.");

            _timer.Reset();
            _throttleTimer.Restart();
        }

        private XInputButtons[] ResolveExpectedButtons(string compositeButtonsString)
        {
            var btnStringArr = compositeButtonsString.Split(',');
            var expetedXButtons = new List<XInputButtons>();
            foreach (var xbuttonString in btnStringArr)
            {
                if (Enum.TryParse<XInputButtons>(xbuttonString, out var xbutton))
                {
                    expetedXButtons.Add(xbutton);
                }
            }
            return expetedXButtons.ToArray();
        }
        #endregion


    }
}
