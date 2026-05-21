#region Licence
/****************************************************************
 *  Filename: IHttpChallengeResponseStore.cs
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
using Pod.LetsEncrypt.Services;

namespace Pod.LetsEncrypt
{
    public interface IHttpChallengeResponseStore
    {
        void AddChallengeResponse(string token, OrderInfo orderInfo);

        bool TryGetResponse(string token, out OrderInfo orderInfo);
        void ClearPendingOrders();
    }
}
