#region Licence
/****************************************************************
 *  Filename: TradeOfferConfirmationMethod.cs
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
namespace Steam.Models.SteamEconomy
{
    /// <summary>
    /// These are the different methods in which a trade offer can be confirmed.
    /// </summary>
    public enum TradeOfferConfirmationMethod
    {
        Invalid = 0,
        Email = 1,
        MobileApp = 2
    }
}