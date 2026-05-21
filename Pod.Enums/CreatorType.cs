#region Licence
/****************************************************************
 *  Filename: CreatorType.cs
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
namespace Pod.Enums
{
    /// <summary>
    /// Defines the Origin of a Data set
    /// </summary>
    public enum CreatorType
    {
        /// <summary>
        /// Invalid Value
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Data is created from by a Station
        /// </summary>
        Station = 10,
    }
}