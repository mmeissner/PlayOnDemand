#region Licence
/****************************************************************
 *  Filename: EventMessageEnums.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    /// <summary>
    /// Reasons for views to be dismissed.
    /// </summary>
    public enum ViewDismissReason
    {
        ActivelyClose,
        Timeout,
        Exception,
    }

    public enum ApplicationMessageType
    {
        NewStart,
        Quit
    }
    /// <summary>
    /// Critical phases that ui is responsible to react during the lifetime of an application execution.
    /// </summary>
    public enum UIApplicationExecutionPhase
    {
        BeginnExecution,
        ComponentTerminationRequested,
        EndedSuccefully,
    }

    public enum UIShellClientUpdateStates
    {
        Unknown = 0,
        NotStarted = 100,
        CheckingNewestVersion = 200,
        UpdateAvailable = 300,
        Downloading = 400,
        ReadyToUpdate = 500,
        Upgrading = 600,
        AwaitToRestart = 700,
        Cancelling = 800,
        UpToDate = 900,
        Cancelled = 901,
        Errored = 902,
    }

    public enum ResponseErrorType
    {
        Undefined
    }

}
