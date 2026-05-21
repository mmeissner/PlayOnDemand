#region Licence
/****************************************************************
 *  Filename: IMultimediaPlaylistRepository.cs
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

namespace LeapVR.Shell.Repository.Interfaces.Interfaces
{
    public interface IMultimediaPlaylistRepository 
    {
        IMultimediaPlaylistData GetOrCreate(string identifier);
        bool Store(IMultimediaPlaylistData playlistData);
        bool Delete(string identifier);
    }
}
