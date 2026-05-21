#region Licence
/****************************************************************
 *  Filename: InputControllerScreen.cs
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
using Caliburn.Micro;
using LeapVR.Shell.UI.Interfaces;
using NLog;
using Action = System.Action;

namespace LeapVR.Shell.UI.Base
{
    public abstract class InputControllerScreen : Screen, IDisposable
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IViewInputHandler _inputHandler;
        protected InputControllerScreen(IViewInputHandler inputHandler, InputExclusivity exclusivity = InputExclusivity.Shared)
        {

            _inputHandler = inputHandler;
            if(_inputHandler != null)inputHandler.Register(this, exclusivity);
        }

        /// <summary>
        /// Sets the object as exclusive input receiver. It's not guarantied that the object is the only exclusive at a time
        /// In this case only the first found will be notified!
        /// </summary>
        /// <param name="exclusivity"></param>
        protected void SetControllerInputExclusivity(InputExclusivity exclusivity)
        {
            _inputHandler?.SetObjectExclusivity(this,exclusivity);
        }
        protected void AddControllerInputAction(ControllerInput input, Action action)
        {
            _inputHandler?.AddAction(this, input, action);
        }

        protected void ClearAllControllerInputActions()
        {
            _inputHandler?.ClearActions(this);
        }

        protected override void OnActivate()
        {
            Logger.Debug($"OnActivate called for Type {GetType()}");
            base.OnActivate();
            _inputHandler?.SetActive(this);
        }

        protected override void OnDeactivate(bool close)
        {
            Logger.Debug($"OnActivate called for Type {GetType()} with close={close}");
            if (close) _inputHandler?.Unregister(this);
            else _inputHandler?.SetInactive(this);
            base.OnDeactivate(close);
        }
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_inputHandler!=null)ClearAllControllerInputActions();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}