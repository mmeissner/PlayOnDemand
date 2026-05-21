#region Licence
/****************************************************************
 *  Filename: IMultimediaSettingsRepository.cs
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
using LeapVR.Shell.Modules.Interfaces.Multimedia;

namespace LeapVR.Shell.Repository.Interfaces.Interfaces {
    public interface IMultimediaSettingsRepository {
        IMultimediaSettings Get(string identifier);
    }
}