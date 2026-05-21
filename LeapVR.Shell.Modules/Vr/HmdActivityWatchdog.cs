#region Licence
/****************************************************************
 *  Filename: HmdActivityWatchdog.cs
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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LeapVR.OpenVR.Wrapper;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Modules.Interfaces.Vr;
using NLog;

namespace LeapVR.Shell.Modules.Vr
{
    public class HmdActivityWatchdog : IHmdActivityWatchdog
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<EVREventType, HmdActivityStatus> HmdActivityStatusMap = new Dictionary<EVREventType, HmdActivityStatus>
        {
            { EVREventType.VREvent_TrackedDeviceUserInteractionStarted, HmdActivityStatus.Active },
            { EVREventType.VREvent_TrackedDeviceUserInteractionEnded, HmdActivityStatus.Inactive },
        };

        private static readonly Dictionary<ETrackedControllerRole, ControllerSide> ControllerSideMap = new Dictionary<ETrackedControllerRole, ControllerSide>
        {
            { ETrackedControllerRole.LeftHand, ControllerSide.LeftHand },
            { ETrackedControllerRole.RightHand, ControllerSide.RightHand },
        };

        private static readonly Dictionary<EVRButtonId, ControllerButton> ControllerButtonMap = new Dictionary<EVRButtonId, ControllerButton>
        {
            {EVRButtonId.k_EButton_Axis0, ControllerButton.Touchpad },
            {EVRButtonId.k_EButton_Axis1, ControllerButton.Trigger },
            {EVRButtonId.k_EButton_ApplicationMenu, ControllerButton.ApplicationMenu },
            {EVRButtonId.k_EButton_Dashboard_Back, ControllerButton.Grip },
        };

        private static readonly Dictionary<EVREventType, ControllerButtonAction> ControllerButtonActionMap = new Dictionary<EVREventType, ControllerButtonAction>
        {
            { EVREventType.VREvent_ButtonPress, ControllerButtonAction.Press },
            { EVREventType.VREvent_ButtonUnpress, ControllerButtonAction.UnPress },
            { EVREventType.VREvent_ButtonTouch, ControllerButtonAction.Touch },
            { EVREventType.VREvent_ButtonUntouch, ControllerButtonAction.UnTouch },
        };

        private readonly Subject<IOpenVrEvent> _whenEventOccuresSubject;
        public IObservable<IOpenVrEvent> WhenEventOccures { get; }

        private int _isDisposed; // 0 = false, 1 = true
        private CancellationTokenSource _cts;

        private CVRSystem _system;

        public bool HasError { get; private set; }

        internal HmdActivityWatchdog()
        {
            if(!Initialize())
            {
                HasError = true;
                return;
            }
        
            _whenEventOccuresSubject = new Subject<IOpenVrEvent>();
            WhenEventOccures = _whenEventOccuresSubject.AsObservable();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Task.Factory.StartNew(() => GetEvents(token), token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Dispose()
        {
            var wasDisposed = QuickLeap.OperateInterlockedFlag(ref _isDisposed, true);
            if (wasDisposed)
            {
                return;
            }

            if(!HasError)
            {
                _cts.Cancel();
                _whenEventOccuresSubject.OnCompleted();
                OpenVR.Wrapper.OpenVR.Shutdown();
            }
        }

        private bool Initialize()
        {
            EVRInitError initError = EVRInitError.Unknown;
            _system = OpenVR.Wrapper.OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Background);

            if (initError != EVRInitError.None || _system == null)
            {
                return false;
            }
            return true;
        }

        private void GetEvents(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(25);
                    token.ThrowIfCancellationRequested();
                    var vrEvent = default(VREvent_t);
                    var isNewEvent = _system.PollNextEvent(ref vrEvent, (uint)Marshal.SizeOf(vrEvent));
                    if (isNewEvent)
                    {
                        HandleEvent(ref vrEvent);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // swallow, let loop finish by itself
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private void HandleEvent(ref VREvent_t vrEvent)
        {
            IOpenVrEvent newEvent = null;

            switch (vrEvent.eventType)
            {
                case (uint)EVREventType.VREvent_TrackedDeviceUserInteractionStarted:
                case (uint)EVREventType.VREvent_TrackedDeviceUserInteractionEnded:
                    if (vrEvent.trackedDeviceIndex == 0) // 0 == HMD
                    {
                        newEvent = GetHmdActivityEvent(ref vrEvent);
                    }
                    break;

                case (uint)EVREventType.VREvent_ButtonPress:
                case (uint)EVREventType.VREvent_ButtonUnpress:
                case (uint)EVREventType.VREvent_ButtonTouch:
                case (uint)EVREventType.VREvent_ButtonUntouch:
                    newEvent = GetControllerButtonActionEvent(ref vrEvent);
                    break;
            }

            if (newEvent != null)
            {
                //                if (newEvent is IControllerButtonActionEvent c)
                //                {
                //                    this.LogDebug($"NEW CONTROLLER BUTTON EVENT: ControllerRole = `{c.ControllerRole}`, ButtonId = `{c.ButtonId}`, Action =`{c.Action}`.");
                //                }

                _whenEventOccuresSubject.OnNext(newEvent);
            }
        }

        private IHmdActivityEvent GetHmdActivityEvent(ref VREvent_t vrEvent)
        {
            HmdActivityStatusMap.TryGetValue((EVREventType)vrEvent.eventType, out var status);

            return new HmdActivityEvent
            {
                Status = status,
            };
        }

        private IControllerButtonActionEvent GetControllerButtonActionEvent(ref VREvent_t vrEvent)
        {
            var isSuccessfulyMaped = ControllerButtonActionMap.TryGetValue((EVREventType)vrEvent.eventType, out var action);
            if (!isSuccessfulyMaped)
            {
                return null;
            }

            var controllerRole = _system.GetControllerRoleForTrackedDeviceIndex(vrEvent.trackedDeviceIndex);
            return new ControllerButtonActionEvent
            {
                ControllerRole = (uint)controllerRole,
                ButtonId = vrEvent.data.controller.button,
                Action = action,
            };
        }
    }
}
