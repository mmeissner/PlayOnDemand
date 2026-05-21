#region Licence
/****************************************************************
 *  Filename: Extensions.cs
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
using Newtonsoft.Json;

namespace Pod.Data.Infrastructure
{
    /// <summary>
    /// Extensions providing general supporting functionalities
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialization method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if(Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings
                                      {ObjectCreationHandling = ObjectCreationHandling.Replace};

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        /// <summary>
        /// Converts a Object to Json
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>The object converted to json as string</returns>
        public static string ToJson(this object obj) { return JsonConvert.SerializeObject(obj, Formatting.Indented); }

        /// <summary>
        /// Converts the object to Json with a format suitable for logging  
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>The json object for logging as string</returns>
        public static string LogJson(this object obj)
        {
            try
            {
                return Environment.NewLine +
                       obj.GetType() +
                       Environment.NewLine +
                       JsonConvert.SerializeObject(
                               obj,
                               Formatting.Indented,
                               new JsonSerializerSettings()
                               {
                                       Converters = new List<JsonConverter>
                                                    {
                                                            new Newtonsoft.Json.Converters.
                                                                    StringEnumConverter()
                                                    }
                               });
            }
            catch(Exception exception)
            {
                return exception.Message;
            }
        }
    }
}