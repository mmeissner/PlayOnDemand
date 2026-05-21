#region Licence
/****************************************************************
 *  Filename: SubscriptionChangeOperation.cs
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
namespace Pod.Enums 
{
    /// <summary>
    /// Defines the ChangeOperations effect
    /// </summary>
    public enum SubscriptionChangeOperation
    {
        /// <summary>
        /// Invalid
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// The first subscription
        /// </summary>
        InitialCreated = 10,

        /// <summary>
        /// The subscription was inactive and was now renewed
        /// </summary>
        Renewed = 20,

        /// <summary>
        /// The subscription was extended before it became inactive
        /// </summary>
        Extend = 30,
    }
}