#region Licence
/****************************************************************
 *  Filename: OpenVrSettingsSetRepository.cs
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
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces.Repositories;
using LeapVR.Shell.Modules.Interfaces.Vr;
using NLog;

namespace LeapVR.Shell.Modules.Vr
{
    public class OpenVrSettingsSetRepository : IOpenVrSettingsSetRepository
    {
        #region Fields & Properties

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _fileStoringDirectory;

        private readonly IDictionary<OpenVrConfigLocation, string> _locationsRelativePaths = new Dictionary<OpenVrConfigLocation, string>
        {
            {OpenVrConfigLocation.OpenVrConfigDirectory, @"OpenVrConfigDirectory"},
            {OpenVrConfigLocation.OpenVrDirectory, @"OpenVrDirectory"},
            {OpenVrConfigLocation.SteamDirectory, @"SteamDirectory"},
        };

        private readonly IEnumerable<IOpenVrSettingEntityDetails> _entitiesDetails;

        #endregion Fields & Properties

        #region Contructors

        public OpenVrSettingsSetRepository(IGlobalConfiguration globalConfiguration, IConfigFileRepository<OpenVrModuleConfig> openVrModuleConfigRepo)
        {
            QuickLeap.AssertNotNullEx(openVrModuleConfigRepo, globalConfiguration.PersistentDirectory);
            _fileStoringDirectory = Path.Combine(globalConfiguration.PersistentDirectory, @"FileRepositories\OpenVrSettingsSet");

            _entitiesDetails = openVrModuleConfigRepo.Get().ConfigFilesDetails;
        }

        #endregion Contructors

        #region Methods

        public IOpenVrSettingsSet Get(string settingsName)
        {
            try
            {
                QuickLeap.AssertNotNull(settingsName);

                var baseDir = Path.Combine(_fileStoringDirectory, settingsName);
                if (!Directory.Exists(baseDir))
                {
                    return null;
                }

                var settingFiles = new List<IOpenVrSettingFile>();
                foreach (var kv in _locationsRelativePaths)
                {
                    var baseLocation = kv.Key;
                    var relativeLocationPath = kv.Value;
                    var locationFullDir = Path.Combine(baseDir, relativeLocationPath);
                    if (!Directory.Exists(locationFullDir))
                    {
                        continue;
                    }

                    var fullFilePaths = Directory.GetFiles(locationFullDir, "*", SearchOption.AllDirectories);

                    foreach (var fullFilePath in fullFilePaths)
                    {
                        var relativeFilePath = QuickLeap.GetRelativePath(fullFilePath, locationFullDir);

                        var entityDetails = _entitiesDetails.SingleOrDefault(q => q.BaseLocation == baseLocation
                                        && string.Equals(q.RelativePath, relativeFilePath, StringComparison.InvariantCultureIgnoreCase));

                        if (entityDetails == null)
                        {
                            // TODO [RM]: just ignore the file that has no releated entry in _entitiesDetails; change behavior later?
                            // TODO [RM]: log warning?
                            continue;
                        }

                        var fileRelativePath = QuickLeap.GetRelativePath(fullFilePath, locationFullDir);
                        var fileContent = File.ReadAllText(fullFilePath);

                        var settingFile = new OpenVrSettingFileDto
                        {
                            EntityDetails = entityDetails,
                            FileContent = fileContent,
                        };
                        settingFiles.Add(settingFile);
                    }
                }

                return new OpenVrSettingsSetDto
                {
                    SettingFiles = settingFiles,
                };
            }
            catch (System.Exception e)
            {
                Logger.Error(e, $"Exception occured while getting {nameof(IOpenVrSettingsSet)} data from repository ({nameof(settingsName)}: `{settingsName}`).");
                return null;
            }

        }

        public bool Store(string settingsName, IOpenVrSettingsSet obj)
        {
            try
            {
                QuickLeap.AssertNotNull(settingsName, obj);

                var baseDir = Path.Combine(_fileStoringDirectory, settingsName);
                if (Directory.Exists(baseDir))
                {
                    Directory.Delete(baseDir, true);
                }

                var settingFiles = obj.SettingFiles;
                foreach (var settingFile in settingFiles)
                {
                    var baseLocation = settingFile.EntityDetails.BaseLocation;
                    var relativeLocationPath = _locationsRelativePaths[baseLocation];
                    var locationFullDir = Path.Combine(baseDir, relativeLocationPath);

                    var fileContent = settingFile.FileContent;

                    var fileRelativePath = settingFile.EntityDetails.RelativePath;
                    var fullFilePath = Path.Combine(locationFullDir, fileRelativePath);
                    var fullFileDir = Path.GetDirectoryName(fullFilePath);

                    Directory.CreateDirectory(fullFileDir);
                    File.WriteAllText(fullFilePath, fileContent);
                }

                return true;
            }
            catch (System.Exception e)
            {
                Logger.Error(e, $"Exception occured while storing {nameof(IOpenVrSettingsSet)} data to repository ({nameof(settingsName)}: `{settingsName}`, {nameof(obj)} == null: {obj == null}).");
                return false;
            }
        }

        public bool Delete(string settingsName)
        {
            try
            {
                QuickLeap.AssertNotNull(settingsName);

                var fullDir = Path.Combine(_fileStoringDirectory, settingsName);
                if (Directory.Exists(fullDir))
                {
                    Directory.Delete(fullDir, true);
                }

                return true;
            }
            catch (System.Exception e)
            {
                Logger.Error(e, $"Exception occured while deleting {nameof(IOpenVrSettingsSet)} data from repository ({nameof(settingsName)}: `{settingsName}`).");
                return false;
            }
        }

        #endregion Methods
    }
}
