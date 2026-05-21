#region Licence
/****************************************************************
 *  Filename: IConfigObject.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-1-17
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

namespace LeapVR.Shell.Domain.Models.Customization
{
    /// <summary>
    /// Represents an object that contains a specific collection of configurations.
    /// </summary>
    
    public interface IConfigObject
    {
        void Initialize();
    }
}
