#region Licence
/****************************************************************
 *  Filename: IConfigFileRepository.cs
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

namespace LeapVR.Shell.Domain.Models.Customization
{
    /// <summary>
    /// Repository for storing configuration objects for different application components.
    /// </summary>
    /// <typeparam name="T">Type of configuration object (should be serializable).</typeparam>
    public interface IConfigFileRepository<T> where T : new()
    {
        /// <summary>
        /// Gets stored config object.
        /// </summary>
        /// <returns>Stored config object</returns>
        T Get();
        T Get(bool allowCached);
        /// <summary>
        /// Overwrites currently stored config object with <see cref="objToStore"/>.
        /// </summary>
        /// <param name="objToStore">New config object to store</param>
        /// <returns>Boolean indicating success or failure.</returns>
        bool Store(T objToStore);
    }
}
