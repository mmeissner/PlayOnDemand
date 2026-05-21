#region Licence
/****************************************************************
 *  Filename: SubsequentProcessTracker.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-1-15
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using LeapVR.VBox.Modules.Interfaces.Execution;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LeapVR.Shared.NetStandard.Extensions;

namespace LeapVR.Utilities.Windows.Processes
{
    /// <inheritdoc />
    /// <summary>
    /// A tracker tracks all subsequent processes after a specific <see cref="T:LeapVR.VBox.Modules.Interfaces.Execution.ProcessIdentifier" /> and notify to subscribers when there're new process created or old process deleted.
    /// </summary>
    public class SubsequentProcessTracker : IDisposable
    {
        /// <summary>
        /// Get the process where it starts to track.
        /// </summary>
        public ProcessIdentifier RootProcess { get; }
        /// <summary>
        /// Get all running processes tracked during application lifetime.
        /// </summary>
        public ObservableCollection<ProcessIdentifier> RunningProcesses { get; }

        private ManagementEventWatcher _processCreationWatcher;
        private readonly ReplaySubject<ProcessIdentifier> _whenProcessCreatedSubject = new ReplaySubject<ProcessIdentifier>();
        public IObservable<ProcessIdentifier> WhenProcessCreated => _whenProcessCreatedSubject.AsObservable();

        private ManagementEventWatcher _processDeletionWatcher;
        private readonly ReplaySubject<ProcessIdentifier> _whenProcessDeletedSubject = new ReplaySubject<ProcessIdentifier>();
        public IObservable<ProcessIdentifier> WhenProcessDeleted => _whenProcessDeletedSubject.AsObservable();


        private readonly double _pollingInterval;
        private readonly string _scope;

        /// <summary>
        /// Initialize a instance of <see cref="SubsequentProcessTracker"/>.
        /// </summary>
        /// <param name="rootProcess">which process it should track after.</param>
        /// <param name="pollingInterval">how many seconds between two poll</param>
        /// <param name="machineName">machine name, it will set to local machine if given null. By default null.</param>
        public SubsequentProcessTracker(ProcessIdentifier rootProcess, double pollingInterval, string machineName = null)
        {
            RunningProcesses = new ObservableCollection<ProcessIdentifier>();

            RootProcess = rootProcess;
            RunningProcesses.Add(RootProcess);
            _pollingInterval = pollingInterval;
            // You could replace the dot by a machine name to watch to that machine
            _scope = string.IsNullOrEmpty(machineName) ? @"\.\root\CIMV2" : $"\\{machineName}\\root\\CIMV2";
        }

        public void Start()
        {

            // Create event query to be notified within 1 second of 
            // a change in a service
            var processCreationQuery =
                //new WqlEventQuery("__InstanceModificationEvent", TimeSpan.FromSeconds(1), "TargetInstance isa \"Win32_Service\"");
                $"SELECT * FROM __InstanceCreationEvent WITHIN {_pollingInterval} WHERE TargetInstance ISA 'Win32_Process'";

            // Initialize an event watcher and subscribe to events 
            // that match this query
            _processCreationWatcher = new ManagementEventWatcher(_scope, processCreationQuery);
            _processCreationWatcher.EventArrived += OnNewProcessCreated;
            _processCreationWatcher.Start();



            var processDeletionQuery =
                //new WqlEventQuery("__InstanceModificationEvent", TimeSpan.FromSeconds(1), "TargetInstance isa \"Win32_Service\"");
                $"SELECT * FROM __InstanceDeletionEvent WITHIN {_pollingInterval} WHERE TargetInstance ISA 'Win32_Process'";
            _processDeletionWatcher = new ManagementEventWatcher(_scope, processDeletionQuery);
            _processDeletionWatcher.EventArrived += OnProcessDeleted;
            _processDeletionWatcher.Start();
            
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _processCreationWatcher.EventArrived -= OnNewProcessCreated;
            _processCreationWatcher?.Stop();
            _processCreationWatcher?.Dispose();
            _processDeletionWatcher.EventArrived -= OnProcessDeleted;
            _processDeletionWatcher?.Stop();
            _processDeletionWatcher?.Dispose();
        }

        private void OnNewProcessCreated(object sender, EventArrivedEventArgs e)
        {
            var processIdentifier = GetProcessIdentifierFromEventArrivedEventArgs(e);

            if (RunningProcesses.Any(p => p.Id == processIdentifier.ParentProcessId))
            {
                RunningProcesses.Add(processIdentifier);
                this.LogInfo($"[{DateTime.Now}] detected a new process '{processIdentifier.Id}' '{processIdentifier.Name}' created by process '{processIdentifier.ParentProcessId}'");
                _whenProcessCreatedSubject.OnNext(processIdentifier);
            }

        }

        private void OnProcessDeleted(object sender, EventArrivedEventArgs e)
        {
            var processIdentifier = GetProcessIdentifierFromEventArrivedEventArgs(e);
            var targetProcessIdentifier = RunningProcesses.FirstOrDefault(p => p.Id == processIdentifier.Id && p.StartTime == processIdentifier.StartTime);
            if (RunningProcesses.Contains(targetProcessIdentifier))
            {
                RunningProcesses.Remove(targetProcessIdentifier);
                _whenProcessDeletedSubject.OnNext(processIdentifier);
            }
        }


        private ProcessIdentifier GetProcessIdentifierFromEventArrivedEventArgs(EventArrivedEventArgs e)
        {
            var processIdStr = ((ManagementBaseObject)e.NewEvent["TargetInstance"])["ProcessId"].ToString();
            var parentProcessIdStr = ((ManagementBaseObject)e.NewEvent["TargetInstance"])["ParentProcessID"].ToString();
            var creationDateStr = ((ManagementBaseObject)e.NewEvent["TargetInstance"])["CreationDate"].ToString();
            var processName = ((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"].ToString();


            var processId = int.Parse(processIdStr);
            var parentProcessId = int.Parse(parentProcessIdStr);
            var startTime = ManagementDateTimeConverter.ToDateTime(creationDateStr);

            var processIdentifier = new ProcessIdentifier(processId, processName, startTime, parentProcessId);

            return processIdentifier;
        }

    }
}
