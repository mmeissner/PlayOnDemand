#region Licence
/****************************************************************
 *  Filename: ControllerKeyActionWatch.cs
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
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Extensions;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Input.OpenVR;
using LeapVR.Shell.Modules.Interfaces.Vr;

// ReSharper disable NotAccessedField.Local

namespace LeapVR.Shell.Controllers.Behavior
{
    public class ControllerKeyActionWatch : IControllerKeyActionWatch
    {
        public IControllerKeyAction KeyAction { get; }

        private readonly object _lock = new object();
        public IControllerKeyState State { get; private set; }
        private readonly BehaviorSubject<IControllerKeyState> _whenStateChangedSubject;
        public IObservable<IControllerKeyState> WhenStateChanged { get; }

        public bool IsSatisfied { get; private set; }
        private readonly BehaviorSubject<bool> _whenIsSatisfiedChangedSubject;
        public IObservable<bool> WhenIsSatisfiedChanged { get; }

        private CancellationTokenSource _cts;
        private Task _scheduledSatisfactionCheckTask;

        private readonly IDisposable _hmdActivityWatchdobEventSubscription;
        private readonly IHmdActivityWatchdog _hmdActivityWatchdog;

        internal ControllerKeyActionWatch(IHmdActivityWatchdog hmdActivityWatchdog, IControllerKeyAction keyAction)
        {
            QuickLeap.AssertNotNull(hmdActivityWatchdog, keyAction);
            _hmdActivityWatchdog = hmdActivityWatchdog;
            KeyAction = keyAction;

            State = new ControllerKeyState
            {
                State = ButtonState.Idle,
                InStateSince = DateTime.UtcNow,
            };
            _whenStateChangedSubject = new BehaviorSubject<IControllerKeyState>(State);
            WhenStateChanged = _whenStateChangedSubject.AsObservable();

            IsSatisfied = false;
            _whenIsSatisfiedChangedSubject = new BehaviorSubject<bool>(IsSatisfied);
            WhenIsSatisfiedChanged = _whenIsSatisfiedChangedSubject.AsObservable();

            _hmdActivityWatchdobEventSubscription = hmdActivityWatchdog.WhenEventOccures
                .OfType<IControllerButtonActionEvent>()
                .Where(q => q.ControllerRole == keyAction.ControlerRole)
                .Where(q => q.ButtonId == keyAction.ButtonId)
                .Subscribe(OnButtonActionEvent);
        }

        public void Dispose()
        {
            _hmdActivityWatchdobEventSubscription.Dispose();
            _whenStateChangedSubject.OnCompleted();
            _whenIsSatisfiedChangedSubject.OnCompleted();
        }

        private static bool ShouldTransitState(ButtonState currentState, ControllerButtonAction buttonAction, out ButtonState newState)
        {
            switch (buttonAction)
            {
                case ControllerButtonAction.Touch:
                    if (new[] { ButtonState.Idle }.Contains(currentState))
                    {
                        newState = ButtonState.Touched;
                        return true;
                    }
                    break;

                case ControllerButtonAction.Press:
                    if (new[] { ButtonState.Idle, ButtonState.Touched }.Contains(currentState))
                    {
                        newState = ButtonState.Pressed;
                        return true;
                    }
                    break;

                case ControllerButtonAction.UnPress:
                    if (new[] { ButtonState.Pressed }.Contains(currentState))
                    {
                        newState = ButtonState.Touched;
                        return true;
                    }
                    break;

                case ControllerButtonAction.UnTouch:
                    if (new[] { ButtonState.Touched, ButtonState.Pressed }.Contains(currentState))
                    {
                        newState = ButtonState.Idle;
                        return true;
                    }
                    break;
            }

            newState = ButtonState.Unknown;
            return false;
        }

        private void OnButtonActionEvent(IControllerButtonActionEvent newEvent)
        {
            //this.LogDebug($"OnButtonActionEvent: ControllerRole = `{newEvent.ControllerRole}`, ButtonId = `{newEvent.ButtonId}`, Action = `{newEvent.Action}`.");

            bool? newIsSatisfied = null;
            IControllerKeyState newState;
            lock (_lock)
            {
                if (!ShouldTransitState(State.State, newEvent.Action, out var newStateValue))
                {
                    return;
                }

                newState = new ControllerKeyState
                {
                    State = newStateValue,
                    InStateSince = DateTime.UtcNow,
                };
                State = newState;

                var isAllSatisfied = CheckIsSatisfied(out var isStateSatisfied, out var isTimeSatisfied);
                if (isAllSatisfied != IsSatisfied)
                {
                    newIsSatisfied = isAllSatisfied;
                    IsSatisfied = isAllSatisfied;
                }

                //this.LogDebug($"OnButtonActionEvent[1]: isAllSatisfied = `{isAllSatisfied}`, isStateSatisfied = `{isStateSatisfied}`, isTimeSatisfied = `{isTimeSatisfied}`.");

                CancelCurrentSatisfactionCheckIfRunning();
                if (isStateSatisfied && !isTimeSatisfied)
                {
                    var now = DateTime.UtcNow;
                    var inStateFor = now - State.InStateSince;
                    var canBeSatisfiedIn = QuickLeap.Max(KeyAction.TriggerTime - inStateFor, TimeSpan.Zero);
                    //this.LogDebug($"now = `{now}`, State.InStateSince = `{State.InStateSince}`, inStateFor = `{inStateFor}`, KeyAction.TriggerTime = `{KeyAction.TriggerTime}`, canBeSatisfiedIn = `{canBeSatisfiedIn}`.");

                    ScheduleSatisfactionCheck(canBeSatisfiedIn + TimeSpan.FromMilliseconds(25));
                }
            }

            NotifyStateChanged(newState);

            if (newIsSatisfied is bool isSatisfied)
            {
                NotifyIsSatisfiedChanged(isSatisfied);
            }
        }

        private bool CheckIsSatisfied(out bool isStateSatisfied, out bool isTimeSatisfied)
        {
            // must be executed in lock(_lock) context

            var now = DateTime.UtcNow;
            isStateSatisfied = State.State == KeyAction.State;
            isTimeSatisfied = now >= State.InStateSince + KeyAction.TriggerTime;
            return isStateSatisfied && isTimeSatisfied;
        }

        private void NotifyIsSatisfiedChanged(bool newValue)
        {
            _whenIsSatisfiedChangedSubject.OnNext(newValue);
        }

        private void NotifyStateChanged(IControllerKeyState newValue)
        {
            _whenStateChangedSubject.OnNext(State);
        }

        private void ScheduleSatisfactionCheck(TimeSpan delay)
        {
            // must be executed in lock(_lock) context

            _cts = new CancellationTokenSource();

            async Task SatisfactionCheckAsync()
            {
                try
                {
                    var ct = _cts.Token;
                    //this.LogDebug($"SatisfactionCheckAsync awaiting Task.Delay({delay})...");
                    await Task.Delay(delay, ct);
                    //this.LogDebug($"SatisfactionCheckAsync AFTER Task.Delay({delay}).");

                    bool? newIsSatisfied = null;
                    lock (_lock)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            //this.LogDebug($"SatisfactionCheckAsync ct.IsCancellationRequested.");
                            return;
                        }

                        var isAllSatisfied = CheckIsSatisfied(out var isStateSatisfied, out var isTimeSatisfied);
                        if (isAllSatisfied != IsSatisfied)
                        {
                            newIsSatisfied = isAllSatisfied;
                            IsSatisfied = isAllSatisfied;
                        }

                        //this.LogDebug($"OnButtonActionEvent[2]: isSatisfied = `{isAllSatisfied}`, isStateSatisfied = `{isStateSatisfied}`, isTimeSatisfied = `{isTimeSatisfied}`.");
                    }

                    if (newIsSatisfied is bool isSatisfied)
                    {
                        NotifyIsSatisfiedChanged(isSatisfied);
                    }
                }
                catch (TaskCanceledException)
                {
                    //this.LogDebug($"SatisfactionCheckAsync TaskCanceledException occured.");
                    // do nothing, allow to finish by itself
                }
            }

            _scheduledSatisfactionCheckTask = SatisfactionCheckAsync().Forget();
        }

        private void CancelCurrentSatisfactionCheckIfRunning()
        {
            // must be executed in lock(_lock) context

            _cts?.Cancel();
        }
    }
}
