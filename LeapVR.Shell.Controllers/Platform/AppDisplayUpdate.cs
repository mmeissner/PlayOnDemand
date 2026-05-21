#region Licence
/****************************************************************
 *  Filename: AppDisplayUpdate.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Controllers.Platform
{
    public class AppDisplayUpdate : IAppDisplayUpdate, IAppDisplayUpdateInfo
    {
        private readonly AppDisplayInfo _unmodifieDisplayInfo;
        private readonly PlatformController _platformController;
        private string _name;
        private string _description;
        private IAppCategory _category;
        internal AppDisplayUpdate(AppDisplayInfo displayInfo, PlatformController platformController)
        {
            _unmodifieDisplayInfo = displayInfo;
            _platformController = platformController;
            _name = displayInfo.Name;
            _description = displayInfo.Description;
            _category = displayInfo.Category;
            ApplicationId = displayInfo.ApplicationGuid;
            
        }

        public Guid ApplicationId { get; }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NameChanged = !_unmodifieDisplayInfo.Name.Equals(value);
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                DescriptionChanged = !_unmodifieDisplayInfo.Description.Equals(value);
            }
        }
        public IAppCategory Category
        {
            get => _category;
            set
            {
                _category = value;
                if(_unmodifieDisplayInfo.Category.Equals(value)) CategoryChanged = false;
                else CategoryChanged = true;
            }
        }
        public void ApplyChanges()
        {
            if(AnyChange())
            {
                _platformController.WhenAppDisplayUpdate.OnNext(this);
            }
        }
        public bool NameChanged { get; private set; }
        public bool DescriptionChanged { get; private set; }
        public bool CategoryChanged { get; private set; }
        internal bool AnyChange() { return NameChanged || DescriptionChanged || CategoryChanged; }

    }
}
