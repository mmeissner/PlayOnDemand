#region Licence
/****************************************************************
 *  Filename: SubscriptionState.cs
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
using Pod.Data.Models.Billing;
using Pod.Enums;

namespace Pod.Data.Models.Shell
{
    /// <summary>
    /// Subscription State for an Station
    /// </summary>
    public class SubscriptionState
    {
        private HashSet<SubscriptionChange> _subscriptionChanges;
        private HashSet<SubscriptionOrder> _orders;

        private SubscriptionState(){}
        internal SubscriptionState(Station station)
        {
            Station = station;
            StartOnUtc = null;
            ExpiresOnUtc = null;
        }
        /// <summary>
        /// Id of this instance
        /// </summary>
        public Guid Id { get;private set; }

        /// <summary>
        /// The StationId this instance belongs to
        /// </summary>
        public Guid StationId { get; private set; }
        /// <summary>
        /// Navigation Property for the Station
        /// </summary>
        public Station Station { get; private set; }

        /// <summary>
        /// DateTime of the Subscription
        /// </summary>
        public DateTime? StartOnUtc { get; private set; }

        /// <summary>
        /// DateTime until Subscription is Valid
        /// </summary>
        public DateTime? ExpiresOnUtc { get; private set; }

        /// <summary>
        /// Changes to this subscriptions, by e.g. extensions
        /// </summary>
        public IReadOnlyCollection<SubscriptionChange> SubscriptionChanges => _subscriptionChanges;
        
        /// <summary>
        /// Orders related to this subscription
        /// </summary>
        public IReadOnlyCollection<SubscriptionOrder> Orders => _orders;

        /// <summary>
        /// Creates a new Subscription or extents an existing one
        /// </summary>
        /// <param name="subscriptionPayment">The Payment instance</param>
        /// <returns></returns>
        internal Result<SubscriptionChange> CreateOrExtend(SubscriptionPayment subscriptionPayment)
        {
            DateTime startTime = DateTime.UtcNow;
            var changeOperation = SubscriptionChangeOperation.Undefined;

            //Is First Subscription
            if(StartOnUtc == null)
            {
                changeOperation = SubscriptionChangeOperation.InitialCreated;
            }
            //Is Extension or Renewal
            else
            {
                //Renewal
                if(startTime > ExpiresOnUtc)
                {
                    changeOperation = SubscriptionChangeOperation.Renewed;
                }
                //Extension
                else
                {
                    changeOperation = SubscriptionChangeOperation.Extend;
                    startTime = ExpiresOnUtc.Value;
                }
            }
            var retval = SubscriptionChange.Create(this, subscriptionPayment,startTime, changeOperation);
            if(retval.HasError()) return retval;

            StartOnUtc = retval.ReturnValue.ExtendsFromUtc;
            ExpiresOnUtc = retval.ReturnValue.ExtendsToUtc;
            return retval;
        }
    }
}
