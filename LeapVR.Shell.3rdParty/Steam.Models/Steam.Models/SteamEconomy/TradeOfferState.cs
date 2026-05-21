#region Licence
/****************************************************************
 *  Filename: TradeOfferState.cs
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
    /// These are the different states for a trade offer.
    /// </summary>
    public enum TradeOfferState
    {
        Invalid = 1,
        Active = 2,
        Accepted = 3,
        Countered = 4,
        Expired = 5,
        Canceled = 6,
        Declined = 7,
        InvalidItems = 8,
        CreateNeedsConfirmation = 9,
        CanceledBySecondFactor = 10,
        InEscrow = 11
    }
}