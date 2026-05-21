#region Licence
/****************************************************************
 *  Filename: ViewModelEnums.cs
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
namespace Pod.ViewModels.Customer
{
    /// <summary>
    /// Provides States for an Subscription
    /// </summary>
    public enum SubscriptionState
    {
        /// <summary>
        /// There is no active subscription
        /// </summary>
        Inactive,
        /// <summary>
        /// There is an active subscription
        /// </summary>
        Active,
        /// <summary>
        /// An subscription existed but is expired
        /// </summary>
        Expired
    }
}
