#region Licence
/****************************************************************
 *  Filename: IAppStatistic.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models.Statistics
{
    public interface IAppStatistic
    {
        IAppPlatformInfo DisplayInfo { get; }
        uint TimesPlayed { get; }
        TimeSpan LongestSession { get; }
        TimeSpan TotalSessions { get; }
        DateTime LastTimePlayed { get; }
    }
}
