#region Licence
/****************************************************************
 *  Filename: OwnedGameModel.cs
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
namespace Steam.Models.SteamCommunity
{
    public class OwnedGameModel
    {
        public int AppId { get; set; }

        public string Name { get; set; }

        public int PlaytimeForever { get; set; }

        public string ImgIconUrl { get; set; }

        public string ImgLogoUrl { get; set; }

        public bool HasCommunityVisibleStats { get; set; }

        public int? Playtime2weeks { get; set; }
    }
}