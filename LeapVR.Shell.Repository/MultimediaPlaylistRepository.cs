#region Licence
/****************************************************************
 *  Filename: MultimediaPlaylistRepository.cs
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
using LeapVR.Shell.Repository.Database;
using LeapVR.Shell.Repository.Entities;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository {

    public class MultimediaPlaylistRepository : IMultimediaPlaylistRepository
    {
        public IMultimediaPlaylistData GetOrCreate(string identifier)
        {
            try
            {
                var retval =  Database.Database.QueryDatabase<MultimediaPlaylistDataDb, MultimediaPlaylistDataDb>(collection => collection.FindOne(x => x.Identifier.Equals(identifier)));
                if(retval == null)
                {
                    var newPlaylist = new MultimediaPlaylistDataDb(){Identifier = identifier, Tracks = new List<Uri>()};
                    if(newPlaylist.Store())retval= newPlaylist;
                    else
                    {
                        throw new RepositoryGetDbException($"Error on {nameof(GetOrCreate)} of {nameof(MultimediaPlaylistDataDb)}  with Identifier = {identifier}, Could not get an existing Playlist but also not create and store a new one");
                    }
                }

                return retval;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(GetOrCreate)} of {nameof(MultimediaPlaylistDataDb)}  with Identifier = {identifier}", exception);
            }
        }

        public bool Store(IMultimediaPlaylistData playlistData)
        {
            try
            {
                if (String.IsNullOrEmpty(playlistData.Identifier))
                {
                    throw new NotSupportedException("Identifier for IMultimediaPlaylistData must be set to be able to save");
                }
                var storeObj = EntityConverter.Convert(playlistData);
                //Dont allow store of two playlists with same identifier
                if(storeObj.Id.Equals(Guid.Empty))
                {
                    if(Database.Database.QueryDatabase<bool, MultimediaPlaylistDataDb>(collection => collection.Exists(x => x.Identifier.Equals(storeObj.Identifier))))
                    {
                        throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(IMultimediaPlaylistData)}  with Identifier = {playlistData.Identifier}, This identifier exists already!");
                    }
                }
                storeObj.Store();
                return true;
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(IMultimediaPlaylistData)}  with Identifier = {playlistData.Identifier}", exception);
            }
        }

        public bool Delete(string identifier)
        {
            try
            {
                var entity = Database.Database.QueryDatabase<MultimediaPlaylistDataDb, MultimediaPlaylistDataDb>(collection => collection.FindOne(x => x.Identifier == identifier));
                if (entity == null) return true;
                return entity.Delete<MultimediaPlaylistDataDb>();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryDeleteDbException($"Error on {nameof(Delete)} of {nameof(MultimediaPlaylistDataDb)}  with Identifier = {identifier}", exception);
            }
        }
    }
}