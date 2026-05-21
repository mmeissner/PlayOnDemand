#region Licence
/****************************************************************
 *  Filename: IApplicationNavigatorViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-19
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
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.UI.Interfaces
{
    public interface IApplicationNavigatorViewModel<T> : IContentViewModel, IDisposable
    {
        IAppCategory Category { get; }
        void ExecuteSelectedItem();
        IObservableCollection<T> Items { get; }
    }
}
