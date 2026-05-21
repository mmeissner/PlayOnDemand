#region Licence
/****************************************************************
 *  Filename: ISessionRate.cs
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

using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Domain.Models.Billing
{
    /// <summary>
    /// Represents set of static data used for possible billing in <see cref="IUISession"/>.
    /// Defines Billing type of session.
    /// </summary>
    public interface ISessionRate
    {
        ISessionRate Clone();
    }
}
