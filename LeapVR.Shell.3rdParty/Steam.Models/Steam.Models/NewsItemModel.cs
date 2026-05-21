#region Licence
/****************************************************************
 *  Filename: NewsItemModel.cs
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
    public class NewsItemModel
    {
        public string Gid { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public bool IsExternalUrl { get; set; }

        public string Author { get; set; }

        public string Contents { get; set; }

        public string FeedLabel { get; set; }

        public int Date { get; set; }

        public string Feedname { get; set; }
    }
}