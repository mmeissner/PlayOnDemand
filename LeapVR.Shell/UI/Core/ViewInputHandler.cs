#region Licence
/****************************************************************
 *  Filename: ViewInputHandler.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Input.XInput;
using LeapVR.Shell.UI.Interfaces;
using NLog;
using Execute = Caliburn.Micro.Execute;

namespace LeapVR.Shell.UI.Core
{
    public class ViewInputHandler : IViewInputHandler, IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IGamepadController _gamepadController;
        readonly ReaderWriterLockSlim _modelsLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        readonly HashSet<Guid> _exclusiveObjects = new HashSet<Guid>();
        readonly HashSet<Guid> _exclusiveGlobalObjects = new HashSet<Guid>();
        readonly HashSet<Guid> _activeObjects = new HashSet<Guid>();
        readonly Dictionary<object,Guid> _registeredObjects = new Dictionary<object, Guid>();
        readonly Dictionary<Guid, ViewGamepadHandler> _viewTypeActionMappings = new Dictionary<Guid, ViewGamepadHandler>();
        readonly Dictionary<ControllerInput,List<Guid>> _inputMapping = new Dictionary<ControllerInput, List<Guid>>();

        public bool IsControllerInputEnabled => _gamepadController.IsEnabled;

        public ViewInputHandler(IGamepadController gamepadController)
        {
            _gamepadController = gamepadController;
            _gamepadController.WhenXInputButtonStateChanged.Subscribe(OnGamepadInput, OnGamePadError);
        }

        private void OnGamePadError(Exception exception)
        {
            Logger.Fatal(exception,"Gamepad Controller subscribtion had an error, from now on there will be no gamepad inputs received!");
        }

        //Should be moved and mapped in module

        private void OnGamepadInput(XInputButtonArgs xInputButtonArgs)
        {
            //Only react on pressed
            Logger.Trace($"Input Detected, Button={xInputButtonArgs.XButton}, State={xInputButtonArgs.XButtonState}");
            if (xInputButtonArgs.XButtonState == XInputButtonState.Released) return;
            ControllerInput input = ControllerInput.None;
            switch (xInputButtonArgs.XButton)
            {
                case XInputButtons.None:
                    break;
                case XInputButtons.Start:
                    input = ControllerInput.Start;
                    break;
                case XInputButtons.Back:
                    input = ControllerInput.Back;
                    break;
                case XInputButtons.LeftStick:
                    input = ControllerInput.PushOne;
                    break;
                case XInputButtons.RightStick:
                    input = ControllerInput.PushTwo;
                    break;
                case XInputButtons.LeftShoulder:
                    input = ControllerInput.PreviousOne;
                    break;
                case XInputButtons.RightShoulder:
                    input = ControllerInput.NextOne;
                    break;
                case XInputButtons.Guide:
                    input = ControllerInput.Guide;
                    break;
                case XInputButtons.A:
                    input = ControllerInput.Accept;
                    break;
                case XInputButtons.B:
                    input = ControllerInput.Cancel;
                    break;
                case XInputButtons.X:
                    input = ControllerInput.XAction;
                    break;
                case XInputButtons.Y:
                    input = ControllerInput.YAction;
                    break;
                case XInputButtons.DPadLeft:
                    input = ControllerInput.DLeft;
                    break;
                case XInputButtons.DPadRight:
                    input = ControllerInput.DRight;
                    break;
                case XInputButtons.DPadUp:
                    input = ControllerInput.DUp;
                    break;
                case XInputButtons.DPadDown:
                    input = ControllerInput.DDown;
                    break;
                case XInputButtons.TriggerLeft:
                    input = ControllerInput.PreviousTwo;
                    break;
                case XInputButtons.TriggerRight:
                    input = ControllerInput.NextTwo;
                    break;
                case XInputButtons.LeftStickLeft:
                    input = ControllerInput.Left;
                    break;
                case XInputButtons.LeftStickRight:
                    input = ControllerInput.Right;
                    break;
                case XInputButtons.LeftStickUp:
                    input = ControllerInput.Up;
                    break;
                case XInputButtons.LeftStickDown:
                    input = ControllerInput.Down;
                    break;
                case XInputButtons.RightStickLeft:
                    input = ControllerInput.Left;
                    break;
                case XInputButtons.RightStickRight:
                    input = ControllerInput.Right;
                    break;
                case XInputButtons.RightStickUp:
                    input = ControllerInput.Up;
                    break;
                case XInputButtons.RightStickDown:
                    input = ControllerInput.Down;
                    break;
            }
            DoAction(input);
        }

        public void SetObjectExclusivity(object exclusiveObject, InputExclusivity exclusivity)
        {
            try
            {
                _modelsLockSlim.EnterUpgradeableReadLock();
                if (_registeredObjects.TryGetValue(exclusiveObject, out var id))
                {
                    if (_viewTypeActionMappings.TryGetValue(id, out var mapping))
                    {
                        try
                        {
                            _modelsLockSlim.EnterWriteLock();
                            Logger.Debug($"Setting Exclusive Object of Type= {exclusiveObject.GetType()}, Guid={id}");
                            mapping.SetExclusivity(exclusivity);
                            switch (exclusivity)
                            {
                                case InputExclusivity.Shared:
                                    _exclusiveObjects.Remove(id);
                                    _exclusiveGlobalObjects.Remove(id);
                                    break;
                                case InputExclusivity.RegisterdControllerInputs:
                                    _exclusiveObjects.Add(id);
                                    _exclusiveGlobalObjects.Remove(id);
                                    break;
                                case InputExclusivity.AllControllerInputs:
                                    _exclusiveObjects.Remove(id);
                                    _exclusiveGlobalObjects.Add(id);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(exclusivity), exclusivity, null);
                            }
                        }
                        finally
                        {
                            _modelsLockSlim.ExitWriteLock();
                        }
                    }
                }
                else
                {
                    Logger.Warn("Can not set object exclusivity if its not registered");
                }
            }
            finally
            {
                _modelsLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void Register(object self, InputExclusivity exclusivity)
        {
            try
            {
                _modelsLockSlim.EnterUpgradeableReadLock();
                if (_registeredObjects.ContainsKey(self)) return;
                try
                {
                    _modelsLockSlim.EnterWriteLock();
                    var viewTypeMapping = new ViewGamepadHandler(self, exclusivity);
                    Logger.Debug($"Registering new Object of Type= {viewTypeMapping.ViewType}, Guid={viewTypeMapping.Id}, ExclusiveType={viewTypeMapping.Exclusivity}");
                    _viewTypeActionMappings.Add(viewTypeMapping.Id, viewTypeMapping);
                    _registeredObjects.Add(self, viewTypeMapping.Id);
                    switch (exclusivity)
                    {
                        case InputExclusivity.RegisterdControllerInputs:
                            _exclusiveObjects.Add(viewTypeMapping.Id);
                            break;
                        case InputExclusivity.AllControllerInputs:
                            _exclusiveGlobalObjects.Add(viewTypeMapping.Id);
                            break;
                    }
                }
                finally
                {
                    _modelsLockSlim.ExitWriteLock();
                }
            }
            finally
            {
                _modelsLockSlim.ExitUpgradeableReadLock();
            }
        }

        public  void AddAction(object self, ControllerInput input, Action action)
        {
            try
            {
                _modelsLockSlim.EnterUpgradeableReadLock();
                if (_registeredObjects.TryGetValue(self, out var id))
                {
                    if (_viewTypeActionMappings.TryGetValue(id, out var mapping))
                    {
                        try
                        {
                            _modelsLockSlim.EnterWriteLock();
                            mapping.AddAction(input, action);
                            Logger.Debug($"Adding Action for ControllerInput={input}, for object with Id={mapping.Id}");
                            if (_inputMapping.TryGetValue(input, out var idsList))
                            {
                                idsList.Add(id);
                            }
                            else
                            {
                                _inputMapping.Add(input, new List<Guid> {id});
                            }
                        }
                        finally
                        {
                            _modelsLockSlim.ExitWriteLock();
                        }
                    }
                    else
                    {
                        Logger.Error("Could not find ActionMapping for an registered Object!");
                    }
                }
                else
                {
                    Logger.Error("Can not Add Action to an object that is not registered");
                }
            }
            finally
            {
                _modelsLockSlim.ExitUpgradeableReadLock();
            }
        }

        public  void ClearActions(object self)
        {
            try
            {
                _modelsLockSlim.EnterUpgradeableReadLock();
                if (_registeredObjects.TryGetValue(self, out var id))
                {
                    if (_viewTypeActionMappings.TryGetValue(id, out var mapping))
                    {
                        try
                        {
                            _modelsLockSlim.EnterWriteLock();
                            Logger.Debug($"Clearing all Actions for object with Id={mapping.Id}");
                            mapping.ClearActions();
                            foreach (KeyValuePair<ControllerInput, List<Guid>> valuePair in _inputMapping)
                            {
                                valuePair.Value.Remove(id);
                            }
                        }
                        finally
                        {
                            _modelsLockSlim.ExitWriteLock();
                        }
                    }
                    else
                    {
                        Logger.Error("Could not find ActionMapping for an registered Object!");
                    }
                }
                else
                {
                    Logger.Error("Can not Clear Actions for an object that is not registered");
                }
            }
            finally
            {
                _modelsLockSlim.ExitUpgradeableReadLock();
            }
        }

        public  void SetActive(object self)
        {
            try
            {
                _modelsLockSlim.EnterUpgradeableReadLock();
                if (_registeredObjects.TryGetValue(self, out var id))
                {
                    try
                    {
                        _modelsLockSlim.EnterWriteLock();
                        Logger.Debug($"Adding as Active object with Id={id}!");
                        _activeObjects.Add(id);
                    }
                    finally
                    {
                        _modelsLockSlim.ExitWriteLock();
                    }
                }
                else
                {
                    Logger.Error($"Can not set object of Type={self.GetType()} to active if its not registered");
                }
            }
            finally
            {
                _modelsLockSlim.ExitUpgradeableReadLock();
            }
        }

        public  void SetInactive(object self)
        {
            try
            {
                _modelsLockSlim.EnterUpgradeableReadLock();
                if (_registeredObjects.TryGetValue(self, out var id))
                {
                    try
                    {
                        _modelsLockSlim.EnterWriteLock();
                        Logger.Debug($"Removing from Active object with Id={id}!");
                        _activeObjects.Remove(id);
                    }
                    finally
                    {
                        _modelsLockSlim.ExitWriteLock();
                    }
                }
                else
                {
                    Logger.Error("Can not set an object to inactive if its not registered");
                }
            }
            finally
            {
                _modelsLockSlim.ExitUpgradeableReadLock();
            }
        }

        public  void Unregister(object self)
        {
            try
            {
                _modelsLockSlim.EnterUpgradeableReadLock();
                if (_registeredObjects.TryGetValue(self, out var id))
                {
                    try
                    {
                        _modelsLockSlim.EnterWriteLock();
                        Logger.Debug($"UnRegistering object with Id={id}!");
                        foreach (KeyValuePair<ControllerInput, List<Guid>> valuePair in _inputMapping)
                        {
                            valuePair.Value.Remove(id);
                        }
                        _activeObjects.Remove(id);
                        _exclusiveObjects.Remove(id);
                        _exclusiveGlobalObjects.Remove(id);
                        if(_viewTypeActionMappings.TryGetValue(id,out var handler))
                        {
                            handler.ClearActions();
                            _viewTypeActionMappings.Remove(id);
                        }
                        _registeredObjects.Remove(self);
                    }
                    finally
                    {
                        _modelsLockSlim.ExitWriteLock();
                    }
                }
                else
                {
                    Logger.Error("Can not Unregister an oject that is not registered");
                }
            }
            finally
            {
                _modelsLockSlim.ExitUpgradeableReadLock();
            }
        }

        public void DoAction(ControllerInput input)
        {
            if (input == ControllerInput.None) return;
            HashSet<Action> actionsToExecute = null;
            try
            {
                Logger.Trace($"Looking for Actions to execute on ControllerInput={input}");
                _modelsLockSlim.EnterReadLock();
                if(_inputMapping.TryGetValue(input, out var mapping))
                {
                    Guid latestExclusiveReactor = Guid.Empty;
                    DateTime latestExclusiveReactorAddedTime = DateTime.MinValue;
                    HashSet<Guid> nonExclusiveReactors = new HashSet<Guid>();
                    bool hasExclusivity = false;

                    //Global Exclusivity has Priority so we check if there is a canditate
                    Guid globalExclusiveActive = Guid.Empty;
                    if(_exclusiveGlobalObjects.Any())
                    {
                        Logger.Trace("Checking for any GlobalObjects that might be active");
                        DateTime latestGlobalExlusive = DateTime.MinValue;
                        foreach(Guid globalObject in _exclusiveGlobalObjects)
                        {
                            //Only if the global reactor is active we will take it into consideration
                            if(_activeObjects.Contains(globalObject) &&
                               latestGlobalExlusive < _viewTypeActionMappings[globalObject].SetExclusiveTime)
                            {
                                Logger.Trace(
                                    $"Found active GlobalObjects with Id={_viewTypeActionMappings[globalObject].Id}");
                                latestGlobalExlusive = _viewTypeActionMappings[globalObject].SetExclusiveTime;
                                globalExclusiveActive = _viewTypeActionMappings[globalObject].Id;
                            }
                        }
                    }
                    //If there is a Global Exclusive Reactor that blocks/handles every input
                    if(globalExclusiveActive != Guid.Empty)
                    {
                        actionsToExecute = new HashSet<Action>(
                            _viewTypeActionMappings[globalExclusiveActive].GetActions(input));
                        Logger.Trace("Adding Action to execute from Excusive GlobalObjects.");
                        return;
                    }
                    else
                    {
                        //Find out if there is a exclusive/shared reactor
                        foreach(Guid guid in mapping)
                        {
                            //Only Active objects get evaluated
                            if(!_activeObjects.Contains(guid)) continue;

                            //Exclusive objects need to be handled in order and only them can be invoked
                            if(_exclusiveObjects.Contains(guid))
                            {
                                if(latestExclusiveReactorAddedTime < _viewTypeActionMappings[guid].SetExclusiveTime)
                                {
                                    latestExclusiveReactorAddedTime = _viewTypeActionMappings[guid].SetExclusiveTime;
                                    latestExclusiveReactor = guid;
                                    hasExclusivity = true;
                                }
                                continue;
                            }
                            //If there is an exclusive object we dont need to handle any non exclusive anymore
                            if(!hasExclusivity)
                            {
                                nonExclusiveReactors.Add(guid);
                            }
                        }
                    }

                    //When there is an exclusive/active reactor for specific inputs
                    if(latestExclusiveReactor != Guid.Empty)
                    {
                        actionsToExecute = new HashSet<Action>(
                            _viewTypeActionMappings[latestExclusiveReactor].GetActions(input));
                        return;
                    }

                    actionsToExecute = new HashSet<Action>();
                    foreach(Guid nonExclusiveReactor in nonExclusiveReactors)
                    {
                        foreach(Action action in _viewTypeActionMappings[nonExclusiveReactor].GetActions(input))
                        {
                            Logger.Debug(
                                $"Adding Action to Execution List from Type={_viewTypeActionMappings[nonExclusiveReactor].ViewType}, Id={_viewTypeActionMappings[nonExclusiveReactor].Id}, Input={input} ");
                            actionsToExecute.Add(action);
                        }
                    }

                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, "Exception occured in handler");
            }
            finally
            {
                _modelsLockSlim.ExitReadLock();
                if (actionsToExecute != null) DoActionOnUiDispatcher(actionsToExecute.ToArray());
            }
        }

        private void DoActionOnUiDispatcher(Action[] actions)
        {
            foreach (Action action in actions)
            {
                try
                {
                    Execute.BeginOnUIThread(action);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "An unhandled Exception occured during invokation of an registered Action");
                }
            }
        }

        private class ViewGamepadHandler :IDisposable
        {
            public ViewGamepadHandler(object obj, InputExclusivity exclusivity)
            {
                ViewType = obj.GetType();
                Instance = new WeakReference(obj);
                Id = Guid.NewGuid();
                SetExclusivity(exclusivity);
            }
            private readonly Dictionary<ControllerInput, HashSet<Action>> _actionMapping = new Dictionary<ControllerInput, HashSet<Action>>();
            public Guid Id { get; }
            public Type ViewType { get; }
            public InputExclusivity Exclusivity { get; private set; }
            public DateTime SetExclusiveTime { get; private set; }
            public WeakReference Instance { get; }
            public Action[] GetActions(ControllerInput input)
            {
                if (!_actionMapping.TryGetValue(input, out var actions)) return new Action[]{};
                return actions.ToArray();
            }
            public void AddAction(ControllerInput input, Action action)
            {
                if (_actionMapping.TryGetValue(input, out var actions))
                {
                    actions.Add(action);
                }
                else
                {
                    _actionMapping.Add(input, new HashSet<Action> { action });
                }
            }

            public void SetExclusivity(InputExclusivity exclusivity)
            {
                switch (exclusivity)
                {
                    case InputExclusivity.Shared:
                        SetExclusiveTime = DateTime.MinValue;
                        break;
                    case InputExclusivity.RegisterdControllerInputs:
                    case InputExclusivity.AllControllerInputs:
                        SetExclusiveTime = DateTime.Now;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(exclusivity), exclusivity, null);
                }
                Exclusivity = exclusivity;
            }
            public void ClearActions()
            {
                _actionMapping.Clear();
            }
            public void Dispose()
            {
                _actionMapping.Clear();
            }
        }

        public void Dispose()
        {
            _modelsLockSlim?.Dispose();
        }
    }
}