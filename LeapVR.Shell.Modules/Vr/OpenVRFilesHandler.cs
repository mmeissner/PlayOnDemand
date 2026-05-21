#region Licence
/****************************************************************
 *  Filename: OpenVRFilesHandler.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces.Repositories;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Utilities.Steam.Steam;
using LeapVR.Utilities.Windows.JsonHelper;
using Newtonsoft.Json;
using NLog;

namespace LeapVR.Shell.Modules.Vr
{
    internal class OpenVRFilesHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IConfigFileRepository<OpenVrModuleConfig> _openVrModuleConfigRepository;
        private readonly IOpenVrSettingsSetRepository _openVrSettingsSetRepository;
        private readonly SteamVrConfigFile _steamVrConfigFile;
        private readonly OpenVrModuleConfig _openVrModuleConfig;
        private readonly SteamLib _steamLib;
        public OpenVRFilesHandler(
                IConfigFileRepository<OpenVrModuleConfig> openVrModuleConfigRepository,
                IOpenVrSettingsSetRepository openVrSettingsSetRepository,
                SteamLib steamLib)
        {
            _openVrModuleConfig = openVrModuleConfigRepository.Get();
            _steamVrConfigFile = GetSteamConfigFile();
            _openVrModuleConfigRepository = openVrModuleConfigRepository;
            _openVrSettingsSetRepository = openVrSettingsSetRepository;
            _steamLib = steamLib;
        }

        #region Public Methods
        public void ReplaceOpenVrConfig()
        {
            if (_openVrModuleConfig.IsOpenVrConfigReplacingEnabled)
            {
                Logger.Debug($"Called ReplaceOpenVrConfig: IsOpenVrConfigReplacingEnabled={_openVrModuleConfig.IsOpenVrConfigReplacingEnabled}, IsOpenVrConfigReplaced={_openVrModuleConfig.IsOpenVrConfigReplaced}");
                var overrideSettings = _openVrSettingsSetRepository.Get(_openVrModuleConfig.DefaultOpenVrSettingsName);

                IOpenVrSettingsSet originalSettings = null;
                try
                {
                    originalSettings = GetCurrentSettings();
                }
                catch (Exception e)
                {
                    Logger.Error( e, "Problem occured while getting current OpenVR settings.");
                }

                if (!_openVrModuleConfig.IsOpenVrConfigReplaced && originalSettings != null && overrideSettings != null)
                {
                    Logger.Debug("Swapping OpenVR settings with Custom Files");

                    _openVrSettingsSetRepository.Store(_openVrModuleConfig.OriginalOpenVrSettingsName, originalSettings);
                    ApplySettings(overrideSettings);

                    _openVrModuleConfig.IsOpenVrConfigReplaced = true;
                    _openVrModuleConfigRepository.Store(_openVrModuleConfig);
                }
            }
        }
        public void RestoreOpenVrConfig()
        {
            try
            {
                Logger.Debug($"Called RestoreOpenVrConfig: IsOpenVrConfigReplacingEnabled={_openVrModuleConfig.IsOpenVrConfigReplacingEnabled}");
                if (_openVrModuleConfig.IsOpenVrConfigReplacingEnabled)
                {
                    Logger.Debug("Restore OpenVR settings from Backuped Files");

                    var originalSettings = _openVrSettingsSetRepository.Get(_openVrModuleConfig.OriginalOpenVrSettingsName);
                    var orginalSettingsAvailible = originalSettings != null;
                    if (_openVrModuleConfig.IsOpenVrConfigReplaced && orginalSettingsAvailible)
                    {
                        ApplySettings(originalSettings, OpenVrConfigApplyBehavior.ReplaceFile);
                        _openVrModuleConfig.IsOpenVrConfigReplaced = false;
                        _openVrModuleConfigRepository.Store(_openVrModuleConfig);
                    }
                    else
                    {
                        Logger.Debug($"Did not restore OpenVR Settings: IsOpenVrConfigReplaced={_openVrModuleConfig.IsOpenVrConfigReplaced}, OrginalSettingsAvailible={orginalSettingsAvailible}");
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, "Exception Occured");
                throw;
            }
        }
        #endregion

        #region Private Methods
        private void ApplySettings(IOpenVrSettingsSet settings, OpenVrConfigApplyBehavior? behaviorOverride = null)
        {
            try
            {
                QuickLeap.AssertNotNull(settings);

                foreach (var settingFile in settings.SettingFiles)
                {
                    var defaultBehavior = _openVrModuleConfig.DefaultApplyBehaviors[settingFile.EntityDetails.FileType];
                    var applyBehavior = behaviorOverride ?? settingFile.EntityDetails.BehaviorOverride ?? defaultBehavior;

                    var baseDir = ResolveDirectory(settingFile.EntityDetails.BaseLocation);
                    var fullFilePath = Path.Combine(baseDir, settingFile.EntityDetails.RelativePath);

                    switch (applyBehavior)
                    {
                        case OpenVrConfigApplyBehavior.Override:
                            if (File.Exists(fullFilePath))
                            {
                                ApplyOverride(settingFile);
                                break;
                            }
                            ApplyReplaceFile(settingFile);
                            break;
                        case OpenVrConfigApplyBehavior.ReplaceFile:
                            ApplyReplaceFile(settingFile);
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid value of {nameof(applyBehavior)} = `{applyBehavior}`.");
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, "Exception Occured");
                throw;
            }
        }
        private void ApplyOverride(IOpenVrSettingFile settingFile)
        {
            switch (settingFile.EntityDetails.FileType)
            {
                case OpenVrConfigFileType.Json:
                    ApplyOverrideJson(settingFile);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid value of {nameof(settingFile.EntityDetails.FileType)} = `{settingFile.EntityDetails.FileType}`.");
            }
        }
        private void ApplyOverrideJson(IOpenVrSettingFile settingFile)
        {
            try
            {
                var baseDir = ResolveDirectory(settingFile.EntityDetails.BaseLocation);
                var fullFilePath = Path.Combine(baseDir, settingFile.EntityDetails.RelativePath);

                var baseFileContent = File.ReadAllText(fullFilePath);
                var overrideFileContent = settingFile.FileContent;

                var resultFileContent = JsonHelper.OverrideJsonString(baseFileContent, overrideFileContent);
                File.WriteAllText(fullFilePath, resultFileContent);
            }
            catch(Exception e)
            {
                Logger.Error(e, "Exception Occured");
                throw;
            }
        }
        private void ApplyReplaceFile(IOpenVrSettingFile settingFile)
        {
            var baseDir = ResolveDirectory(settingFile.EntityDetails.BaseLocation);
            var fullFilePath = Path.Combine(baseDir, settingFile.EntityDetails.RelativePath);

            File.WriteAllText(fullFilePath, settingFile.FileContent);
        }
        private string ResolveDirectory(OpenVrConfigLocation configLocation)
        {
            try
            {
                switch (configLocation)
                {
                    case OpenVrConfigLocation.OpenVrConfigDirectory:
                        return _steamVrConfigFile.config.First();
                    case OpenVrConfigLocation.SteamDirectory:
                        return _steamLib.SteamPath;
                    case OpenVrConfigLocation.OpenVrDirectory:
                        return _steamLib.SteamVrInstallDir;
                    default:
                        throw new InvalidOperationException($"Invalid value of {nameof(configLocation)} = `{configLocation}`.");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exception Occured");
                throw;
            }
        }
        private IOpenVrSettingsSet GetCurrentSettings()
        {
            var settingFiles = new List<OpenVrSettingFile>();
            foreach (var entityDetails in _openVrModuleConfig.ConfigFilesDetails)
            {
                try
                {
                    var baseLocation = entityDetails.BaseLocation;
                    var baseDir = ResolveDirectory(baseLocation);
                    var relativePath = entityDetails.RelativePath;
                    var fullFilePath = Path.Combine(baseDir, relativePath);

                    var fileContent = File.ReadAllText(fullFilePath);

                    settingFiles.Add(new OpenVrSettingFile(entityDetails, fileContent));
                }
                catch (Exception e)
                {
                    Logger.Error($"In GetCurrentSettings, while getting data for entity `{entityDetails.BaseLocation}`:`{entityDetails.RelativePath}` Exception occured ({e}).");
                    throw;
                }
            }

            return new OpenVrSettingsSet
                   {
                           SettingFiles = settingFiles,
                   };
        }
        private SteamVrConfigFile GetSteamConfigFile()
        {
            var resolvedConfigFilePath = Environment.ExpandEnvironmentVariables(_openVrModuleConfig.VrMonitorConfigFilePath);
            return JsonConvert.DeserializeObject<SteamVrConfigFile>(File.ReadAllText(resolvedConfigFilePath));
        }
        #endregion
    }
}
