#region Licence
/****************************************************************
 *  Filename: InstallationProgress.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-2-27
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

namespace LeapVR.Shell.Domain.Models.Container.Installation
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallationProgress
    {

        #region Fields & Properties
        public Guid ApplicationGuid { get; }
        public string ApplicationName { get; }
        public InstallationPhases InstallationPhase { get; }
        public int PercentageDone { get; }
        public Exception Exception { get; }
        public IProgressAwarePackage CurrentInstallingPackage { get; }
        public IProgressAwarePackage[] Packages { get; }
        #endregion

        #region Constructors

        public InstallationProgress(
            Guid applicationGuid,
            string applicationName,
            InstallationPhases phase,
            int percentageDone,
            Exception exception,
            IProgressAwarePackage currentInstallingPackage,
            IProgressAwarePackage[] packages
            )
        {
            ApplicationGuid = applicationGuid;
            ApplicationName = applicationName;
            InstallationPhase = phase;
            PercentageDone = percentageDone;
            Exception = exception;
            CurrentInstallingPackage = currentInstallingPackage;
            Packages = packages;
        }
        #endregion

        #region Methods

        #endregion
    }
}
