#region Licence
/****************************************************************
 *  Filename: IMultimediaPlaylistData.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces
{
    
    public interface IMultimediaPlaylistData
    {
        string Identifier { get; set; }
        List<Uri> Tracks { get; set; }
    }
}
