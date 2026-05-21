#region Licence
/****************************************************************
 *  Filename: RegistryUtil.cs
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
using System.Linq;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;
using NLog;

namespace LeapVR.Utilities.Windows
{
    public static class RegistryUtil
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        /// <param name="key">The key e.g. "HKEY_LOCAL_MACHINE\SOFTWARE\MyApplication\AppPath",
        /// CurrentUser or DynData or ClassesRoot are not supported</param>
        /// <param name="value">The value e.g Installed </param>
        /// <returns>the data or null</returns>
        public static string GetValueData(string key, string value)
        {
            Logger.Debug($"Key:{key}, value: {value}");
            var temp = Registry.GetValue(key, value, null);
            Logger.Debug($"Data received for value ={value}, data={temp}");
            return temp?.ToString();
        }

        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        /// <param name="hive">The hive,CurrentUser or DynData or ClassesRoot are not supported</param>
        /// <param name="key">The key e.g. "SOFTWARE\MyApplication\AppPath" WITHOUT the Hive!,
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetValueData(RegistryHive hive, string key, string value)
        {
            Logger.Debug($"Hive:{hive}, Key:{key}, value: {value}");
            return GetValueData($"{GetHiveString(hive)}\\{key}", value);
        }

        /// <summary>
        /// Checks if an Key exists.
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static bool KeyExist(RegistryHive hive, string key)
        {
            RegistryKey result;
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    result = Registry.ClassesRoot.OpenSubKey(key);
                    break;
                case RegistryHive.CurrentUser:
                    result = Registry.CurrentUser.OpenSubKey(key);
                    break;
                case RegistryHive.LocalMachine:
                    result = Registry.LocalMachine.OpenSubKey(key);
                    break;
                case RegistryHive.Users:
                    result = Registry.Users.OpenSubKey(key);
                    break;
                case RegistryHive.PerformanceData:
                    result = Registry.PerformanceData.OpenSubKey(key);
                    break;
                case RegistryHive.CurrentConfig:
                    result = Registry.CurrentConfig.OpenSubKey(key);
                    break;
                case RegistryHive.DynData:
                    throw new NotSupportedException("DynData is not supported!");
                default:
                    throw new ArgumentOutOfRangeException(nameof(hive), hive, null);
            }
            if (result == null)
            {
                Logger.Debug($"Key:{key} does not exist!");
                return false;
            }
            Logger.Debug($"Key:{key} exist!");
            return true;
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="key">The key in format e.g.:SOFTWARE\Valve\Steam\Apps\397610</param>
        /// <returns></returns>
        public static string[] GetValues(RegistryHive hive, string key)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    return Registry.ClassesRoot.OpenSubKey(key)?.GetValueNames();
                case RegistryHive.CurrentUser:
                    return Registry.CurrentUser.OpenSubKey(key)?.GetValueNames();
                case RegistryHive.LocalMachine:
                    return Registry.LocalMachine.OpenSubKey(key)?.GetValueNames();
                case RegistryHive.Users:
                    return Registry.Users.OpenSubKey(key)?.GetValueNames();
                case RegistryHive.PerformanceData:
                    return Registry.PerformanceData.OpenSubKey(key)?.GetValueNames();
                case RegistryHive.CurrentConfig:
                    return Registry.CurrentConfig.OpenSubKey(key)?.GetValueNames();
                default:
                    throw new ArgumentOutOfRangeException(nameof(hive), hive, null);
            }
        }

        /// <summary>
        /// Gets the representative string for the Hive Enum.
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <returns></returns>
        public static string GetHiveString(RegistryHive hive)
        {
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    return "HKEY_CLASSES_ROOT";
                case RegistryHive.CurrentUser:
                    return "HKEY_CURRENT_USER";
                case RegistryHive.LocalMachine:
                    return "HKEY_LOCAL_MACHINE";
                case RegistryHive.Users:
                    return "HKEY_USERS";
                case RegistryHive.PerformanceData:
                    return "HKEY_PERFORMANCE_DATA";
                case RegistryHive.CurrentConfig:
                    return "HKEY_CURRENT_USER";
                case RegistryHive.DynData:
                    return "HKEY_DYN_DATA";
                default:
                    throw new ArgumentOutOfRangeException(nameof(hive), hive, null);
            }
        }

        /// <summary>
        /// Gets the hive enum.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static RegistryHive GetHiveEnum(string key)
        {
            var parts = key.Split('\\');
            if (parts.Length < 1)
            {
                throw new ArgumentException($"The provided string does not contain an valid Hive key! {key}");
            }
            switch (parts[0])
            {
                case "HKEY_CURRENT_USER":
                    return RegistryHive.CurrentUser;
                case "HKEY_LOCAL_MACHINE":
                    return RegistryHive.LocalMachine;
                case "HKEY_CLASSES_ROOT":
                    return RegistryHive.ClassesRoot;
                case "HKEY_USERS":
                    return RegistryHive.CurrentUser;
                case "HKEY_PERFORMANCE_DATA":
                    return RegistryHive.PerformanceData;
                case "HKEY_CURRENT_CONFIG":
                    return RegistryHive.CurrentConfig;
                case "HKEY_DYN_DATA":
                    return RegistryHive.DynData;
                default:
                    throw new ArgumentException($"The provided string does not contain an valid Hive key! {key}");
            }
        }

        public static bool SetValue(RegistryHive hive, string key,string valueName,object value,RegistryValueKind kind)
        {
            Registry.SetValue($"{GetHiveString(hive)}\\{key}",valueName,value,kind);
            var result = GetValueData(hive, key, valueName);
            if(result == null) return false;
            return true;
        }
    }
    public class RegistryWatcher : RegistryWatcherBase, IDisposable
    {
        public delegate void RegistryEntryChangedEventArgs(string sender, string newData);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ManagementEventWatcher _watcher;
        private string _getKey;
        private string _getValue;
        private WqlEventQuery _query;
        private bool _started;

        public RegistryWatcher(string key, string value)
        {
            var hive = RegistryUtil.GetHiveEnum(key);
            key = key.Replace($"{RegistryUtil.GetHiveString(hive)}\\", "");
            Initialize(hive, key, value);
        }

        public RegistryWatcher(RegistryHive hive, string key, string value)
        {
            Initialize(hive, key, value);
        }

        private void Initialize(RegistryHive hive, string key, string value)
        {
            Logger.Debug($"Called with Hive: {hive}, Key: {key}, Value:{value}");
            _getKey = $"{ResolveHiveForWmiRegistryEvent(hive)}\\{key}";
            _getValue = value;

            if (hive == RegistryHive.ClassesRoot || hive == RegistryHive.DynData)
            {
                throw new NotSupportedException("Hive is not supported, see: https://msdn.microsoft.com/en-us/library/aa393040(v=vs.85).aspx");
            }

            //Current User is just a pointer, so that this registry path actually does not exists
            if (hive == RegistryHive.CurrentUser)
            {
                _query = new WqlEventQuery($"SELECT * FROM RegistryValueChangeEvent WHERE Hive='{RegistryUtil.GetHiveString(RegistryHive.Users)}'" + $" AND KeyPath='{WindowsIdentity.GetCurrent()?.User?.Value}\\\\{key.Replace(@"\", @"\\")}'" + $" AND ValueName='{value}'");
            }
            else
            {
                _query = new WqlEventQuery($"SELECT * FROM RegistryKeyChangeEvent WHERE Hive='{RegistryUtil.GetHiveString(hive)}'" + $" AND KeyPath='{key.Replace(@"\", @"\\")}'" + $" AND ValueName='{value}'");
            }
            Logger.Debug($"QueryString is: {_query.QueryString}");
        }

        public void Start()
        {
            if (_started) return;
            _watcher = new ManagementEventWatcher(_query);
            _watcher.EventArrived += WatcherOnEventArrived;
            _watcher.Start();
            _started = true;
        }

        public void Stop()
        {
            if (!_started) return;
            Logger.Debug($"Stop with watcher is null: {_watcher == null}");
            _watcher?.Stop();
            _started = false;
        }

        private void WatcherOnEventArrived(object sender, EventArrivedEventArgs eventArrivedEventArgs)
        {
            Logger.Debug($"WatcherEvent Arrived, trying to get key: {_getKey}, value:{_getValue}");
            var newData = RegistryUtil.GetValueData(_getKey, _getValue);
            Logger.Debug($"New Data is: {newData}");
            RegistryEntryChanged(newData);
        }

        public event RegistryEntryChangedEventArgs OnRegistryEntryChanged;

        protected virtual void RegistryEntryChanged(string newData)
        {
            OnRegistryEntryChanged?.Invoke(_getValue, newData);
        }

        public void Dispose()
        {
            Logger.Debug($"Dispose called with watcher is Null: {_watcher == null}");
            _watcher?.Stop();
        }
    }
    public class RegistryWatcherMultiValue : RegistryWatcherBase, IDisposable
    {
        public delegate void RegistryEntryChangedEventExtArgs(string value, string oldData, string newData);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private ManagementEventWatcher _watcher = null;
        private string _getKey;
        private RegistryHive _getHive;
        private List<string> _filteredValues;
        private List<string> _registeredValues;
        private readonly Dictionary<string, string> _currentKeysAndValues = new Dictionary<string, string>();
        private bool _isObserving = false;
        public RegistryWatcherMultiValue(string key, string[] values)
        {
            var hive = RegistryUtil.GetHiveEnum(key);
            key = key.Replace($"{RegistryUtil.GetHiveString(hive)}\\", "");
            Initialize(hive, key, values);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryWatcherMultiValue" /> class.
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="key">The key. e.g.: "HKEY_LOCAL_MACHINE\SOFTWARE\MyApplication\AppPath" </param>
        /// <param name="values">The values.</param>
        public RegistryWatcherMultiValue(RegistryHive hive, string key, string[] values)
        {
            Initialize(hive, key, values);
        }

        private void Initialize(RegistryHive hive, string key, string[] values)
        {

            Logger.Debug($"Initializing with Hive: {hive}, key:{key}");
            string query;
            _getKey = $"{key}";
            _getHive = hive;
            if (hive == RegistryHive.ClassesRoot || hive == RegistryHive.DynData)
            {
                throw new NotSupportedException("Hive is not supported, see: https://msdn.microsoft.com/en-us/library/aa393040(v=vs.85).aspx");
            }
            //Current User is just a pointer, so that this registry path actually does not exists
            if (hive == RegistryHive.CurrentUser)
            {
                query = $"SELECT * FROM RegistryKeyChangeEvent WHERE Hive='{RegistryUtil.GetHiveString(RegistryHive.Users)}'" + $" AND KeyPath='{WindowsIdentity.GetCurrent()?.User?.Value}\\\\{key.Replace(@"\", @"\\")}'";
            }
            else
            {
                query = $"SELECT * FROM RegistryKeyChangeEvent WHERE Hive='{RegistryUtil.GetHiveString(_getHive)}'" + $" AND KeyPath='{key.Replace(@"\", @"\\")}'";
            }

            //Check if values exist and filter out the not existend
            var availibleValues = RegistryUtil.GetValues(_getHive, _getKey);
            if (availibleValues == null)
            {
                Logger.Error($"Could not find any Values in registry for hive= {_getHive}; and key = {_getKey}");
                return;
            }
            var existingValues = availibleValues.ToList();
            _filteredValues = new List<string>(values);
            foreach (string requestedValue in values)
            {
                //Remove all non exisiting Values
                if (!existingValues.Contains(requestedValue))
                {
                    Logger.Warn($"This value does not seem to exist: {requestedValue} and will be not observed!");
                    _filteredValues.Remove(requestedValue);
                }
            }
            _registeredValues = new List<string>(_filteredValues);

            //Get the current Data from the Values
            foreach (string value in _filteredValues)
            {
                var data = RegistryUtil.GetValueData(_getHive, _getKey, value);
                _currentKeysAndValues.Add(value, data);
            }
            WqlEventQuery eventquery = new WqlEventQuery(query);
            //WORKING -> WqlEventQuery query= new WqlEventQuery($"SELECT * FROM RegistryKeyChangeEvent WHERE Hive='{RegistryProcessor.GetHiveString(hive)}'" + $" AND KeyPath='{WindowsIdentity.GetCurrent()?.User?.Value}\\\\SOFTWARE\\\\Valve\\\\Steam\\\\Apps\\\\397610'");

            _watcher = new ManagementEventWatcher(eventquery);
            _watcher.EventArrived += WatcherOnEventArrived;
            Logger.Debug("Starting Watcher!");
            _watcher.Start();
            _isObserving = true;
        }

        public List<string> RegistredValues => _registeredValues;
        public bool IsObserving => _isObserving;
        public void Stop()
        {
            Logger.Debug($"Dispose called with watcher is Null: {_watcher == null}");
            _watcher?.Stop();
        }

        private void WatcherOnEventArrived(object sender, EventArrivedEventArgs eventArrivedEventArgs)
        {
            //We must check what changed by ourselfs
            //We have no idea if the Events comes in order and if that value that we return is really that ones that changed
            //at this moment, but at least we should receive as many events as the values change
            foreach (string requiredValue in _filteredValues)
            {
                var data = RegistryUtil.GetValueData(_getHive, _getKey, requiredValue);
                if (_currentKeysAndValues[requiredValue] != data)
                {
                    var oldData = _currentKeysAndValues[requiredValue];
                    _currentKeysAndValues[requiredValue] = data;
                    Logger.Debug($"Value has changed for {requiredValue}, with oldData:{oldData}, newData:{data}!");
                    RegistryEntryChanged(requiredValue, oldData, data);
                    return;
                }
            }
        }

        public event RegistryEntryChangedEventExtArgs OnRegistryEntryChanged;

        protected virtual void RegistryEntryChanged(string value, string oldData, string newData)
        {
            OnRegistryEntryChanged?.Invoke(value, oldData, newData);
        }


        public void Dispose()
        {
            Logger.Debug($"Disposing for isWatcher Null= {_watcher == null}!");
            _watcher?.Stop();
            _watcher?.Dispose();
        }
    }
    public abstract class RegistryWatcherBase
    {
        /// <summary>
        /// Resolves the hive for registry.
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <returns></returns>
        public static string ResolveHiveForWmiRegistryEvent(RegistryHive hive)
        {
            string retval = "";
            if (hive == RegistryHive.ClassesRoot || hive == RegistryHive.DynData)
            {
                throw new NotSupportedException("Hive is not supported, see: https://msdn.microsoft.com/en-us/library/aa393040(v=vs.85).aspx");
            }
            //Current User is just a pointer, so that this registry path actually does not exists
            if (hive == RegistryHive.CurrentUser)
            {
                retval = $"{RegistryUtil.GetHiveString(RegistryHive.Users)}\\{WindowsIdentity.GetCurrent()?.User?.Value}";
            }
            else
            {
                retval = RegistryUtil.GetHiveString(RegistryHive.Users);
            }
            return retval;
        }
    }

}
