#region Licence
/****************************************************************
 *  Filename: Enums.cs
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
using System.ComponentModel;
using LeapVR.Content.Creator.Language;
using LeapVR.Shared.Lib.Wpf;

namespace LeapVR.Content.Creator.Logic
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PlatformType
    {
        None = 0,
        [LocalizedDescription("Enum_Platform_LeapVR", typeof(Resources))]
        LeapVr = 1,
        [LocalizedDescription("Enum_Platform_Steam", typeof(Resources))]
        Steam = 2,
    }
}
