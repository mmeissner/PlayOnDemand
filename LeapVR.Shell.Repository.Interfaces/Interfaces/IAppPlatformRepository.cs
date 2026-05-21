#region Licence
/****************************************************************
 *  Filename: IAppPlatformRepository.cs
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
    /// Repository for <see cref="T:LeapVR.VBox.DataModel.Interfaces.App.IAppPlatformData" />. Marked as <see cref="IRepository"/>.
    /// </summary>
    public interface IAppPlatformRepository : IRepository
    {
        IAppPlatformData Get(Guid applicationGuid);
        IEnumerable<Guid> GetAllEnabledApps();
        bool TryGetEnabledApp(Guid applicationGuid, out IAppPlatformData platformData);
        IEnumerable<IAppPlatformData> GetAll();
        bool IsAppEnabled(Guid applicationGuid);
        bool SetAppEnabled(Guid applicationGuid, bool enabledValue, out Guid platformId);
        bool Update(Guid applicationGuid, IEnumerable<IProcessExecutionLogic> processExecutionLogic);
        bool Store(IAppPlatformData appPlatformData);
        bool Delete(Guid applicationGuid);
    }
}
