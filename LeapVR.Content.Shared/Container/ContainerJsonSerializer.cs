#region Licence
/****************************************************************
 *  Filename: ContainerJsonSerializer.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2018-1-22
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Linq;
using System.Reflection;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeapVR.Content.Shared.Container
{

    public static class ContainerJsonSerializer
    {
        private static readonly JsonConverter[] TypeConverters =
        {
                new InterfaceConverter<IDiskEntityDto, DiskEntityDto>(),
                new InterfaceConverter<IProcessMonitorInstructionDto, ProcessMonitorInstructionDto>(),
                new InterfaceConverter<IProcessExecutionLogicDto, ProcessExecutionLogicDto>(),
                new InterfaceConverter<IAppPlatformDataDto, AppPlatformDataDto>(),
                new InterfaceConverter<IAppDisplayDataDto, AppDisplayDataDto>(),
        };

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, TypeConverters);
        }

        public static string SerializeObject(
                object value,
                Formatting formatting = Formatting.Indented,
                JsonSerializerSettings settings = null)
        {
            settings = settings ?? new JsonSerializerSettings();
            settings.Converters = (settings.Converters ?? new JsonConverter[0]).Concat(TypeConverters).ToArray();

            return JsonConvert.SerializeObject(value, formatting, TypeConverters);
        }
    }


    public class InterfaceConverter<TInterface, TImplementation> : JsonConverter
            where TImplementation : class, TInterface, new()
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value == null)
            {
                writer.WriteToken(JsonToken.Null);
                return;
            }

            var jsonObject = JObject.FromObject(value);
            jsonObject.WriteTo(writer);
        }

        public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
        {
            if(reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var implementation = new TImplementation();
            serializer.Populate(reader, implementation);
            return implementation;
        }

        public override bool CanConvert(Type objectType) { return typeof(TInterface).IsAssignableFrom(objectType); }
    }

    //public class ExecutionLogicConverter : JsonConverter
    //{
    //    private const string _typePropertyName = "Type";
    //    private static readonly Dictionary<Type, (string Name, Func<IExecutionLogic> Factory)> TypeDiscriminatorMap = new Dictionary<Type, (string Name, Func<IExecutionLogic> Factory)>
    //    {
    //        { typeof(IProcessExecutionLogic), (Name: "ProcessExecutionLogic", Factory: () => new ProcessExecutionLogicDto()) },
    //    };
    //    //private static readonly Dictionary<Type, (string Name, Func<IExecutionLogic> Factory)> TypeDiscriminatorMap = new Dictionary<Type, (string Name, Func<IExecutionLogic> Factory)>
    //    //{
    //    //    { typeof(IProcessExecutionLogic), (Name: "ProcessExecutionLogic", Factory: () => new ProcessExecutionLogicDto()) },
    //    //};

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        if (value == null)
    //        {
    //            writer.WriteToken(JsonToken.Null);
    //            return;
    //        }

    //        var jsonObject = JObject.FromObject(value);

    //        string typeDiscriminator = TypeDiscriminatorMap
    //            .Single(kv => kv.Key.IsAssignableFrom(value?.GetType())).Value.Name;
    //        jsonObject.AddFirst(new JProperty(_typePropertyName, typeDiscriminator));

    //        jsonObject.WriteTo(writer);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null)
    //        {
    //            return null;
    //        }

    //        var jsonObject = JObject.Load(reader);

    //        string typeDiscriminator = jsonObject[_typePropertyName].Value<string>();
    //        IExecutionLogic result = TypeDiscriminatorMap
    //            .Single(kv => kv.Value.Name == typeDiscriminator).Value.Factory();
    //        serializer.Populate(jsonObject.CreateReader(), result);

    //        return result;
    //    }

    //    public override bool CanConvert(Type objectType)
    //    {
    //        return typeof(IExecutionLogic).IsAssignableFrom(objectType);
    //    }
    //}
}