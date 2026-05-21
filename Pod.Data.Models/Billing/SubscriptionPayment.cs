#region Licence
/****************************************************************
 *  Filename: SubscriptionPayment.cs
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
    /// Payment data for an Order
    /// </summary>
    public class SubscriptionPayment
    {
        private SubscriptionPayment() { }
        private SubscriptionPayment(SubscriptionOrder subscriptionOrder, DateTime paymentReceivedUtc, string paymentGatewayRef)
        {
            CreatedOnUtc = DateTime.UtcNow;
            PaymentReference = subscriptionOrder.CustomerOrderReference;
            PaymentReceivedDate = paymentReceivedUtc;
            PaymentGatewayReference = paymentGatewayRef;
            PaymentAmount = subscriptionOrder.PaymentAmount;
            Currency = subscriptionOrder.Currency;
            SubscriptionOrder = subscriptionOrder;
        }

        /// <summary>
        /// The Id of this instance
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The DateTime this instance was created
        /// </summary>
        public DateTime CreatedOnUtc { get; private set; }

        /// <summary>
        /// CustomerOrderReference here redundant for the Payment 
        /// </summary>
        public string PaymentReference { get; private set; }

        /// <summary>
        /// Payment reference provided from the payment gateway
        /// </summary>
        public string PaymentGatewayReference { get; private set; }

        /// <summary>
        /// The Amount Payed
        /// </summary>
        public decimal PaymentAmount { get; private set; }

        /// <summary>
        /// The Currency paid in
        /// </summary>
        public CurrencyIsoCode Currency { get; private set; }

        /// <summary>
        /// The DateTime the Payment was received, e.g by payment processor/gateway
        /// </summary>
        public DateTime PaymentReceivedDate { get; private set; }

        /// <summary>
        /// The Order related to this Payment
        /// </summary>
        public SubscriptionOrder SubscriptionOrder { get; private set; }

        /// <summary>
        /// The change operation to the stations subscription related to the payment
        /// </summary>
        public SubscriptionChange SubscriptionChange { get; private set; }

        /// <summary>
        /// Creates a new Payment statement for an Order
        /// </summary>
        /// <param name="subscriptionState">The current Subscription state of a station</param>
        /// <param name="subscriptionOrder">The order related to this payment</param>
        /// <param name="paymentReceivedUtc">The dateTime the payment was received</param>
        /// <param name="paymentGatewayRef">The reference provided by the payment gateway/processor</param>
        /// <returns>result</returns>
        internal static Result<SubscriptionPayment> Create(SubscriptionState subscriptionState, SubscriptionOrder subscriptionOrder, DateTime paymentReceivedUtc, string paymentGatewayRef)
        {
            var result = new Result<SubscriptionPayment>();
            var payment = new SubscriptionPayment(subscriptionOrder, paymentReceivedUtc, paymentGatewayRef);
            var resultChangeOperation = subscriptionState.CreateOrExtend(payment);
            if(resultChangeOperation.HasError())return result.Add(resultChangeOperation);
            payment.SubscriptionChange = resultChangeOperation.ReturnValue;
            return result.Add(payment);
        }
    }
}