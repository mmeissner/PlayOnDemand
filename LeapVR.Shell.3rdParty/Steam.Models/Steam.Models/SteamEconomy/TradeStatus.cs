#region Licence
/****************************************************************
 *  Filename: TradeStatus.cs
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
    /// Tracks the status of a trade after a trade offer has been accepted.
    /// </summary>
    public enum TradeStatus
    {
        Init = 0,
        PreCommitted = 1,
        Committed = 2,
        Complete = 3,
        Failed = 4,
        PartialSupportRollback = 5,
        FullSupportRollback = 6,
        SupportRollbackSelective = 7,
        RollbackFailed = 8,
        RollbackAbandoned = 9,
        InEscrow = 10,
        EscrowRollback = 11
    }
}