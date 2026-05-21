#region Licence
/****************************************************************
 *  Filename: MultimediaSettingsRepository.cs
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
using LeapVR.Shell.Domain.Models.Multimedia;
using LeapVR.Shell.Repository.Database;
using LeapVR.Shell.Repository.Entities;
using LeapVR.Shell.Repository.Exception;
using LeapVR.Shell.Repository.Interfaces.Interfaces;

namespace LeapVR.Shell.Repository {
    public class MultimediaSettingsRepository : IMultimediaSettingsRepository
    {
        public IMultimediaSettings Get(string identifier)
        {
            try
            {
                var result = Database.Database.QueryDatabase<MultimediaSettingsDataDb, MultimediaSettingsDataDb>(collection => collection.FindOne(x => x.Identifier == identifier));
                return result != null ? new MultimediaSettings(this,result) : new MultimediaSettings(this, identifier);
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryGetDbException($"Error on {nameof(Get)} of {nameof(MultimediaSettingsDataDb)}  with identifier = {identifier}", exception);
            }
        }

        internal void Store(MultimediaSettingsDataDb settings)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(settings.Identifier))
                {
                    throw new NotSupportedException("ApplicationGuid for PlatformInfo must be set to save");
                }
                //Save only as MultimediaSettingsDataDb even its derived
                settings.Store();
            }
            catch (LiteDB.LiteException exception)
            {
                throw new RepositoryStoreDbException($"Error on {nameof(Store)} of {nameof(MultimediaSettingsDataDb)}  with Identifier = {settings.Identifier}", exception);
            }
        }
    }
}