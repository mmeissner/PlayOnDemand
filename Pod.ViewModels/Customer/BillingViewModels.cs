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
using Pod.Enums;

namespace Pod.ViewModels.Customer
{
    /// <summary>
    /// A Station ViewModel with the current Subscription information 
    /// </summary>
    public class StationSubscriptionViewModel
    {
        /// <summary>
        /// The System generated StationId
        /// </summary>
        public Guid StationId { get; set; }
        /// <summary>
        /// The Name of the Station
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// The subscription state of the station
        /// </summary>
        public SubscriptionState State { get; set; }
        /// <summary>
        /// The Date when the Subscription expires or null if non ever existed
        /// </summary>
        public DateTime? ExpiresOnUtc { get; set; }
    }

    /// <summary>
    /// An Request for an Subscription for an Station  
    /// </summary>
    public class UserRequestSubscriptionOrderViewModel
    {
        /// <summary>
        /// The Station Id the request was created for
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// The requested duration for that subscription
        /// </summary>
        public TimeSpan Duration { get; set; }
    }

    /// <summary>
    /// An reduced VM for an Order of a Subscription
    /// </summary>
    public class SubscriptionOrderBasicViewModel
    {
        /// <summary>
        /// The User for that this order was created
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The Station Id that order was created for
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// The Id of the Order 
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// The DateTime the order will expire
        /// </summary>
        public DateTime ExpiresOn { get; set; }

    }

    /// <summary>
    /// An Order for an subscription
    /// </summary>
    public class SubscriptionOrderViewModel
    {
        /// <summary>
        /// The Station Name during the time the Order was created
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// The DateTime the order was created on
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// The DateTime the order will expire
        /// </summary>
        public DateTime ExpiresOnUtc { get; set; }

        /// <summary>
        /// The duration for the ordered subscription
        /// </summary>
        public TimeSpan OrderedDuration { get; set; }

        /// <summary>
        /// The amount to be paid for the order
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// The currency in that the amount is to be paid
        /// </summary>
        public CurrencyIsoCode Currency { get; set; }

        /// <summary>
        /// A reference provided by the user that allows him to identify this order
        /// </summary>
        public string CustomerReference { get; set; }

        /// <summary>
        /// An internal reference created by the system that allows to identify the order on an purchase
        /// </summary>
        public string PaymentReference { get; set; }

    }

    /// <summary>
    /// Represents an Payment for an Ordered Subscription
    /// </summary>
    public class SubscriptionPaymentViewModel
    {
        /// <summary>
        /// The Station Name during the Order for that the payment were done
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// The DataTime of the Order this payment is for
        /// </summary>
        public DateTime OrderedOnUtc { get; set; }

        /// <summary>
        /// The DateTime the payment was received
        /// </summary>
        public DateTime PayedOnUtc { get; set; }

        /// <summary>
        /// The Amount Payed 
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// The currency in that the payment was done
        /// </summary>
        public CurrencyIsoCode Currency { get; set; }

        /// <summary>
        /// The subscription duration that was ordered and payed for
        /// </summary>
        public TimeSpan OrderedDuration { get; set; }

        /// <summary>
        /// A reference provided by the user that allows him to identify this order
        /// </summary>
        public string CustomerReference { get; set; }

        /// <summary>
        /// An internal reference created by the system that allows to identify the order on an purchase
        /// </summary>
        public string PaymentReference { get; set; }
    }
}
