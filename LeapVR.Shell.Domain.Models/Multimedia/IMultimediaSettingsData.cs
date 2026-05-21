#region Licence
/****************************************************************
 *  Filename: IMultimediaSettingsData.cs
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

namespace LeapVR.Shell.Domain.Models.Multimedia
{
    
    public interface IMultimediaSettingsData
    {
        string Identifier { get; }
        bool AutoStart { get; set; }
        bool Repeat { get; set; }
        double Volume { get; set; }
    }
}