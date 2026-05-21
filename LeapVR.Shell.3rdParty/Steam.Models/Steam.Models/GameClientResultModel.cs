#region Licence
/****************************************************************
 *  Filename: GameClientResultModel.cs
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
namespace Steam.Models
{
    public class GameClientResultModel
    {
        public bool Success { get; set; }

        public int DeployVersion { get; set; }

        public int ActiveVersion { get; set; }
    }
}