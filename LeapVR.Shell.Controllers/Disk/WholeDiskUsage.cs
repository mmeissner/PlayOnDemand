#region Licence
/****************************************************************
 *  Filename: WholeDiskUsage.cs
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

using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;

namespace LeapVR.Shell.Controllers.Disk
{
    public class WholeDiskUsage : IWholeDiskUsage // immutable object
    {
        #region Properties & Fields

        public long TotalDiskSpace { get; internal set; }
        public IDictionary<ContentType, long> ContentUsedDiskSpace { get; internal set; }
        public long SystemUsedDiskUsage { get; internal set; }

        #endregion Properties & Fields

        #region Constructors

        internal WholeDiskUsage()
        {
            //
        }

        #endregion Constructors

        #region Methods

        //

        #endregion Methods
    }
}
