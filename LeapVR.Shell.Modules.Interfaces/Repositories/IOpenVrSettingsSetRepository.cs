#region Licence
/****************************************************************
 *  Filename: IOpenVrSettingsSetRepository.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.Modules.Interfaces.Repositories
{
    /// <inheritdoc />
    /// <summary>
    /// Special repository for storing and retrieving <see cref="T:LeapVR.Shell.Modules.Interfaces.Vr.IOpenVrSettingsSet" />. Marked as <see cref="LeapVR.Shell.Repository.Repositories.IRepository"/>.
    /// </summary>
    public interface IOpenVrSettingsSetRepository 
    {
        /// <summary>
        /// Gets <see cref="IOpenVrSettingsSet"/> object stored under specified <see cref="settingsName"/>.
        /// </summary>
        /// <param name="settingsName">Name of settings set</param>
        /// <returns>Releated <see cref="IOpenVrSettingsSet"/> if successful; Null if failure</returns>
        IOpenVrSettingsSet Get(string settingsName);

        /// <summary>
        /// Stores <see cref="IOpenVrSettingsSet"/> object under specified <see cref="settingsName"/>.
        /// </summary>
        /// <param name="settingsName">Name of settings set</param>
        /// <param name="obj"><see cref="IOpenVrSettingsSet"/> object to store</param>
        /// <returns>Boolean indicating success or failure</returns>
        bool Store(string settingsName, IOpenVrSettingsSet obj);

        /// <summary>
        /// Deletes <see cref="IOpenVrSettingsSet"/> stored under specified <see cref="settingsName"/>.
        /// </summary>
        /// <param name="settingsName">Name of settings set</param>
        /// <returns>Boolean indicating success or failure</returns>
        bool Delete(string settingsName);
    }
}
