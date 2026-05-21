#region Licence
/****************************************************************
 *  Filename: ContainerCreation.cs
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
using System.Threading.Tasks;
using LeapVR.Content.Shared.Container;
using LeapVR.Shared.Lib;
using LeapVR.Shell.Modules.Container;

namespace LeapVR.Content.Creator.Logic
{
    public abstract class ContainerCreation: IWizardModule
    {
        #region Properties & Fields

        public PlatformType PlatformType { get; }
        public string SteamId { get; set; }
        public string MainPictureFilePath { get; set; }
        public string ContainerOutputFilePath { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public List<IAppExecuteInstruction> Executables { get; set; }


        public abstract int TotalFilesCount { get; }
        public abstract int DoneFilesCount { get; }
        public abstract long TotalFilesSize { get; }
        public abstract long DoneFilesSize { get; }

        public abstract IObservable<Empty> WhenContainerCreationStarted { get; }
        public abstract IObservable<Empty> WhenProgressChanged { get; }
        public abstract IObservable<Empty> WhenContainerCreationEnded { get; }

        public abstract bool WasContainerCreationStarted { get; }
        public abstract bool IsContainerCreationEnded { get; }
        public abstract Exception OccuredException { get; }

        protected IContainerModule ContainerModule;

        #endregion Properties & Fields

        #region Constructors

        protected ContainerCreation(PlatformType platformType)
        {
            PlatformType = platformType;
            Executables = new List<IAppExecuteInstruction>();
            var v2Serializer = new AppInstallationHeaderSerializer();
            ContainerModule = new ContainerModule(v2Serializer);
        }

        #endregion Constructors

        #region Methods

        public abstract Task DoWork();

        #endregion Methods
    }
}
