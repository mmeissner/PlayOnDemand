#region Licence
/****************************************************************
 *  Filename: IAccountAccess.cs
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
using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Platform.Account
{
    public interface IAccountAccess
    {
        string Password { get; }
        string Username { get; }
        bool IsReleased { get; }
        void Release();
    }
}