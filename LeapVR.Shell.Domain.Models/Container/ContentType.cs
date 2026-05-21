#region Licence
/****************************************************************
 *  Filename: ContentType.cs
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

using System.Reflection;

namespace LeapVR.Shell.Domain.Models.Container
{
    public enum ContentType : uint
    {
        Unset = 0,
        GameFiles = 10,
        PufFiles = 20,
        PufBatch = 21,
        PufRegistry = 22,
        MediaFiles = 30,
        HardwareTemplates = 40,
        Redistributables = 50,
        Metadata = 60,
    }
}
