#region Licence
/****************************************************************
 *  Filename: ITabItemScreen.cs
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
using System.Windows.Controls;
using Caliburn.Micro;

namespace LeapVR.Shell.UI.Interfaces
{
    /// <summary>
    /// Representing a view model object contained by the <see cref="Conductor{T}"/> of <see cref="TabControl"/> with its icon from <see cref="IHaveIcon"/> and its display order from <see cref="IHaveDisplayOrder"/>
    /// </summary>
    public interface ITabItemScreen : IScreen, IHaveIcon, IHaveDisplayOrder, IComparable
    {

    }
}
