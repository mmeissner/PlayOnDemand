#region Licence
/****************************************************************
 *  Filename: SchemaQualitiesModel.cs
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

namespace Steam.Models.TF2
{
    public class SchemaQualitiesModel
    {
        /// <summary>
        /// Normal item rarity: https://wiki.teamfortress.com/wiki/Normal
        /// </summary>
        public int Normal { get; set; }

        /// <summary>
        /// Genuine item rarity: https://wiki.teamfortress.com/wiki/Genuine
        /// </summary>
        public int Rarity1 { get; set; }

        /// <summary>
        /// Unused
        /// </summary>
        public int Rarity2 { get; set; }

        /// <summary>
        /// Vintage item rarity: https://wiki.teamfortress.com/wiki/Vintage
        /// </summary>
        public int Vintage { get; set; }

        /// <summary>
        /// Unused
        /// </summary>
        public int Rarity3 { get; set; }

        /// <summary>
        /// Unusual item rarity: https://wiki.teamfortress.com/wiki/Unusual
        /// </summary>
        public int Rarity4 { get; set; }

        /// <summary>
        /// Unique item rarity: https://wiki.teamfortress.com/wiki/Unique
        /// </summary>
        public int Unique { get; set; }

        /// <summary>
        /// Community item: https://wiki.teamfortress.com/wiki/Community_(quality)
        /// </summary>
        public int Community { get; set; }

        /// <summary>
        /// Developer owned item: https://wiki.teamfortress.com/wiki/Valve_(quality)
        /// </summary>
        public int Developer { get; set; }

        /// <summary>
        /// Self made item: https://wiki.teamfortress.com/wiki/Self-Made
        /// </summary>
        public int SelfMade { get; set; }

        /// <summary>
        /// Unused
        /// </summary>
        public int Customized { get; set; }

        /// <summary>
        /// Strange item: https://wiki.teamfortress.com/wiki/Strange
        /// </summary>
        public int Strange { get; set; }

        /// <summary>
        /// Unused
        /// </summary>
        public int Completed { get; set; }

        /// <summary>
        /// Haunted item: https://wiki.teamfortress.com/wiki/Haunted
        /// </summary>
        public int Haunted { get; set; }

        /// <summary>
        /// Collector's item: https://wiki.teamfortress.com/wiki/Collector%27s
        /// </summary>
        public int Collectors { get; set; }
        
        public int PaintKitWeapon { get; set; }
    }
}
