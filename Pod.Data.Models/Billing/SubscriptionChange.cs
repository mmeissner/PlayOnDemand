#region Licence
/****************************************************************
 *  Filename: SubscriptionChange.cs
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
    /// Tracks changes to an Station Subscription
    /// </summary>
    public class SubscriptionChange
    {
        private SubscriptionChange() { }
        private SubscriptionChange(
                SubscriptionState subscriptionState,
                SubscriptionPayment subscriptionPayment,
                DateTime startTime,
                SubscriptionChangeOperation operationType)
        {
            SubscriptionPayment = subscriptionPayment;
            SubscriptionState = subscriptionState;
            ExtendsFromUtc = startTime;
            ExtendsToUtc = startTime.Add(subscriptionPayment.SubscriptionOrder.TimeOrdered);
            Type = operationType;
            CreatedOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// The Id of this instance
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// DateTime when this instance was created
        /// </summary>
        public DateTime CreatedOnUtc { get; private set; }

        /// <summary>
        /// The DateTime for the start of the effect of this operation
        /// </summary>
        public DateTime ExtendsFromUtc { get; private set; }

        /// <summary>
        /// The DateTime to that the subscription was extended
        /// </summary>
        public DateTime ExtendsToUtc { get; private set; }

        /// <summary>
        /// The Type of effect this operation created
        /// </summary>
        public SubscriptionChangeOperation Type { get; private set; }

        /// <summary>
        /// The SubscriptionState Id this instance belongs to
        /// </summary>
        public Guid SubscriptionStateId { get; private set; }

        /// <summary>
        /// The navigational property for the SubscriptionState
        /// </summary>
        public SubscriptionState SubscriptionState { get; private set; }

        /// <summary>
        /// The SubscriptionPaymentId this instance belongs to
        /// </summary>
        public Guid SubscriptionPaymentId { get; private set; }

        /// <summary>
        /// The navigational property for the SubscriptionPaymentId
        /// </summary>
        public SubscriptionPayment SubscriptionPayment { get; private set; }

        /// <summary>
        /// Creates a new SubscriptionChange from an successful Payment
        /// </summary>
        /// <param name="subscriptionState">The affected SubscriptionState</param>
        /// <param name="subscriptionPayment">The Payment done</param>
        /// <param name="startTime">The DateTime from that this Operation affects the <see cref="SubscriptionState"/></param>
        /// <param name="operationType"></param>
        /// <returns>result</returns>
        internal static Result<SubscriptionChange> Create(
                SubscriptionState subscriptionState,
                SubscriptionPayment subscriptionPayment,
                DateTime startTime,
                SubscriptionChangeOperation operationType)
        {
            var result = new Result<SubscriptionChange>();
            result.ArgNotEnum(
                    typeof(SubscriptionChangeOperation),
                    operationType,
                    SubscriptionChangeOperation.Undefined,
                    nameof(operationType));
            result.ArgNotNull(subscriptionState, nameof(subscriptionState));
            result.ArgNotNull(subscriptionPayment,nameof(subscriptionPayment));

            if(result.IsSuccess())
                result.Add(new SubscriptionChange(subscriptionState, subscriptionPayment, startTime, operationType));
            return result;
        }
    }
}