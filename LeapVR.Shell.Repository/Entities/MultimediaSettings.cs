#region Licence
/****************************************************************
 *  Filename: MultimediaSettings.cs
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
using LeapVR.Shell.Domain.Models.Multimedia;

namespace LeapVR.Shell.Repository.Entities {

    internal class MultimediaSettings : IMultimediaSettings
    {

        private readonly MultimediaSettingsRepository _repository;
        private readonly MultimediaSettingsDataDb _settings;
        internal MultimediaSettings(MultimediaSettingsRepository repository,string identifier): this(repository,new MultimediaSettingsDataDb(){Identifier = identifier}){}
        internal MultimediaSettings(MultimediaSettingsRepository repository, MultimediaSettingsDataDb multimediaSettingsData)
        {
            _repository = repository;
            _settings = multimediaSettingsData;
        }

        public void Store()
        {
            _repository.Store(_settings);
        }
        public string Identifier => _settings.Identifier;
        public bool AutoStart
        {
            get => _settings.AutoStart;
            set => _settings.AutoStart = value;
        }
        public bool Repeat
        {
            get => _settings.Repeat;
            set => _settings.Repeat = value;
        }
        public double Volume
        {
            get => _settings.Volume;
            set => _settings.Volume = value;
        }
    }
}