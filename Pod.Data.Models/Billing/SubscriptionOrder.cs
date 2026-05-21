#region Licence
/****************************************************************
 *  Filename: SubscriptionOrder.cs
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
using Pod.Data.Infrastructure;
using Pod.Data.Models.Shell;
using Pod.Enums;

namespace Pod.Data.Models.Billing
{
    /// <summary>
    /// Order for an payed subscription for an station
    /// </summary>
    public class SubscriptionOrder
    {
        private SubscriptionOrder() { }
        internal SubscriptionOrder(Guid applicationUserId,
                SubscriptionState subscriptionState, DateTime expiresUtc, TimeSpan timeOrdered, decimal paymentAmount,
                CurrencyIsoCode currencyCode, string customerOrderReference, string sourceIpAddress)
        {
            ApplicationUserId = applicationUserId;
            SubscriptionStateId = subscriptionState.Id;
            SubscriptionState = subscriptionState;
            TimeOrdered = timeOrdered;
            PaymentAmount = paymentAmount;
            CreatedFromIpAddress = sourceIpAddress;
            Currency = currencyCode;
            CustomerOrderReference = customerOrderReference;
            CreatedOnUtc = DateTime.UtcNow;
            ExpiresOnUtc = expiresUtc;
        }

        /// <summary>
        /// The Id of this Order
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The UserId this order belongs to
        /// </summary>
        public Guid ApplicationUserId { get; private set; }

        /// <summary>
        /// Automatically set by Database triggers
        /// </summary>
        public long OrderNumber { get; private set; }
        /// <summary>
        /// DateTime when this instance was created
        /// </summary>
        public DateTime CreatedOnUtc { get; private set; }

        /// <summary>
        /// Order will expire on and will not be allowed to be fulfilled afterwards
        /// </summary>
        public DateTime ExpiresOnUtc { get; private set; }

        /// <summary>
        /// Order received from IpAddress
        /// </summary>
        public string CreatedFromIpAddress { get; private set; }

        /// <summary>
        /// Time of order received
        /// </summary>
        public TimeSpan TimeOrdered { get; private set; }

        /// <summary>
        /// Amount to pay for this order
        /// </summary>
        public decimal PaymentAmount { get; private set; }

        /// <summary>
        /// The currency related to the Payment Amount
        /// </summary>
        public CurrencyIsoCode Currency { get; private set; }

        /// <summary>
        /// Custom reference that was provided when the order was issued
        /// </summary>
        public string CustomerOrderReference { get; private set; }

        /// <summary>
        /// The Id of the Subscription State of an Station
        /// </summary>
        public Guid SubscriptionStateId { get; private set; }

        /// <summary>
        /// The Navigation property to the Subscription State
        /// </summary>
        public SubscriptionState SubscriptionState { get; private set; }

        /// <summary>
        /// The Id to the Payment data for this order
        /// </summary>
        public Guid? SubscriptionPaymentId { get; private set; }

        /// <summary>
        /// The Navigational Property for the Payment 
        /// </summary>
        public SubscriptionPayment SubscriptionPayment { get; private set; }

        /// <summary>
        /// Sets the Order to payed
        /// </summary>
        /// <param name="paymentReceivedUtc">The Time the payment was received</param>
        /// <param name="paymentGatewayReference">The payment reference provided by the payment gateway</param>
        /// <returns></returns>
        public IResult PayOrder(DateTime paymentReceivedUtc, string paymentGatewayReference = null)
        {
            //Has Station SubscriptionState
            var retval = new Result();
            retval.RefNotNull(SubscriptionState,nameof(SubscriptionState));

            //Validate if it was already Payed
            retval.RefNotNull(SubscriptionPayment,SubscriptionPaymentId,nameof(SubscriptionPayment),UserError.OrderAlreadyPayed);

            //Not already Expired
            retval.ArgNotAfterOrEqualThen(paymentReceivedUtc,nameof(paymentReceivedUtc),ExpiresOnUtc,nameof(ExpiresOnUtc),UserError.OrderAlreadyExpired);
            if(retval.HasError()) return retval;

            var paymentResult = SubscriptionPayment.Create(
                    SubscriptionState,
                    this,
                    paymentReceivedUtc,
                    paymentGatewayReference);
            if(paymentResult.HasError()) return retval.Add(paymentResult);
            SubscriptionPayment = paymentResult.ReturnValue;
            return retval;
        }
    }
}