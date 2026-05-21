#region Licence
/****************************************************************
 *  Filename: DetailedConcurrencyException.cs
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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Pod.Data.Exceptions
{
    /// <summary>
    /// Wrapper exception around <see cref="DbUpdateConcurrencyException"/> thrown by Entity Framework when Optimistic Concurrency fails.
    /// This wrapper contains more information than original exception in <see cref="HelperExceptions.Message"/> field to ease investigation.
    /// </summary>
    public class DetailedConcurrencyException : Exception
    {
        public DetailedConcurrencyException(string message, DbUpdateConcurrencyException innerException)
                : base(message, innerException)
        { }

        /// <summary>
        /// Method used to wrap <see cref="DbUpdateConcurrencyException"/> including more details about the causing concurrency problem.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static DetailedConcurrencyException Wrap(DbUpdateConcurrencyException exception)
        {
            var sb = new StringBuilder();
            foreach (var en in exception.Entries)
            {
                sb.Append($@"Type = `{en.Entity.GetType()}`, Object = `{JsonConvert.SerializeObject(en.CurrentValues.ToObject(), Formatting.Indented)}`; ");
            }

            return new DetailedConcurrencyException(sb.ToString(), exception);
        }
    }
}
