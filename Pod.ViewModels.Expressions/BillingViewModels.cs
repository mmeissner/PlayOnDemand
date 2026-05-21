#region Licence
/****************************************************************
 *  Filename: BillingViewModels.cs
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
using System.Linq.Expressions;
using Pod.Data.Models.Billing;
using Pod.Data.Models.Shell;
using Pod.ViewModels.Customer;
using SubscriptionState = Pod.ViewModels.Customer.SubscriptionState;

namespace Pod.ViewModels.Expressions
{
    public static class ToSubscriptionPaymentVm
    {
        public static Expression<Func<SubscriptionOrder, SubscriptionPaymentViewModel>> FromSubscriptionOrder()
        {
            return x => new SubscriptionPaymentViewModel
                        {
                                StationName =
                                        x.SubscriptionState.Station.StationSettings.
                                          DisplayName,
                                OrderedOnUtc = x.CreatedOnUtc,
                                Currency = x.Currency,
                                PaymentAmount = x.PaymentAmount,
                                CustomerReference = x.CustomerOrderReference,
                                OrderedDuration = x.TimeOrdered,
                                PaymentReference = x.CreatePaymentReference(),
                                PayedOnUtc = x.SubscriptionPayment.PaymentReceivedDate
                        };
        }
    }

    public static class ToSubscriptionOrderVm
    {
        public static Expression<Func<SubscriptionOrder, SubscriptionOrderViewModel>> FromSubscriptionOrder()
        {
            return x => new SubscriptionOrderViewModel
                        {
                                StationName =
                                        x.SubscriptionState.Station.
                                          StationSettings.DisplayName,
                                CreatedOnUtc = x.CreatedOnUtc,
                                ExpiresOnUtc = x.ExpiresOnUtc,
                                PaymentAmount = x.PaymentAmount,
                                Currency = x.Currency,
                                OrderedDuration = x.TimeOrdered,
                                CustomerReference = x.CustomerOrderReference,
                                PaymentReference = x.CreatePaymentReference()
                        };
        }
    }

    public static class ToSubscriptionOrderBasicVm
    {
        public static Expression<Func<SubscriptionOrder, SubscriptionOrderBasicViewModel>> FromSubscriptionOrder()
        {
            return x => new SubscriptionOrderBasicViewModel
                        {
                                UserId = x.ApplicationUserId,
                                StationId = x.SubscriptionState.StationId,
                                OrderId = x.Id,
                                ExpiresOn = x.ExpiresOnUtc
                        };
        }
    }

    public static class ToStationSubscriptionVm
    {
        public static readonly Func<Station, StationSubscriptionViewModel> FuncFromStation = FromStation().Compile();

        public static Expression<Func<Station, StationSubscriptionViewModel>> FromStation()
        {
            return x => new StationSubscriptionViewModel
                        {
                                StationId = x.SubscriptionState.StationId,
                                DisplayName = x.StationSettings.DisplayName,
                                State = x.SubscriptionState.ExpiresOnUtc.GetSubscriptionState(),
                                ExpiresOnUtc = x.SubscriptionState.ExpiresOnUtc
                        };
        }
    }

    public static class BillingModelExtensions
    {
        public static string CreatePaymentReference(this SubscriptionOrder subscriptionOrder)
        {
            return $"{subscriptionOrder.SubscriptionState.Station.ApplicationUser.CustomerNumber.ToString()}-{subscriptionOrder.OrderNumber.ToString()}";
        }
        public static SubscriptionState GetSubscriptionState(this DateTime? expiresOnUtc)
        {
            var subscriptionState = SubscriptionState.Inactive;
            if (expiresOnUtc != null)
            {
                subscriptionState = expiresOnUtc > DateTime.UtcNow ? SubscriptionState.Active :
                        SubscriptionState.Expired;
            }

            return subscriptionState;
        }
    }
}
