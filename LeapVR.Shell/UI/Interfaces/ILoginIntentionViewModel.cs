#region Licence
/****************************************************************
 *  Filename: ILoginIntentionViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-9-5
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
using LeapVR.Shared.Lib;
using LeapVR.Shell.Domain.Models.Authentication;

namespace LeapVR.Shell.UI.Interfaces
{
    /// <summary>
    /// Representing a view model that holds the information of a <see cref="ILoginIntention"/>.
    /// </summary>
    public interface ILoginIntentionViewModel : ILoginPageViewModel,IDisposable
    {
         Guid LoginIntentionId { get;}
         bool IsCancelled { get;  }
         bool IsExpired { get;  }
         bool IsTimeout { get;}
    }
}
