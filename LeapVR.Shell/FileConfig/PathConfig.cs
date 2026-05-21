#region Licence
/****************************************************************
 *  Filename: PathConfig.cs
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
using System.Reflection;

namespace LeapVR.Shell.FileConfig
{
    
    public class PathConfig
    {
        public string ImagesFolder { get; set; } = "Images";
        public string CategoryIconFolder { get; set; } = System.IO.Path.Combine("Images", "Categories");
    }
}
