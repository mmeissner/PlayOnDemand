#region Licence
/****************************************************************
 *  Filename: ControllerShortcutWatch.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.RemoteService;
using LeapVR.Shell.Controllers.Station;
using LeapVR.Shell.Domain.Models.Input.OpenVR;
using LeapVR.Shell.Modules.Interfaces.Vr;
using NLog;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable NotAccessedField.Local

namespace LeapVR.Shell.Controllers.Behavior
{
    internal class ControllerShortcutWatch : IControllerShortcutWatch
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ConditionScope ConditionScope { get; }

        private readonly IControllerKeyAction[] _keyActions;
        public IEnumerable<IControllerKeyAction> KeyActions { get; }

        private readonly object _lock = new object();
        public bool IsSatisfied { get; private set; }
        private readonly BehaviorSubject<bool> _whenIsSatisfiedChangedSubject;
        public IObservable<bool> WhenIsSatisfiedChanged { get; }

        private readonly Dictionary<ControllerKeyActionWatch, bool> _keyActionWatchesSatisfactionMap;

        private readonly ControllerKeyActionWatch[] _keyActionWatches;
        private readonly IDisposable[] _keyActionWatchesSubscriptions;

        private readonly StationController _stationController;
        private readonly RemoteServiceController _remoteServiceController;
        private readonly IHmdActivityWatchdog _hmdActivityWatchdog;

        internal ControllerShortcutWatch(
            StationController stationController,
            RemoteServiceController remoteServiceController,
            IHmdActivityWatchdog hmdActivityWatchdog, 
            ConditionScope conditionScope, 
            IEnumerable<IControllerKeyAction> keyActions)
        {
            _keyActions = keyActions?.ToArray();

            QuickLeap.AssertNotNullEx(stationController, hmdActivityWatchdog, _keyActions);
            _stationController = stationController;
            _remoteServiceController = remoteServiceController;
            _hmdActivityWatchdog = hmdActivityWatchdog;
            ConditionScope = conditionScope;
            // ReSharper disable once AssignNullToNotNullAttribute
            KeyActions = new ReadOnlyCollection<IControllerKeyAction>(_keyActions);

            IsSatisfied = false;
            _whenIsSatisfiedChangedSubject = new BehaviorSubject<bool>(IsSatisfied);
            WhenIsSatisfiedChanged = _whenIsSatisfiedChangedSubject.AsObservable();

            _keyActionWatches = _keyActions
                .Select(q => new ControllerKeyActionWatch(hmdActivityWatchdog, q))
                .ToArray();

            _keyActionWatchesSatisfactionMap = _keyActionWatches
                .ToDictionary(q => q, q => false);

            var keyActionWatchSubscriptionsList = new List<IDisposable>();
            foreach (var keyActionWatch in _keyActionWatches)
            {
                var subscription = keyActionWatch.WhenIsSatisfiedChanged.Subscribe(q => OnSatisfiedChanged(keyActionWatch, q));
                keyActionWatchSubscriptionsList.Add(subscription);
            }
            _keyActionWatchesSubscriptions = keyActionWatchSubscriptionsList.ToArray();
        }

        public void Dispose()
        {
            foreach (var keyActionWatchSubscription in _keyActionWatchesSubscriptions)
            {
                keyActionWatchSubscription.Dispose();
            }

            foreach (var keyActionWatch in _keyActionWatches)
            {
                keyActionWatch.Dispose();
            }
        }

        private void OnSatisfiedChanged(ControllerKeyActionWatch keyActionWatch, bool isSatisfied)
        {
            lock (_lock)
            {
                _keyActionWatchesSatisfactionMap[keyActionWatch] = isSatisfied;

                var isAllKeysSatisfied = _keyActionWatchesSatisfactionMap.All(kv => kv.Value);
                var isConditionScopeSatisfied = CheckConditionScopeSatisfied(ConditionScope);
                var isAllSatisfied = isAllKeysSatisfied && isConditionScopeSatisfied;

                //this.LogDebug($"isAllKeysSatisfied = `{isAllKeysSatisfied}`, isConditionScopeSatisfied = `{isConditionScopeSatisfied}`, isAllSatisfied = `{isAllSatisfied}`.");

                if (IsSatisfied != isAllSatisfied)
                {
                    Logger.Info($"new value for IsSatisfied: `{isAllSatisfied}`.");

                    IsSatisfied = isAllSatisfied;
                    _whenIsSatisfiedChangedSubject.OnNext(isAllSatisfied);
                }
            }
        }

        private bool CheckConditionScopeSatisfied(ConditionScope conditionScope)
        {
            var isSessionRunning = _remoteServiceController.IsSessionRunning();
            var isGameRunning = _stationController.CurrentlyExecuting;

            return (conditionScope.HasFlag(ConditionScope.NoSession) && !isSessionRunning)
                   || (conditionScope.HasFlag(ConditionScope.SessionNoGame) && isSessionRunning && !isGameRunning)
                   || (conditionScope.HasFlag(ConditionScope.SessionGame) && isSessionRunning && isGameRunning);
        }
    }
}
