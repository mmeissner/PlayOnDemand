#region Licence
/****************************************************************
 *  Filename: DialogType.cs
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
namespace LeapVR.Shell.UI.Universal.Dialog
{
    public enum DialogType
    {
        StartScreenGameInVrMode,
        StartVrGameInScreenMode,
        ResetVrBoxStatistics,
        AttemptToLogout,
        AttemptToShutdown,
        AttemptToPowerOff,
        AttemptToDeletePlatformAccount,
        NoSuitableExecution
    }
}
