#region Licence
/****************************************************************
 *  Filename: ConfigFileRepository.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.IO;
using System.Reflection;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Customization;
using Newtonsoft.Json;
using NLog;
using Pod.Data.Infrastructure;

namespace LeapVR.Shell.Modules
{
    public class ConfigFileRepository<T> : IConfigFileRepository<T> where T : IConfigObject, new()
    {
        #region Ctors
        public ConfigFileRepository()
        {
            ConfigDirectory = GlobalConfig.GetGlobalConfiguration().ConfigFilesDirectory;
            if(!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);
        }
        #endregion

        public T Get() { return Get(true); }
        /// <summary>
        ///     Read the text from {ConfigType}.json file and mapping the settings to object that is ready to use.
        ///     If the file does not exists, a new object with initial setting values will be created and stored.
        ///     If there is already the config file, it will check structure changes and reserve the old values to merge config
        ///     from different versions and store it instantly.
        /// </summary>
        /// <returns>value of the config type <see cref="T" />.</returns>
        public T Get(bool allowCache)
        {
            try
            {
                lock(_locker)
                {
                    if(_isInitialized && allowCache)
                    {
                        Logger.Debug( $"Returning cached config Object for {typeof(T).FullName}.");
                        return _configObject.CloneJson();
                    }

                    if(string.IsNullOrEmpty(FileName))
                        _configFilePath = $"{Path.Combine(ConfigDirectory, typeof(T).FullName)}.json";
                    else _configFilePath = $"{Path.Combine(ConfigDirectory, FileName)}";

                    var isFileNew = false;
                    if(File.Exists(_configFilePath))
                    {
                        var text = File.ReadAllText(_configFilePath);
                        _configObject = JsonConvert.DeserializeObject<T>(
                                text,
                                new JsonSerializerSettings {ObjectCreationHandling = ObjectCreationHandling.Auto});
                    }
                    else
                    {
                        _configObject = new T();
                        isFileNew = true;
                    }

                    _configObject.Initialize();
                    if(isFileNew)
                    {
                        Logger.Info( $"Config of {typeof(T).FullName} is new and has default values!");
                    }

                    _isInitialized = true;
                    Logger.Debug($"Config of {typeof(T).FullName} is now initialized!");
                    return _configObject;
                }
            }
            catch(System.Exception e)
            {
                Logger.Fatal(e, $"Configuration File for {typeof(T).FullName} could not be initialized!");
                throw;
            }
        }

        /// <summary>
        ///     Overwrites currently stored config object with <see cref="objToStore" />.
        /// </summary>
        /// <param name="objToStore">New config object to store</param>
        /// <returns>Boolean indicating success or failure.</returns>
        public bool Store(T objToStore)
        {
            try
            {
                lock(_locker)
                {
                    File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(objToStore, Formatting.Indented));
                    Logger.Info(
                            $"Config of {typeof(T).FullName} is going to be saved now, going to save it!");
                    _configObject = objToStore;
                }

                return true;
            }
            catch(System.Exception e)
            {
                Logger.Fatal(e, $"Configuration File for {typeof(T).FullName} could not be saved!");
                throw;
            }
        }

        #region Fields & Properties 
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected readonly string ConfigDirectory;
        protected string FileName;

        private T _configObject;
        private string _configFilePath;

        private volatile bool _isInitialized;
        private readonly object _locker = new object();
        #endregion
    }
}