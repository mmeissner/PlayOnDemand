#region Licence
/****************************************************************
 *  Filename: SystemController.cs
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
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.System;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using NLog;

namespace LeapVR.Shell.Controllers.System
{

    public class SystemController : ISystemController
    {
        #region Fields & Properties
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly QueryCancelAutoPlay _queryCancelAutoPlay;
        private readonly IApplicationHost _applicationHost;
        private readonly IStationController _stationController;
        private volatile bool _isStarted;


        public ILocalMachine LocationMachine { get; }
        #endregion

        #region Constructor
        public SystemController(
            IStationController stationController,
            ILocalMachine localMachineManager,
            IApplicationHost applicationHost)
        {
            QuickLeap.AssertNotNull(stationController, localMachineManager, applicationHost);
            _applicationHost = applicationHost;
            _stationController = stationController;
            _queryCancelAutoPlay = new QueryCancelAutoPlay();
            _queryCancelAutoPlay.Initialize();
            LocationMachine = localMachineManager;
        }

        #endregion

        #region Methods
        public void Initialize()
        {
            Logger.Debug("System Controller Intialization Requested");
            _stationController.Initialize(OnStationMessage, OnTerminationSignal);
        }

        private void OnStationMessage(StationMessage message)
        {            
            Logger.Debug($"Received {nameof(OnStationMessage)} = {message}");
            switch (message)
            {
                case StationMessage.Start:
                    if (_isStarted) return;
                    _applicationHost.ShowGUI();
                    _isStarted = true;
                    break;
            }
        }

        private void OnTerminationSignal(TerminationSignal signal)
        {
            Logger.Debug($"Received {nameof(OnTerminationSignal)} = {signal}");
            switch (signal)
            {
                case TerminationSignal.Close:
                    _queryCancelAutoPlay.Dispose();
                    _applicationHost.Shutdown();
                    break;
                case TerminationSignal.Restart:
                    _queryCancelAutoPlay.Dispose();
                    _applicationHost.Restart();
                    break;
                case TerminationSignal.PowerOff:
                    _queryCancelAutoPlay.Dispose();
                    _applicationHost.RequestPoweroff();
                    break;
            }
        }
        #endregion

    }
}
