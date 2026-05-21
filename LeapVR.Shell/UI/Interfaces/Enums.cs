#region Licence
/****************************************************************
 *  Filename: Enums.cs
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
using LeapVR.Shell.Domain.Models.Station;

namespace LeapVR.Shell.UI.Interfaces
{
    /// <summary>
    /// Generic loading states for all could-be busy views.
    /// </summary>
    public enum LoadingStates
    {
        Nothing,
        Loading,
        Loaded
    }

    /// <summary>
    /// Specify different states for auto-update process.
    /// </summary>
    public enum UpdatesStates
    {
        Unchecked,
        UpdatesAvailable,
        Downloading,
        ReadyToUpdate,
        UpToDate,
        Loading,
        UnexpectedSituation
    }

    public enum MessageType
    {
        Unknown,
        Connecting,
        LocalConnectionProblem,
        CloudNotAccessibleProblem,
        ShellVersionOutOfDate,
        InvalidUsernameOrPassword,
        LicenseNotDeployed,
        LicenseNotFound,
        LicenseNotLinked,
        LicenseSuspended,
        LicenseRevoked,
        StationRevoked,
        StationSuspended,
    }
    public enum NavigationDirection
    {
        Up,
        Down,
        Left,
        Right
    }
    public enum ControllerInput
    {
        None,
        Up,
        Down,
        Left,
        Right,
        DUp,
        DDown,
        DLeft,
        DRight,
        Accept,
        Cancel,
        XAction,
        YAction,
        PushOne,
        PushTwo,
        Start,
        Back,
        Guide,
        NextOne,
        PreviousOne,
        NextTwo,
        PreviousTwo,
    }

    public enum InputExclusivity
    {
        Shared,
        RegisterdControllerInputs,
        AllControllerInputs
    }
}
