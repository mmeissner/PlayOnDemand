#region Licence
/****************************************************************
 *  Filename: IAppCategory.cs
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Domain.Models.App
{
    public interface IAppCategory : INotifyPropertyChanged
    {
        string DisplayName { get; }
        ImageSource Icon { get; }
        string Identifier { get; }
        void Handle(IUILanguageChangedEvent message);
    }
}
