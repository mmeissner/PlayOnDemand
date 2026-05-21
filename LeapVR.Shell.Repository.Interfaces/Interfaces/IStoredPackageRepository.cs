#region Licence
/****************************************************************
 *  Filename: IStoredPackageRepository.cs
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

using System;
using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Repository for <see cref="T:LeapVR.Shell.Repository.Interfaces.Interfaces.IStoredPackageRepository" />. Marked as <see cref="IRepository"/>.
    /// </summary>
    public interface IStoredPackageRepository : IRepository
    {
        IStoredPackageData Get(Guid packageGuid);
        IEnumerable<IStoredPackageData> GetAll();
        IEnumerable<IStoredPackageData> GetAll(ContentType contentType);
        IEnumerable<IStoredPackageData> GetAll(Guid applicationGuid);

        bool Store(IPackageData packageData, PackageState packageState, out IStoredPackageData storedPackage);
        bool Store(IStoredPackageData packageData);
        bool Delete(Guid packageGuid);
    }
}
