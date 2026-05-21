#region Licence
/****************************************************************
 *  Filename: UtcDateTimeConverter.cs
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
using System.Text;

namespace Pod.Data.Converter
{
    /// <summary>
    /// Helper class to convert Database Time values to .Net values
    /// </summary>
    public static class UtcDateTimeConverter
    {
        /// <summary>
        /// Adds the <see cref="DateTimeKind.Utc"/> info to received DateTimes from Db
        /// </summary>
        /// <param name="fromDb">The nullable value from the Db</param>
        /// <returns>The nullable DateTime with Utc kind</returns>
        public static DateTime? ConvertDateTime(DateTime? fromDb)
        {
            if(!fromDb.HasValue) return null;
            return DateTime.SpecifyKind(fromDb.Value, DateTimeKind.Utc);
        }

        /// <summary>
        /// Adds the <see cref="DateTimeKind.Utc"/> info to received DateTimes from Db
        /// </summary>
        /// <param name="fromDb">The value from the Db</param>
        /// <returns>The DateTime with Utc kind</returns>
        public static DateTime ConvertDateTime(DateTime fromDb)
        {
            return DateTime.SpecifyKind(fromDb, DateTimeKind.Utc);
        }
    }
}
