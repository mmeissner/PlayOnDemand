#region Licence
/****************************************************************
 *  Filename: AppInstallationHeaderSerializer.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2018-1-19
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.IO;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shell.Domain.Models.Container;
using NLog;
using Pod.Data.Infrastructure;
using ProtoBuf;
using IAppInstallationHeader = LeapVR.Shell.Domain.Models.Container.IAppInstallationHeader;

namespace LeapVR.Content.Shared.Container
{
    public class AppInstallationHeaderSerializer : BaseAppInstallationHeaderSerializer, IAppInstallationHeaderSerializer
    {        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public IZipContainerHeader LoadFromFile(string filePath)
        {
            try
            {
                Logger.Debug($"Trying to Load {nameof(IZipContainerHeader)} from file={filePath}");
                using (var fs = File.OpenRead(filePath))
                {
                    return LoadFromStream(fs);
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public IZipContainerHeader LoadFromStream(Stream source)
        {
            return Serializer.Deserialize<AppInstallationHeaderDto>(source);
        }

        public bool SaveToFile(string filePath, IZipContainerHeader header)
        {
            try
            {
                if(!(header is IAppInstallationHeader appInstallation))
                {
                    Logger.Error($"Could not convert {nameof(IZipContainerHeader)} to {nameof(IAppInstallationHeader)}! {header.LogJson()}");
                    return false;
                }
                if (File.Exists(filePath)) File.Delete(filePath);
                using (var fs = File.Create(filePath))
                {
                    Serializer.Serialize(fs, new AppInstallationHeaderDto(appInstallation));
                }
                return true;
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public bool SaveToStream(Stream destination, IZipContainerHeader header)
        {
            try
            {
                if(!(header is IAppInstallationHeader appInstallation))
                {
                    Logger.Error($"Could not convert {nameof(IZipContainerHeader)} to {nameof(IAppInstallationHeader)}! {header.LogJson()}");
                    return false;
                }
                Serializer.Serialize(destination, new AppInstallationHeaderDto(appInstallation));
                return true;
            }
            catch(Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }
    }
}
