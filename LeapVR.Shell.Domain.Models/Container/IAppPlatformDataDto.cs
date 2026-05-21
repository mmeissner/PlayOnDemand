#region Licence
/****************************************************************
 *  Filename: IAppPlatformDataDto.cs
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

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Interface used for PlatformData transfered through Container
    /// </summary>
    public interface IAppPlatformDataDto
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
        /// Contains collection of <see cref="IProcessExecutionLogicDto"/> objects containing data on how to execute this application, in platform context.
        /// </summary>
        IEnumerable<IProcessExecutionLogicDto> ExecutionLogicInstructions { get; }
    }
}