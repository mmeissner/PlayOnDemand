#region Licence
/****************************************************************
 *  Filename: IAppDisplayRepository.cs
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
using LeapVR.Shell.Domain.Models.Container;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Repository for <see cref="T:LeapVR.VBox.DataModel.Interfaces.App.IAppDisplayData" />. Marked as <see cref="IRepository"/>.
    /// </summary>
    public interface IAppDisplayRepository : IRepository
    {
        IAppDisplayData Get(Guid applicationGuid);
        IEnumerable<IAppDisplayData> GetAll();
        bool Store(IAppDisplayData appDisplayData);
        bool Delete(Guid applicationGuid);
    }
}