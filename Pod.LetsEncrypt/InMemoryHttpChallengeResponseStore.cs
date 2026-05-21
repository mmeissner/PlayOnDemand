#region Licence
/****************************************************************
 *  Filename: InMemoryHttpChallengeResponseStore.cs
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
using System.Collections.Concurrent;
using Pod.LetsEncrypt.Services;

namespace Pod.LetsEncrypt
{
    /// <summary>
    /// Stores the ACME Challenges in memory
    /// </summary>
    internal class InMemoryHttpChallengeResponseStore : IHttpChallengeResponseStore
    {
        private ConcurrentDictionary<string, OrderInfo> _values = new ConcurrentDictionary<string, OrderInfo>();

        /// <summary>
        /// Adds a Challenge Response Token
        /// </summary>
        /// <param name="token">The token</param>
        /// <param name="orderInfo">Info about the challenge that was requested/orderd</param>
        public void AddChallengeResponse(string token, OrderInfo orderInfo)
            => _values.AddOrUpdate(token, orderInfo, (_, __) => orderInfo);

        /// <summary>
        /// Retrieve Challenge Response information
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderInfo"></param>
        /// <returns>Challenge Response Info</returns>
        public bool TryGetResponse(string token, out OrderInfo orderInfo)
            => _values.TryGetValue(token, out orderInfo);

        /// <summary>
        /// Removes all Stored Challenge Responses
        /// </summary>
        public void ClearPendingOrders()
        {
            _values.Clear();
        }
    }
}
