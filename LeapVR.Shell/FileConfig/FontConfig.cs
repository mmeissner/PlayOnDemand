#region Licence
/****************************************************************
 *  Filename: FontConfig.cs
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
    
    public class FontConfig
    {
        public double H1 { get; set; } = 72d;
        public double H2 { get; set; } = 60d;
        public double H3 { get; set; } = 48d;
        public double H4 { get; set; } = 36d;
        public double H5 { get; set; } = 28d;
        public double H6 { get; set; } = 20d;
        public double Paragraph { get; set; } = 12d;

    }
}
