#region Licence
/****************************************************************
 *  Filename: UnixTimeJsonConverter.cs
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
using System;
using System.Reflection;

namespace SteamWebAPI2.Utilities.JsonConverters
{
    internal class UnixTimeJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long unixTime = long.Parse(reader.Value.ToString());
            return unixTime.ToDateTime();
        }

        public override bool CanWrite { get { return false; } }

        public override bool CanConvert(Type objectType)
        {
            return typeof(long).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }
    }
}