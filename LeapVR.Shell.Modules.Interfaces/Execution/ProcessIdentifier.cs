#region Licence
/****************************************************************
 *  Filename: ProcessIdentifier.cs
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

namespace LeapVR.Shell.Modules.Interfaces.Execution
{
    /// <summary>
    /// A state collection that uniquely presents a real process running in windows.
    /// </summary>
    public struct ProcessIdentifier : IEquatable<ProcessIdentifier>, IComparable<ProcessIdentifier>
    {
        #region Fields & Properties
        public int Id { get; }
        public string Name { get; }
        public DateTime StartTime { get; }
        public int? ParentProcessId { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a process identifer.
        /// </summary>
        /// <param name="id">the process id</param>
        /// <param name="name">process name</param>
        /// <param name="startTime">the time when a process get started</param>
        /// <param name="parentProcessId">the parent process id if there is</param>
        public ProcessIdentifier(int id, string name, DateTime startTime, int? parentProcessId = null)
        {
            Id = id;
            Name = name;
            StartTime = startTime;
            ParentProcessId = parentProcessId;
        }
        #endregion

        #region Methods

        public static bool operator ==(ProcessIdentifier id1, ProcessIdentifier id2)
        {
            return id1.Equals(id2);
        }

        public static bool operator !=(ProcessIdentifier id1, ProcessIdentifier id2)
        {
            return !id1.Equals(id2);
        }

        public bool Equals(ProcessIdentifier other)
        {
            var isTheSame =
                Id == other.Id
                && ParentProcessId == other.ParentProcessId;
            return isTheSame;
        }
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ProcessIdentifier other)
        {
            return Math.Max(Id, other.Id);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ProcessIdentifier identifier && Equals(identifier);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ StartTime.GetHashCode();
                hashCode = (hashCode * 397) ^ ParentProcessId.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
