#region Licence
/****************************************************************
 *  Filename: TradeOfferResultModel.cs
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
using System.Collections.Generic;

namespace Steam.Models.SteamEconomy
{
    public class TradeOfferResultModel
    {
        /// <summary>
        /// A CEcon_TradeOffer representing the offer
        /// </summary>
        public TradeOfferModel TradeOffer { get; set; }

        /// <summary>
        /// If language was set, this will be a list of item display information. This is associated with the data in the items_to_receive and items_to_give lists via the classid / instanceid identifier pair.
        /// </summary>
        public IList<string> Descriptions { get; set; }
    }
}