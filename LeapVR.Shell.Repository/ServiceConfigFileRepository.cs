#region Licence
/****************************************************************
 *  Filename: ServiceConfigFileRepository.cs
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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LeapVR.VBox.Modules.Interfaces.Repositories;
using LeapVR.VBox.Repository.Utilities;
using Newtonsoft.Json;

namespace LeapVR.VBox.Repository
{
    public class ServiceConfigFileRepository<T> : IConfigFileRepository<T> where T : ISerializable, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly string ConfigDirectory = Path.Combine(Environment.CurrentDirectory, "ServiceConfig");
        protected ServiceConfigFileRepository()
        {
            if (!Directory.Exists(ConfigDirectory)) Directory.CreateDirectory(ConfigDirectory);
        }
        public T Get()
        {
            var retval = new T();
            string configFilePath = $"{Path.Combine(ConfigDirectory, typeof(T).FullName)}.json";
            if (File.Exists(configFilePath))
            {
                var text = File.ReadAllText(configFilePath);
                retval = JsonConvert.DeserializeObject<T>(text);
            }
            return retval;
        }
        public bool Store(T objToStore)
        {
            try
            {
                File.WriteAllText($"{Path.Combine(ConfigDirectory, typeof(T).FullName)}.json", JsonConvert.SerializeObject(objToStore.ToJsonString(), Formatting.Indented));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
