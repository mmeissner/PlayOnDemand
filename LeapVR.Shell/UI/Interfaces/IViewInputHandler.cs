#region Licence
/****************************************************************
 *  Filename: IViewInputHandler.cs
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
using LeapVR.Shell.UI.Core;

namespace LeapVR.Shell.UI.Interfaces
{
    public interface IViewInputHandler: IDisposable
    {
        bool IsControllerInputEnabled { get; }
        void SetObjectExclusivity(object exclusiveObject,InputExclusivity exclusivity);
        void Register(object self, InputExclusivity exclusivity);
        void Unregister(object self);
        void AddAction(object self, ControllerInput input, Action action);
        void ClearActions(object self);
        void SetActive(object self);
        void SetInactive(object self);
        void DoAction(ControllerInput input);
    }
}