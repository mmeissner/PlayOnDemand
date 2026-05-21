#region Licence
/****************************************************************
 *  Filename: MultimediaPlaylistDataDb.cs
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
using LeapVR.Shell.Repository.Interfaces.Entities;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository.Entities
{
    
    public class MultimediaPlaylistDataDb : IEntity,IMultimediaPlaylistData
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public List<Uri> Tracks { get; set; }

        public MultimediaPlaylistDataDb(){}
        public MultimediaPlaylistDataDb(IMultimediaPlaylistData playlistData)
        {
            Identifier = playlistData.Identifier;
            Tracks = playlistData.Tracks;
        }
    }
}
