#region Licence
/****************************************************************
 *  Filename: ResolutionDivider.cs
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
namespace Unosquare.FFME.Shared
{
    /// <summary>
    /// Enumerates the different low reolution divider indices.
    /// </summary>
    public enum ResolutionDivider
    {
        /// <summary>
        /// Represents no resolution reduction
        /// </summary>
        Full = 0,

        /// <summary>
        /// Represents 1/2 resolution
        /// </summary>
        Half = 1,

        /// <summary>
        /// Represents 1/4 resolution
        /// </summary>
        Quarter = 2,

        /// <summary>
        /// Represents 1/8 resolution
        /// </summary>
        Eighth = 3
    }
}
