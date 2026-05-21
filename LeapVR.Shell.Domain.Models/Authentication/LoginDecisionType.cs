#region Licence
/****************************************************************
 *  Filename: LoginDecisionType.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapVR.Shell.Domain.Models.Authentication
{
    /// <summary>
    /// Type of possible decision in response to <see cref="ILoginIntention"/>.
    /// </summary>
    public enum LoginDecisionType
    {
        Unknown = 0,

        /// <summary>
        /// User's intention to start new session has been confirmed.
        /// </summary>
        Confirm = 1,

        /// <summary>
        /// User denied the intention to start new session.
        /// </summary>
        Cancel = 2,
    }
}
