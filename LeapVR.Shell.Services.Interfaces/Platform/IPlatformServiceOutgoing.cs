#region Licence
/****************************************************************
 *  Filename: IPlatformServiceOutgoing.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-7-14
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
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Services.Interfaces.Platform
{
    /// <summary>
    /// Outgoing service for Station-Server communication releated to applications and platforms.
    /// </summary>
    public interface IPlatformServiceOutgoing
    {
        bool CanExecute(Guid applicationGuid, IProcessExecutionLogic logic);
    }
}
