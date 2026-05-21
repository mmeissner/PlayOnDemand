#region Licence
/****************************************************************
 *  Filename: CSGODataCenterJsonConverter.cs
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
using Newtonsoft.Json.Linq;
using SteamWebAPI2.Models.CSGO;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SteamWebAPI2.Utilities.JsonConverters
{
    internal class CSGODataCenterJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            List<ServerStatusDatacenter> dataCenters = new List<ServerStatusDatacenter>();

            JObject o = JObject.Load(reader);

            foreach (var x in o)
            {
                ServerStatusDatacenter dataCenter = new ServerStatusDatacenter()
                {
                    Name = x.Key,
                    Capacity = x.Value.Value<string>("capacity") ?? "unknown",
                    Load = x.Value.Value<string>("load") ?? "unknown"
                };

                dataCenters.Add(dataCenter);
            }

            return dataCenters;
        }

        public override bool CanWrite { get { return false; } }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ServerStatusDatacenter).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }
    }
}