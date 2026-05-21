#region Licence
/****************************************************************
 *  Filename: IAppPlatformData.cs
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Domain.Models.App
{
    /// <summary>
    /// Stores data releated to platform (and execution) for single application.
    /// </summary>
    
    public interface IAppPlatformData
    {
        /// <summary>
        /// Guid of application.
        /// </summary>
        Guid ApplicationGuid { get; }

        /// <summary>
        /// Guid of platform that application belongs to. This platform is responsible for execution, installation and other related topics for this application.
        /// </summary>
        Guid PlatformPluginId { get; }

        /// <summary>
        /// Gets the name of the App on the Platform.
        /// </summary>
        /// <value>
        /// The name of the App on the Platform.
        /// </value>
        string ApplicationName { get; }

        /// <summary>
        /// Contains collection of <see cref="IProcessExecutionLogicDto"/> objects containing data on how to execute this application, in platform context.
        /// </summary>
        IEnumerable<IProcessExecutionLogic> ExecutionLogicInstructions { get; }
        /// <summary>
        /// Identifies if application is set to be available to play, or is meant to be unavailable.
        /// </summary>
        bool IsEnabled { get; }
    }
}