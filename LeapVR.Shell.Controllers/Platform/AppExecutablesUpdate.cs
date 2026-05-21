#region Licence
/****************************************************************
 *  Filename: AppExecutablesUpdate.cs
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
using System.Linq;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Disk;
using LeapVR.Shell.Domain.Models.Module;

namespace LeapVR.Shell.Controllers.Platform
{
    class AppExecutablesUpdate : IAppExecutablesUpdate
    {
        private readonly PlatformController _platformController;
        private readonly IAppPlatformData _platformData;
        internal readonly List<EditableProcessExecutionLogic> Execution = new List<EditableProcessExecutionLogic>();
        public Guid ApplicationId { get; }
        public IEnumerable<ISelectableVrType> SelectableVrTypes { get; }
        public AppExecutablesUpdate(
                PlatformController platformController, IEnumerable<ISelectableVrType> selectableVrTypes,
                IAppPlatformData platformData)
        {
            _platformController = platformController;
            _platformData = platformData;
            SelectableVrTypes = selectableVrTypes;
            ApplicationId = platformData.ApplicationGuid;
            foreach(IProcessExecutionLogic executionLogic in platformData.ExecutionLogicInstructions)
            {
                Execution.Add(new EditableProcessExecutionLogic(_platformData, executionLogic));
            }
        }

        public IEditableProcessExecutionLogic CreateExecutionLogic()
        {
            return new EditableProcessExecutionLogic(_platformData);
        }

        public IEnumerable<IEditableProcessExecutionLogic> GetExecutionLogics() { return Execution; }

        public bool AddExecutionLogic(IEditableProcessExecutionLogic executionLogic)
        {
            if(!executionLogic.IsValid()) return false;
            if(executionLogic.IsValid() && executionLogic is EditableProcessExecutionLogic editableExecutionLogic)
            {
                if(executionLogic.IsNew || !Execution.Any(x => x.ExecutionGuid.Equals(executionLogic.ExecutionGuid)))
                {
                    Execution.Add(editableExecutionLogic);
                }

                return true;
            }

            return false;
        }

        public bool RemoveExecutionLogic(IEditableProcessExecutionLogic executionLogic)
        {
            if(Execution.Count <= 1 ||
               !(executionLogic is EditableProcessExecutionLogic editableProcessExecutionLogic)) return false;
            return Execution.Remove(editableProcessExecutionLogic);
        }

        public bool ApplyChanges()
        {
            if(!ValidateUpdate()) return false;
            _platformController.WhenAppExecutablesUpdate.OnNext(this);
            return true;
        }

        public bool ValidateUpdate()
        {
            if(Execution.Count == 0) return false;
            if(Execution.Any(x => !x.IsValid())) return false;
            return true;
        }
    }


    class EditableDiskEntity : IEditableDiskEntity
    {
        private readonly IDiskEntity _diskEntity;
        private DiskEntityType _type;
        private string _path;
        public DiskEntityType Type
        {
            get => _type;
            set
            {
                _type = value;
                CheckModificationState();
            }
        }
        public Guid ApplicationGuid { get; }
        public Guid PlatformGuid { get; }
        public Guid PackageGuid { get; private set; }
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                CheckModificationState();
            }
        }

        public EditableDiskEntity(IDiskEntity diskEntity)
        {
            _diskEntity = diskEntity;
            _type = diskEntity.Type;
            _path = diskEntity.Path;
            ApplicationGuid = diskEntity.ApplicationGuid;
            PlatformGuid = diskEntity.PlatformGuid;
            PackageGuid = diskEntity.PackageGuid;
        }

        public EditableDiskEntity(IAppPlatformData platformData)
        {
            _diskEntity = null;
            Type = DiskEntityType.Absolute;
            ApplicationGuid = platformData.ApplicationGuid;
            PlatformGuid = platformData.PlatformPluginId;
            PackageGuid = Guid.Empty;
        }

        public bool IsValid()
        {
            if(ApplicationGuid.Equals(Guid.Empty) ||
               PlatformGuid.Equals(Guid.Empty) ||
               String.IsNullOrWhiteSpace(Path)) return false;
            return true;
        }

        private void CheckModificationState()
        {
            if(_diskEntity == null) return;
            if(_path.Equals(_diskEntity.Path) && _type.Equals(_diskEntity.Type))
            {
                PackageGuid = _diskEntity.PackageGuid;
            }
            else
            {
                PackageGuid = Guid.Empty;
            }
        }
        public bool DataEquals(IDiskEntity other)
        {
            return _type == other.Type &&
                   string.Equals(_path, other.Path) &&
                   ApplicationGuid.Equals(other.ApplicationGuid) &&
                   PlatformGuid.Equals(other.PlatformGuid) &&
                   PackageGuid.Equals(other.PackageGuid);
        }
    }
}