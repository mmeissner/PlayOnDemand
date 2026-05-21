#region Licence
/****************************************************************
 *  Filename: IUniqueAppFactory.cs
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
using System.Threading.Tasks;
using Pod.Data.Infrastructure;
using Pod.Data.Models.Shell;
using Pod.Enums;

namespace Pod.Data.Models.Interfaces
{
    /// <summary>
    /// Provides Access to Unique Applications
    /// </summary>
    public interface IUniqueAppFactory
    {
        /// <summary>
        /// Gets or Creates an Unique application
        /// </summary>
        /// <param name="creatorType">The origin this request is triggered by</param>
        /// <param name="creatorId">The Id of the creator that triggered the request</param>
        /// <param name="appUpdate">The information about the app</param>
        /// <returns>The Result</returns>
        IResult<UniqueApp> GetOrCreate(CreatorType creatorType,string creatorId,IAppUpdate appUpdate);

        /// <summary>
        /// Gets or Creates an Unique application async
        /// </summary>
        /// <param name="creatorType">The origin this request is triggered by</param>
        /// <param name="creatorId">The Id of the creator that triggered the request</param>
        /// <param name="appUpdate">The information about the app</param>
        /// <returns>The Result</returns>
        Task<IResult<UniqueApp>> GetOrCreateAsync(CreatorType creatorType, string creatorId, IAppUpdate appUpdate);
    }

    /// <summary>
    /// Update information for an remotely installed app
    /// </summary>
    public interface IAppUpdate
    {
        /// <summary>
        /// The AppId
        /// </summary>
        Guid ApplicationId { get; }

        /// <summary>
        /// The current instance version the update is for
        /// This version has to increase remotely for each applied update
        /// </summary>
        UInt32 InstanceVersion { get; }
        /// <summary>
        /// The Name of the Application
        /// </summary>
        string DisplayName { get; }
        /// <summary>
        /// Enabled state of the Application
        /// </summary>
        bool IsEnabled { get; }
    }
}
