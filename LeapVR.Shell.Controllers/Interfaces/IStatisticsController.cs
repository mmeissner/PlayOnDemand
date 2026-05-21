#region Licence
/****************************************************************
 *  Filename: IStatisticsController.cs
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

using System;
using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Statistics;

namespace LeapVR.Shell.Controllers.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Handles acquisition, aggregation, and calculation of statistics about user actions.
    /// </summary>
    public interface IStatisticsController : IController, IExecutionMessageReceiver
    {
        IEnumerable<IAppStatistic> GetAllAppStatistics();
        IAppStatistic GetApplicationWithMostExecutedTimes();
        IAppStatistic GetApplicationWithLongestTotalSession();
        IAppStatistic GetApplicationWithLongestSession();
        TimeSpan GetTotalRuntimeForAllApplications();

        void ClearStatisticsDataForSpecificApp(Guid applicationGuid);
        void ClearStatisticsDataForAllApp();

    }
}
