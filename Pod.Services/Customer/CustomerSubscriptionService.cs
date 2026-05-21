#region Licence
/****************************************************************
 *  Filename: CustomerSubscriptionService.cs
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pod.Data;
using Pod.Data.Exceptions;
using Pod.Data.Infrastructure;
using Pod.Enums;
using Pod.ViewModels.Customer;
using Pod.ViewModels.Expressions;

namespace Pod.Services.Customer
{
    /// <summary>
    /// Service for Users for Subscriptions
    /// </summary>
    public class CustomerSubscriptionService
    {
        private readonly ILogger<CustomerSubscriptionService> _logger;
        private readonly PodDbContext _podContext;
        public CustomerSubscriptionService(ILogger<CustomerSubscriptionService> logger, PodDbContext podContext)
        {
            _logger = logger;
            _podContext = podContext;
        }

        /// <summary>
        /// Get all subscriptions for all the Stations of an User
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <returns>Collection with all Station Subscriptions</returns>
        public async Task<IResult<ICollection<StationSubscriptionViewModel>>> GetStationSubscriptions(Guid userId)
        {
            var result = new Result<ICollection<StationSubscriptionViewModel>>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            if(result.HasError()) return result;
            return result.Add(
                    await _podContext.Stations.Where(x => x.ApplicationUserId == userId).
                                      Select(ToStationSubscriptionVm.FromStation()).
                                      AsNoTracking().
                                      ToArrayAsync());
        }

        /// <summary>
        /// Get unpaid orders
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="includeExpired">True to include also expired orders</param>
        /// <returns>Collection of unpaid orders</returns>
        public async Task<IResult<ICollection<SubscriptionOrderViewModel>>> GetUnpaidOrders(
                Guid userId, bool includeExpired = false)
        {
            var result = new Result<ICollection<SubscriptionOrderViewModel>>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            if (result.HasError()) return result;
            var now = DateTime.UtcNow;
            var dbUnpaidOrders = await _podContext.SubscriptionOrders.Where(
                                                           x => x.ApplicationUserId == userId &&
                                                                x.SubscriptionPaymentId == null &&
                                                                (includeExpired || x.ExpiresOnUtc > now)).
                                                   Select(ToSubscriptionOrderVm.FromSubscriptionOrder()).
                                                   AsNoTracking().
                                                   ToArrayAsync();
            return result.Add(dbUnpaidOrders);
        }

        /// <summary>
        /// Creates an new order for an subscription
        /// </summary>
        /// <param name="userId">The Users Id</param>
        /// <param name="stationId">The Stations Id</param>
        /// <param name="orderDuration">The duration of the subscription</param>
        /// <param name="sourceIpAddress">The peer the order is send from</param>
        /// <param name="customerOrderRef">A reference to the order</param>
        /// <returns>The Order Result</returns>
        public async Task<IResult<SubscriptionOrderViewModel>> RequestNewOrder(Guid userId, Guid stationId,TimeSpan orderDuration,string sourceIpAddress,string customerOrderRef = null)
        {
            var result = new Result<SubscriptionOrderViewModel>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            result.ArgNotEmpty(stationId, nameof(stationId), UserError.StationInvalidStationId);
            if (result.HasError()) return result;

            var dbUnpaidOrders = await _podContext.SubscriptionOrders.
                                                   Where(
                                                           x => x.ApplicationUserId == userId &&
                                                                x.ExpiresOnUtc >= DateTime.UtcNow &&
                                                                x.SubscriptionState.StationId == stationId).
                                                   Select(ToSubscriptionOrderBasicVm.FromSubscriptionOrder()).
                                                   ToArrayAsync();
            result.ValueNotEqual(dbUnpaidOrders.Any(),nameof(dbUnpaidOrders),true,UserError.OrderMaximumActiveAmountReached);
            if(result.HasError()) return result;
            var subscriptionState = await _podContext.Stations.
                                                Where(x => x.Id == stationId && x.ApplicationUserId == userId).
                                                Include(x => x.SubscriptionState).
                                                Include(x=> x.StationSettings).
                                                FirstAsync();
            var orderResult  = subscriptionState.CreateOrder(
                    DateTime.UtcNow.AddDays(7),
                    orderDuration,
                    Convert.ToDecimal(orderDuration.TotalDays) * 7m,
                    CurrencyIsoCode.CNY,
                    sourceIpAddress,
                    customerOrderRef);
            if(orderResult.HasError()) return result.Add(orderResult);
            _podContext.Add(orderResult.ReturnValue);
            await _podContext.SaveChangesAsync();
            var retval = await _podContext.SubscriptionOrders.
                                           Where(x => x.Id == orderResult.ReturnValue.Id).
                                           Select(ToSubscriptionOrderVm.FromSubscriptionOrder()).
                                           FirstAsync();
            return result.Add(retval);
        }

        /// <summary>
        /// Get all paid orders
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <returns>All paid orders</returns>
        public async Task<IResult<ICollection<SubscriptionPaymentViewModel>>> GetAllPayedOrders(Guid userId)
        {
            var result = new Result<ICollection<SubscriptionPaymentViewModel>>();
            result.ArgNotEmpty(userId, nameof(userId), UserError.UserIdentityInvalidId);
            if (result.HasError()) return result;
            var paidOrders = await _podContext.SubscriptionOrders.
                                               Where(
                                                       x => x.ApplicationUserId == userId &&
                                                            x.SubscriptionPaymentId != null).
                                               Select(ToSubscriptionPaymentVm.FromSubscriptionOrder()).
                                               AsNoTracking().
                                               ToArrayAsync();
            return result.Add(paidOrders);
        }
    }
}