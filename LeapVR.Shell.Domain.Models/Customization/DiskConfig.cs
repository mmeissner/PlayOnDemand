#region Licence
/****************************************************************
 *  Filename: DiskConfig.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Domain.Models.Customization
{
    
    public class DiskConfig : ConfigObject
    {
        public const string DefaultGamesSubDir = "Games\\LeapPlay\\";
        private static readonly IDictionary<ContentType, string> _defaultContentRelativeDirs = new Dictionary<ContentType, string>
                                                                                               {
                                                                                                   { ContentType.GameFiles, @"app" },
                                                                                                   { ContentType.HardwareTemplates, @"hardware_template" },
                                                                                                   { ContentType.MediaFiles, @"media" },
                                                                                                   { ContentType.PufFiles, @"puf" },
                                                                                                   { ContentType.Metadata, @"metadata" },
                                                                                               };
        public string[] SystemDrives { get; set; } 
        public string StorageBaseDir { get; set; }
        public double ReservedDiskSpaceRatio { get; set; } = 0.05;
        public IDictionary<ContentType, string> ContentRelativeDirs { get; set; } = _defaultContentRelativeDirs.ToDictionary(kv => kv.Key, kv => kv.Value);

        public override void Initialize()
        {
            // If ContentRelativeDirs is missing some of required keys we add them with default value.
            var missingKeys = _defaultContentRelativeDirs
                .Except(ContentRelativeDirs)
                .Select(kv => kv.Key)
                .ToArray();

            foreach (var missingKey in missingKeys)
            {
                ContentRelativeDirs.Add(missingKey, _defaultContentRelativeDirs[missingKey]);
            }
        }
    }
}
