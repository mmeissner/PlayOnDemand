#region Licence
/****************************************************************
 *  Filename: ContentCreatorConfig.cs
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
using System.Collections.Generic;
using System.Reflection;
using LeapVR.Shell.Domain.Models.Customization;

namespace LeapVR.Content.Creator
{
    public class ContentCreatorConfig : ConfigObject
    {
        public string LastApplicationTitle { get; set; } = string.Empty;
        public string LastCategory { get; set; } = string.Empty;
        public string LastImageDirectory { get; set; } = string.Empty;
        public string LastBaseDirectory { get; set; } = string.Empty;
        public string LastOutputPackageDirectory { get; set; } = string.Empty;
        public long MaximunAppImageKilobytes { get; set; } = 1024;
        public char DefaultInvalidFileNameCharReplacement { get; set; } = '_';
        public int[] AvailablePlatforms { get; set; } = { 1 };
        public string Language { get; set; } = "english"; // available: `english`, `schinese`

        public bool IsPathManualEditEnabled { get; set; } = false;

        public Dictionary<string, string> LanguageCountrycodeMap = new Dictionary<string, string>
        {
            { "english", "US" },
            { "schinese", "CN" },
        };
        public string[] CountrycodesToUse = { "CN", "US", "EE" };
        public bool UseSelectedLanguageCountrycodeFirst = true;

        public bool IsParsingLauncherConfigEnabled { get; set; } = true;

    }
}
