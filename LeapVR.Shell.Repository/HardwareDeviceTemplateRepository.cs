#region Licence
/****************************************************************
 *  Filename: HardwareDeviceTemplateRepository.cs
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
using System.IO;
using LeapVR.Shell.Domain.Models.Hardware;
using LeapVR.Shell.Repository.Entities;

namespace LeapVR.Shell.Repository
{
    public class HardwareDeviceTemplateRepository : IHardwareDeviceTemplateRepository
    {
        public static readonly string RootDirectory = Path.Combine(Environment.CurrentDirectory, "HardwareTemplates");

        public HardwareDeviceTemplateRepository()
        {
            throw new NotImplementedException();
            //if (!Directory.Exists(RootDirectory))
            //{
            //    Directory.CreateDirectory(RootDirectory);
            //    CreateTemplates();
            //}
        }

        public IEnumerable<IHardwareDeviceTemplateData> GetAll()
        {
            throw new NotImplementedException();
            //List<IHardwareDeviceTemplateData> retval = new List<IHardwareDeviceTemplateData>();
            //DirectoryInfo dirInfo = new DirectoryInfo(RootDirectory);
            //var templateFiles = dirInfo.EnumerateFiles("*.json", SearchOption.AllDirectories);

            //foreach (FileInfo file in templateFiles)
            //{
            //    //Dont read files that are too big
            //    if(file.Length > 100000)continue;

            //    //Dont read files with wrong names
            //    Guid hardwareGuid;
            //    if(!Guid.TryParse(Path.GetFileNameWithoutExtension(file.FullName),out hardwareGuid))continue;

            //    var deserializedObj = DeserializeFile(file);
            //    if (deserializedObj != null) retval.Add(deserializedObj);
            //}
            //return retval;
        }

        public IHardwareDeviceTemplateData Get(Guid hardwareTemplateId)
        {
            throw new NotImplementedException();
            //var fileInfo = new FileInfo(Path.Combine(RootDirectory, $"{hardwareTemplateId}.json"));
            //if (!fileInfo.Exists) return null;
            //return DeserializeFile(fileInfo);
        }

        public void Store(IHardwareDeviceTemplateData dataTemplate)
        {
            throw new NotImplementedException();
            //var storeObj = new HardwareDeviceDataTemplateDb();
            //storeObj.InjectFrom(dataTemplate);
            //File.WriteAllText(Path.Combine(RootDirectory, $"{dataTemplate.HardwareDeviceTemplateGuid}.json"), JsonConvert.SerializeObject(storeObj, Formatting.Indented));
        }

        public void Delete(IHardwareDeviceTemplateData dataTemplate)
        {
            throw new NotImplementedException();
            //var file = new FileInfo(Path.Combine(RootDirectory, $"{dataTemplate.HardwareDeviceTemplateGuid}.json"));
            //if(file.Exists)file.Delete();
        }
        private void CreateTemplates()
        {
            throw new NotImplementedException();
            //Store(new HardwareDeviceDataTemplateDb()
            //{
            //    HardwareDeviceTemplateGuid = Guid.Parse("eb9927b0-f913-427b-bd8c-45ca2cedd14f"),
            //    DisplayName = "XBOX One Wireless Gamepad",
            //    DefaultState = DeviceState.Enabled,
            //    GenericDeviceId = @"USB\VID_045E&amp;amp;PID_02E6\*",
            //    DisableDelayMs = 500,
            //    EnableDelayMs = 500
            //});
            //Store(new HardwareDeviceDataTemplateDb()
            //{
            //    HardwareDeviceTemplateGuid = Guid.Parse("2a8335ab-39fa-41f8-9ecb-badcc03d2ea3"),
            //    DisplayName = "XBOX 360 Wireless Gamepad",
            //    DefaultState = DeviceState.Enabled,
            //    GenericDeviceId = @"USB\VID_045E&amp;amp;PID_0719\*",
            //    DisableDelayMs = 500,
            //    EnableDelayMs = 500
            //});
            //Store(new HardwareDeviceDataTemplateDb()
            //{
            //    HardwareDeviceTemplateGuid = Guid.Parse("65c199e0-3637-4127-8d96-251ad5d954bb"),
            //    DisplayName = "Thrustmaster HOTAS Throttle",
            //    DefaultState = DeviceState.Enabled,
            //    GenericDeviceId = @"HID\VID_044F&amp;amp;PID_0404\*",
            //    DisableDelayMs = 2000,
            //    EnableDelayMs = 5000
            //});
            //Store(new HardwareDeviceDataTemplateDb()
            //{
            //    HardwareDeviceTemplateGuid = Guid.Parse("5f52ef9f-0263-4425-8991-39f5660ec613"),
            //    DisplayName = "Thrustmaster HOTAS Joystick",
            //    DefaultState = DeviceState.Enabled,
            //    GenericDeviceId = @"HID\VID_044F&amp;amp;PID_0402\*",
            //    DisableDelayMs = 500,
            //    EnableDelayMs = 1500
            //});
        }

        private HardwareDeviceDataTemplateDb DeserializeFile(FileInfo fileinfo)
        {
            throw new NotImplementedException();
            //using (var stream = fileinfo.OpenText())
            //{
            //    var content = stream.ReadToEnd();
            //    var deserializedObj = JsonConvert.DeserializeObject<HardwareDeviceDataTemplateDb>(content);
            //    return deserializedObj;
            //}
        }
    }
}