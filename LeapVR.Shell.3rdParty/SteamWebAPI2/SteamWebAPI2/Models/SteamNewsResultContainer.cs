#region Licence
/****************************************************************
 *  Filename: SteamNewsResultContainer.cs
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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamWebAPI2.Models
{
    internal class NewsItem
    {
        [JsonProperty("gid")]
        public string Gid { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("is_external_url")]
        public bool IsExternalUrl { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("contents")]
        public string Contents { get; set; }

        [JsonProperty("feedlabel")]
        public string FeedLabel { get; set; }

        [JsonProperty("date")]
        public int Date { get; set; }

        [JsonProperty("feedname")]
        public string Feedname { get; set; }
    }

    internal class SteamNewsResult
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("newsitems")]
        public IList<NewsItem> NewsItems { get; set; }
    }

    internal class SteamNewsResultContainer
    {
        [JsonProperty("appnews")]
        public SteamNewsResult Result { get; set; }
    }
}