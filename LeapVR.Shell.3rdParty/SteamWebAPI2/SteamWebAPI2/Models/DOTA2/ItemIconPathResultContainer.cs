#region Licence
/****************************************************************
 *  Filename: ItemIconPathResultContainer.cs
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
namespace SteamWebAPI2.Models.DOTA2
{
    internal class ItemIconPathResult
    {
        public string Path { get; set; }
    }

    internal class ItemIconPathResultContainer
    {
        public ItemIconPathResult Result { get; set; }
    }
}