#region Licence
/****************************************************************
 *  Filename: IAppInstallationRepository.cs
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
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Repository for <see cref="T:LeapVR.VBox.DataModel.Interfaces.App.IAppInstallationData" />. Marked as <see cref="IRepository"/>.
    /// </summary>
    public interface IAppInstallationRepository:IRepository
    {
        IEnumerable<IAppInstallationData> GetAll();
        IEnumerable<IAppInstallationData> GetAllByPlatformId(Guid platformId);
        IAppInstallationData Get(Guid applicationGuid);  
        bool Store(IAppInstallationData appInstallationData);
        bool Delete(Guid applicationGuid);
    }
}