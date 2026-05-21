#region Licence
/****************************************************************
 *  Filename: BaseAppInstallationHeaderSerializer.cs
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
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Execution;
using ProtoBuf.Meta;
using DiskEntityDto = LeapVR.Content.Shared.Container.DiskEntityDto;

namespace LeapVR.Content.Shared
{
    public abstract class BaseAppInstallationHeaderSerializer
    {
        private static volatile bool _initialized;
        private static readonly object Locker = new object();

        public BaseAppInstallationHeaderSerializer()
        {
            if(_initialized)return;
            lock (Locker)
            {
                if(_initialized)return;
                ProtobufInitialization();
                _initialized = true;
            }
        }

        private static void ProtobufInitialization()
        {
            RuntimeTypeModel.Default[typeof(IAppInstallationHeader)].AddSubType(20, typeof(AppInstallationHeaderDto));
            RuntimeTypeModel.Default[typeof(IPackageData)].AddSubType(30, typeof(PackageDataDto));
            RuntimeTypeModel.Default[typeof(IAppDisplayDataDto)].AddSubType(70, typeof(AppDisplayDataDto));
        }
    }
}