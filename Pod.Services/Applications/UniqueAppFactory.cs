#region Licence
/****************************************************************
 *  Filename: UniqueAppFactory.cs
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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Infrastructure;
using Pod.Data.Models;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Shell;
using Pod.Enums;

namespace Pod.Services.Applications
{
    public class UniqueAppFactory : IUniqueAppFactory
    {
        private readonly ILogger<UniqueAppFactory> _logger;
        private readonly PodDbContext _dbContext;
        public UniqueAppFactory(ILogger<UniqueAppFactory> logger,PodDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IResult<UniqueApp> GetOrCreate(CreatorType creatorType, string creatorId, IAppUpdate appUpdate)
        {
            var result = new Result<UniqueApp>();
            var uniqueApp = _dbContext.UniqueApps.Find(appUpdate.ApplicationId);
            if(uniqueApp != null)return result.Add(uniqueApp);
            return CreateUniqueApp(creatorType, creatorId, appUpdate);
        }

        public async Task<IResult<UniqueApp>> GetOrCreateAsync(CreatorType creatorType, string creatorId, IAppUpdate appUpdate)
        {
            var result = new Result<UniqueApp>();
            var uniqueApp = await _dbContext.UniqueApps.FindAsync(appUpdate.ApplicationId);
            if(uniqueApp != null)return result.Add(uniqueApp);
            return CreateUniqueApp(creatorType, creatorId, appUpdate);
        }

        private IResult<UniqueApp> CreateUniqueApp(CreatorType creatorType, string creatorId, IAppUpdate appUpdate)
        {
            var result = new Result<UniqueApp>();
            var platform = appUpdate.ApplicationId.DecodeGuid(out _);
            var createUniqueAppResult = UniqueApp.Create(creatorType, creatorId, platform, appUpdate.ApplicationId, appUpdate.DisplayName);
            if(createUniqueAppResult.HasError()) return result.Add(createUniqueAppResult);
            var uniqueApp = createUniqueAppResult.ReturnValue;
            _dbContext.Add(uniqueApp);
            return result;
        }
    }
}
