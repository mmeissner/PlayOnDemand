#region Licence
/****************************************************************
 *  Filename: IPlaylistModule.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeapVR.Shell.Modules.Interfaces.Multimedia
{
    public interface IPlaylistModule {
        IMultimediaPlaylist GetPlaylist(string identifier);
        void DeletePlaylist(IMultimediaPlaylist playlist);
    }
}
