#region Licence
/****************************************************************
 *  Filename: IConfigFileRepository.cs
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
namespace LeapVR.VBox.Modules.Interfaces.Repositories
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
