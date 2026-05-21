#region Licence
/****************************************************************
 *  Filename: Dummies.cs
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
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Modules.Interfaces.Vr;

namespace LeapVR.Shell.Setup
{
    class AppInfoProcessorDummy : IAppInfoProcessor
    {
        public IEnumerable<IExecuteable> GetExecutionInfoResult(
                IEnumerable<IProcessExecutionLogic> processExecutionInstructions,bool needsFullfilledRequirements)
        {
            throw new NotImplementedException();
        }
    }

    class CategoryProviderDummy: ICategoryProvider
    {
        public IAppCategory GetOrCreateAppCategory(string identifier) { throw new NotImplementedException(); }
        public IEnumerable<IAppCategory> GetAllCategories { get; set; } = new IAppCategory[0];
    }

    class VirtualRealityControllerDummy : IVirtualRealityController
    {
        public void OnExecutionMessage(AppExecutionMessage message) { throw new NotImplementedException(); }
        public void OnStationMessage(StationMessage messages) { throw new NotImplementedException(); }
        public VrMode Mode { get; }
        public IVrModule ActiveVrModule { get; }
        public VrModuleState VrModuleState { get; }
        public VrGuiState VrGuiState { get; }
        public IEnumerable<IVrModule> AvailableVrModules { get; }
        public IObservable<VrModuleState> WhenVrModuleStateChanged { get; }
        public bool ForceDriverRestart { get; set; }
        public bool DisableDriverInteraction { get; set; }
        public IObservable<VrGuiState> WhenVrGuiStateChanged { get; }
        public void ChangeMode(VrMode requestedMode) { throw new NotImplementedException(); }
        public async Task ChangeModeAsync(VrMode requestedMode) { throw new NotImplementedException(); }
        public async Task SetActiveVRModuleAsync(IVrModule module) { throw new NotImplementedException(); }
        public void SetUiInteractivity(TransparencyAreaCallBack transparencyAreaCallback) { throw new NotImplementedException(); }
        public IEnumerable<ISelectableVrType> GetSelectableVrModules(IAppPlatformData executablesUpdate) { throw new NotImplementedException(); }
    }

    class LocalMachineDummy : ILocalMachine
    {
        public Version SoftwareVersion { get; }
        public string VBoxFingerprint { get; }
        public string CpuDetails { get; }
        public string VgaDetails { get; }
        public string RamDetails { get; }
    }
}
