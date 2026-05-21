#region Licence
/****************************************************************
 *  Filename: UniqueApp.cs
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
using Pod.Data.Infrastructure;
using Pod.Enums;

namespace Pod.Data.Models.Shell
{
    /// <summary>
    /// Representation for each unique Application.
    /// This is a global instance of an Application with a unique Id
    /// similar to a SteamId of an Application. Each <see cref="LocalApp"/>
    /// must have a UniqueApp as parent, were the Local App is an Local Instance
    /// of an Unique App
    /// </summary>
    public class UniqueApp
    {
        private HashSet<LocalApp> _localApps;
        private UniqueApp() { }
        private UniqueApp(
                Guid applicationId,
                PlatformType platformType,
                string displayName,
                CreatorType origin,
                string creatorId)
        {
            CreatedOnUtc = DateTime.UtcNow;
            Id = applicationId;
            Platform = platformType;
            DisplayName = displayName;
            Origin = origin;
            CreatorId = creatorId;
        }
        /// <summary>
        /// This is the global unique identifier for each Unique App
        /// It follows special encoding to ensure uniqueness for applications
        /// from different platforms
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The Display Name or international Name of a Unique App
        /// This is set initially by the first Local App occurence and its
        /// Name and should be curated 
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// The DateTime this app was created
        /// </summary>
        public DateTime CreatedOnUtc { get; private set; }

        /// <summary>
        /// The Creator or Origin that triggered the creation of this app
        /// </summary>
        public CreatorType Origin { get; private set; }

        /// <summary>
        /// The Id of the Creator as String
        /// </summary>
        public string CreatorId { get; private set; }

        /// <summary>
        /// The Platform this App is related to
        /// </summary>
        public PlatformType Platform { get; private set; }

        /// <summary>
        /// Creates a new UniqueApp
        /// </summary>
        /// <param name="origin">The source of this request</param>
        /// <param name="creatorId">The Id of the requesting entity</param>
        /// <param name="platformType">The Platform the unique app belongs to</param>
        /// <param name="applicationId">The global unique application Id </param>
        /// <param name="displayName">The international display name of the application</param>
        /// <returns></returns>
        public static IResult<UniqueApp> Create(
                CreatorType origin,
                string creatorId,
                PlatformType platformType,
                Guid applicationId,
                string displayName)
        {
            var result = new Result<UniqueApp>();
            result.ArgNotEnum(typeof(CreatorType), origin, CreatorType.Undefined, nameof(origin));
            result.ArgNotNullOrWhitespace(creatorId, nameof(creatorId));
            result.ArgNotEnum(typeof(PlatformType), platformType, PlatformType.Undefined, nameof(platformType));
            result.ArgNotEqual(applicationId,nameof(applicationId),Guid.Empty);
            result.ArgNotNullOrWhitespace(displayName, nameof(displayName));
            if(result.HasError()) return result;
            return result.Add(new UniqueApp(applicationId, platformType, displayName, origin, creatorId));
        }

        /// <summary>
        /// Collection of all Local Apps related to this Unique App
        /// </summary>
        public IReadOnlyCollection<LocalApp> LocalApps => _localApps;
    }
}