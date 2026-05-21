#region Licence
/****************************************************************
 *  Filename: DotaBlogFeedItem.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Models.DOTA2
{
    public class DotaBlogFeedItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime PublishDate { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string ContentEncoded { get; set; }
        public string ImageUrl { get; set; }
    }
}
