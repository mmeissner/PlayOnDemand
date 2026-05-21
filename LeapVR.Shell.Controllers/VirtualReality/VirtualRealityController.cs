#region Licence
/****************************************************************
 *  Filename: VirtualRealityController.cs
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
using System.Reactive.Subjects;
using System.Threading.Tasks;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Module;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Vr;
using NLog;

namespace LeapVR.Shell.Controllers.VirtualReality
{
    public class VirtualRealityController:IVirtualRealityController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields
        private readonly object _lockChangeInProgress = new object();
        private readonly Subject<VrGuiState> _whenVrGuiStateChanged = new Subject<VrGuiState>();
        private readonly Subject<VrModuleState> _whenVrModuleStateChanged = new Subject<VrModuleState>();
        private readonly IEnumerable<IVrModule> _allVrModules;

        private volatile VrMode _mode = VrMode.Off;
        private volatile bool _isInitialized;
        private volatile VrModuleState _vrModuleState;
        private volatile VrGuiState _vrGuiState;
        private volatile IVrDesktopModule _vrDesktopModule;
        private volatile IEnumerable<IVrModule> _availibleVrModules;
        private TransparencyAreaCallBack _transparencyAreaCallback;
        private IVrModule _currentVrModule;
        private IApplicationExecution _vrAppInExecution;
        #endregion

        #region Properties
        
        public VrMode Mode => _mode;
        public IVrModule ActiveVrModule => _currentVrModule;
        public VrModuleState VrModuleState => _vrModuleState;
        public VrGuiState VrGuiState => _vrGuiState;
        public IEnumerable<IVrModule> AvailableVrModules => _availibleVrModules;

        public bool ForceDriverRestart { get;set; }
        public bool DisableDriverInteraction { get; set; }
        public IObservable<VrModuleState> WhenVrModuleStateChanged => _whenVrModuleStateChanged;
        public IObservable<VrGuiState> WhenVrGuiStateChanged => _whenVrGuiStateChanged;

        #endregion

        #region Constructor
        public VirtualRealityController(IEnumerable<IVrModule> vrModules, IVrDesktopModule vrDesktopModule)
        {
            ForceDriverRestart = true;
            DisableDriverInteraction = true;
            if(vrModules != null)
            {
                _allVrModules = vrModules;
                _availibleVrModules = _allVrModules.Where(module => module.IsAvailible).ToList();
            }
            else
            {
                _allVrModules = new IVrModule[0];
                _availibleVrModules = new IVrModule[0];
            }
            _vrDesktopModule = vrDesktopModule;
        }
        #endregion

        #region Public Methods
        public void OnExecutionMessage(AppExecutionMessage appExecutionMessage)
        {
            try
            {
                Logger.Debug($"Received Execution Message Phase={appExecutionMessage.Phase},TerminationRequested={appExecutionMessage.TerminationRequested}");
                lock (_lockChangeInProgress)
                {
                    Logger.Debug($"Processing Execution Message");
                    if (!_isInitialized) return;
                    bool isShutdownInProgress = false;
                    (bool needsVr, bool canProvideVrSupport, TerminationReason terminationReason) supportInfo;
                    //Check if a Systemshutdown is in progress and Stop all VR Activities if present
                    if (appExecutionMessage.TerminationRequested &&
                        appExecutionMessage.TerminationReasons.Contains(TerminationReason.SystemShutdown))
                    {
                        Logger.Info($"Execution Message includes System Shutdown as Termination Reason!");
                        isShutdownInProgress = true;
                    }
                    switch (appExecutionMessage.Phase)
                    {
                        case ExecutionPhase.BeforeStart:
                            //We do nothing if a shutdown is processing
                            if (isShutdownInProgress) return;

                            supportInfo = CanProvideVrSupport(appExecutionMessage.AppExecutionData.LogicToExecute.ReguiredVrModuleGuid);
                            if (supportInfo.needsVr && supportInfo.canProvideVrSupport)
                            {
                                //We can and will provide VR Support
                                OnStartVRApplication(appExecutionMessage.AppExecutionData);
                            }
                            else if (supportInfo.needsVr && !supportInfo.canProvideVrSupport)
                            {
                                // We cant provide the needed VR Support, or VR is Off
                                appExecutionMessage.RequestTermination(supportInfo.terminationReason);
                            }
                            else if (!supportInfo.needsVr)
                            {
                                //No VR Support Needed
                                OnStartNonVRApplication();
                            }
                            break;
                        case ExecutionPhase.OnFinished:
                            //If a shutdown is in Progress we Change our mode to Off
                            if (isShutdownInProgress)
                            {
                                ChangeMode(_mode, VrMode.Off);
                                return;
                            }

                            //Otherwise handle a normal stop
                            supportInfo = CanProvideVrSupport(appExecutionMessage.AppExecutionData.LogicToExecute.ReguiredVrModuleGuid);
                            if (supportInfo.needsVr && supportInfo.canProvideVrSupport)
                            {
                                //We have provided VR Support
                                OnStopVRApplication(appExecutionMessage.AppExecutionData);
                            }
                            if (!supportInfo.needsVr)
                            {
                                //We didnt had to provide VR Support
                                OnStopNonVRApplication();
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exception Occured");
                throw;
            }
            
        }
        public void OnStationMessage(StationMessage stationMessage)
        {
            Logger.Info($"Received Station Message: {stationMessage}!");
            switch (stationMessage)
            {
                case StationMessage.GuiStarted:
                    lock (_lockChangeInProgress)
                    {
                        _isInitialized = true;
                        ChangeMode(VrMode.Off, _mode);
                    }
                    break;
                case StationMessage.InitStop:
                    ChangeMode(_mode, VrMode.Off);
                    break;
            }
        }
        public async Task ChangeModeAsync(VrMode requestedMode)
        {
            Logger.Debug($"Request VrMode={requestedMode}, Current VrMode={_mode}");
            await Task.Run(() =>
                           {
                               lock (_lockChangeInProgress) { _mode = ChangeMode(_mode, requestedMode); }
                           });
        }
        public void ChangeMode(VrMode requestedMode)
        {
            lock (_lockChangeInProgress) { _mode = ChangeMode(_mode, requestedMode); }
        }
        public IEnumerable<ISelectableVrType> GetSelectableVrModules(IAppPlatformData platformData)
        {
            var retval = _allVrModules.Select(module => new SelectableVrType(module)).ToList();
            retval.Add(new SelectableVrType());
            foreach(var execution in platformData.ExecutionLogicInstructions)
            {
                if(String.IsNullOrWhiteSpace(execution.ReguiredVrModuleGuid))continue;
                var vrModuleGuid = Guid.Parse(execution.ReguiredVrModuleGuid);
                if(retval.Exists(x=> x.ModuleId.Equals(vrModuleGuid)))continue;
                retval.Add(new SelectableVrType(vrModuleGuid));
            }
            return retval;
        }
        #endregion

        #region Private Methods
        public Task SetActiveVRModuleAsync(IVrModule newModule)
        {
            //Check if there is currently one and handle vrgui and previous module
            //Then Handle new Module
            return Task.Run(() =>
            {
                try
                {
                    Logger.Debug("Task started!");
                    lock (_lockChangeInProgress)
                    {
                        Logger.Debug(
                            $"Is Initialized={_isInitialized}, " +
                            $"CurrentVrModule={_currentVrModule?.DisplayName}, " +
                            $"NewModule={newModule?.DisplayName}, " +
                            $"VrAppInExecution={_vrAppInExecution}");
                        if (!_isInitialized) _currentVrModule = newModule;
                        if (_vrAppInExecution != null) return;
                        var modeCurrentlySet = _mode;
                        var newCurrentMode = ChangeMode(modeCurrentlySet, VrMode.Off);
                        _currentVrModule = newModule;
                        ChangeMode(newCurrentMode, modeCurrentlySet);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Exception Occured");
                    throw;
                }
            });
        }

        public void SetUiInteractivity(
                TransparencyAreaCallBack transparencyAreaCallback)
        {
            _transparencyAreaCallback = transparencyAreaCallback;
        }

        private VrMode ChangeMode(VrMode currentMode, VrMode requestedMode)
        {
            //Check if there is currently a VrModule availible,
            //Then check its state and handle mode change
            lock (_lockChangeInProgress)
            {
                Logger.Info( $"Change Mode with VrMode={requestedMode}, Current VrMode={currentMode}");
                if (!_isInitialized) return requestedMode;
                if (ActiveVrModule == null) return VrMode.Off;
                bool isSuccess = false;
                switch (currentMode)
                {
                    case VrMode.Off:
                        switch (requestedMode)
                        {
                            case VrMode.Off:
                            case VrMode.OnDemand:
                                isSuccess = true;
                                break;
                            case VrMode.AllwaysOn:
                                isSuccess = StartVrDriver();
                                if(isSuccess)StartVrGui();
                                break;
                        }
                        break;
                    case VrMode.AllwaysOn:
                        switch (requestedMode)
                        {
                            case VrMode.Off:
                            case VrMode.OnDemand:
                                isSuccess = true;
                                StopVrGui();
                                StopVrDriver();
                                break;
                        }
                        break;
                    case VrMode.OnDemand:
                        switch (requestedMode)
                        {
                            case VrMode.OnDemand:
                            case VrMode.Off:
                                isSuccess = true;
                                break;
                            case VrMode.AllwaysOn:
                                isSuccess = StartVrDriver();
                                if(isSuccess)StartVrGui();
                                break;
                        }
                        break;
                }

                if(isSuccess) return requestedMode;
                return currentMode;
            }
        }

        private (bool needsVr,bool canProvideVrSupport, TerminationReason reason) CanProvideVrSupport(string requiredVrModuleGuid)
        {
            if(String.IsNullOrEmpty(requiredVrModuleGuid))return (false,false,TerminationReason.None);
            if(_mode == VrMode.Off) return (true, false,TerminationReason.VRModuleOff);
            if (_currentVrModule == null || !_currentVrModule.IsAvailible) return (true, false, TerminationReason.VRModuleUnavailible);
            return _currentVrModule.HasModuleSupport(Guid.Parse(requiredVrModuleGuid)) ? (true, true,TerminationReason.None) : (true, false, TerminationReason.VRModuleUnavailible);
        }

        private void OnStartVRApplication(IApplicationExecution applicationToProvideVR)
        {
            if (_vrAppInExecution != null)
            {
                Logger.Warn("Received Start for an VR Application, but there is already one in Execution!");
            }
            else
            {
                Logger.Info($"Start of VRApplication={applicationToProvideVR}, Current VrMode={_mode}");
                _vrAppInExecution = applicationToProvideVR;
                switch (_mode)
                {
                    case VrMode.OnDemand:
                        StartVrDriver();
                        break;
                    case VrMode.AllwaysOn:
                        StopVrGui();
                        break;
                }
            }
        }
        private void OnStopVRApplication(IApplicationExecution applicationToStop)
        {
            if (_vrAppInExecution.Equals(applicationToStop))
            {
                Logger.Info($"Stop of VRApplication={applicationToStop}, Current VrMode={_mode}");
                switch (Mode)
                {
                    case VrMode.OnDemand:
                    case VrMode.Off:
                        StopVrDriver();
                        break;
                    case VrMode.AllwaysOn:
                        if(ForceDriverRestart)
                        {
                            StopVrDriver();
                            StartVrDriver();
                        }
                        StartVrGui();
                        break;
                }
                _vrAppInExecution = null;
            }
            else
            {
                Logger.Warn("Received an application stop for a VR App we do not currently execute!");
            }
        }
        private void OnStartNonVRApplication()
        {
            switch (Mode)
            {
                case VrMode.Off:
                case VrMode.OnDemand:
                    Logger.Debug($"VRMode = {Mode}, VRModuleState={VrModuleState} . There is nothing to do");
                    return;
                case VrMode.AllwaysOn:
                    StopVrGui();
                    StopVrDriver();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void OnStopNonVRApplication()
        {
            switch (Mode)
            {
                case VrMode.Off:
                case VrMode.OnDemand:
                    Logger.Debug($"VRMode = {Mode}, VRModuleState={VrModuleState} . There is nothing to do");
                    return;
                case VrMode.AllwaysOn:
                    switch (VrModuleState)
                    {
                        case VrModuleState.Started:
                            Logger.Debug($"VRMode = {Mode}, VRModuleState={VrModuleState} . There is nothing to do");
                            break;
                        case VrModuleState.None:
                        case VrModuleState.Stopped:
                            StartVrGui();
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #region VRDriver Start and Stop

        private bool StartVrDriver()
        {
            try
            {
                Logger.Debug($"StartVrDriver CurrentVrModule.IsRunning= {_currentVrModule.IsRunning}");
                if (_currentVrModule.IsRunning) return true;
                bool result;
                if(_transparencyAreaCallback != null)
                {
                    result = _currentVrModule.StartVrDriver(DisableDriverInteraction,_transparencyAreaCallback, _vrDesktopModule.RestartVrDesktopModule);
                }
                else
                {
                    result= _currentVrModule.StartOnlyVrDriver();
                }
                if(result)
                {
                    _vrModuleState = VrModuleState.Started;
                    _whenVrModuleStateChanged.OnNext(_vrModuleState);
                }
                Logger.Debug($"Returning Result={result}");
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exception Occured");
                throw;
            }
        }
        private void StopVrDriver()
        {
            try
            {
                Logger.Debug($"StopVrDriver CurrentVrModule.IsRunning= {_currentVrModule.IsRunning}");
                if (!_currentVrModule.IsRunning) return;
                _currentVrModule.StopVrDriver();
                _vrModuleState = VrModuleState.Stopped;
                _whenVrModuleStateChanged.OnNext(_vrModuleState);
            }
            catch(Exception e)
            {
                Logger.Error(e,"Exception Occured");
                throw;
            }
        }
        #endregion

        private void StartVrGui()
        {
            try
            {
                Logger.Info("Starting VR-Gui");
                if (StartVrDriver())
                {
                    if (!_vrDesktopModule.ShouldBeRunning)
                    {
                        _vrGuiState = VrGuiState.Started;
                        _whenVrGuiStateChanged.OnNext(_vrGuiState);
                        _vrDesktopModule.ChangeShouldBeRunning(true);
                    }
                }
                else
                {
                    Logger.Warn("StartVrDriver returned false");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Exception Occured");
                throw;
            }
        }
        private void StopVrGui()
        {
            Logger.Info( $"Stopping VR-Gui with VrDesktopModule.ShouldBeRunning={_vrDesktopModule.ShouldBeRunning}");
            if (!_vrDesktopModule.ShouldBeRunning) return;
            _vrDesktopModule.ChangeShouldBeRunning(false);
            _vrGuiState = VrGuiState.Stopped;
            _whenVrGuiStateChanged.OnNext(_vrGuiState);
        }
        #endregion
    }
}
