#region Licence
/****************************************************************
 *  Filename: AccountantService.cs
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
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Exceptions;
using Pod.Data.Infrastructure;

namespace Pod.Services.Accountant
{
    /// <summary>
    /// Provides Accountant related functionality to process Orders
    /// </summary>
    public class AccountantService
    {
        private readonly ILogger<AccountantService> _logger;

        private readonly PodDbContext _podContext;
        public AccountantService(ILogger<AccountantService> logger,PodDbContext podContext)
        {
            _logger = logger;
            _podContext = podContext;
        }

        public async Task<IResult> SetOrderPayed(long orderId, DateTime localDateTime, TimeZoneInfo timeZoneInfo)
        {
            //https://stackoverflow.com/questions/5958662/mvc-utc-date-to-localtime
            throw new NotImplementedException();
        } 

        public async Task<IResult> SetOrderPayed(long orderId, DateTime paymentTimeUtc)
        {
            throw new NotImplementedException();
        }
    }

    public class DummyViewModel{}
}
