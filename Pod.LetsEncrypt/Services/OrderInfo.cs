#region Licence
/****************************************************************
 *  Filename: OrderInfo.cs
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
using Certes.Acme;

namespace Pod.LetsEncrypt.Services
{
    /// <summary>
    /// Holds information for an Certificate Request with LetsEncrypt
    /// The objects are defined by the ACME Client
    /// </summary>
    public class OrderInfo
    {
        /// <summary>
        /// Holds all the information about the Order for the certificate
        /// </summary>
        public IOrderContext Order { get; set; }


        /// <summary>
        /// Holds all the information about the Challenge for the Order
        /// </summary>
        public IChallengeContext Challenge { get; set; }

        /// <summary>
        /// The DomainName the Order and Challenge belongs to
        /// </summary>
        public string DomainName { get; set; }
    }
}
