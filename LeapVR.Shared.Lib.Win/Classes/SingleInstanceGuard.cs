#region Licence
/****************************************************************
 *  Filename: SingleInstanceGuard.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-30
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
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace LeapVR.Shared.Lib.Win.Classes
{
    /// <summary>
    /// Is responsible for having one and only one instance of application running at the same time.
    /// Uses Global <see cref="Mutex"/> to achieve this.
    /// </summary>
    public sealed class SingleInstanceGuard : IDisposable
    {
        private readonly string _instanceName;
        private readonly Mutex _mutex;

        private SingleInstanceGuard(string instanceName, Mutex mutex)
        {
            _instanceName = instanceName;
            _mutex = mutex;
        }

        /// <summary>
        /// Tries to acquire <see cref="SingleInstanceGuard"/> for specified <see cref="instanceName"/>.
        /// If there is already instance using the same <see cref="instanceName"/> running returns false.
        /// </summary>
        /// <param name="instanceName">Unique application identifier. Only one instance with the same name can be running</param>
        /// <param name="guard">Guard object returned of acquiring suceeded, or null if failed</param>
        /// <returns>Boolean indicating if acquiring <see cref="SingleInstanceGuard"/> suceeded</returns>
        public static bool TryAcquire(string instanceName, out SingleInstanceGuard guard)
        {
            Mutex mutex = null;
            try
            {
                // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
                // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
                var allowEveryoneRule =
                    new MutexAccessRule(
                        new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                        MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);

                mutex = new Mutex(false, $@"Global\SingleInstance_{instanceName}", out var createdNew, securitySettings);

                bool isMutexObtained;
                try
                {
                    isMutexObtained = mutex.WaitOne(0, false);
                }
                catch (AbandonedMutexException)
                {
                    // The exception that is thrown when one thread acquires a Mutex object that another thread has abandoned by exiting without releasing it.
                    // swallow
                    isMutexObtained = true;
                }

                guard = new SingleInstanceGuard(instanceName, mutex);
                return isMutexObtained;
            }
            catch
            {
                try
                {
                    mutex?.ReleaseMutex();
                }
                catch
                {
                    // swallow
                }

                guard = null;
                return false;
            }
        }

        /// <summary>
        /// Releases guard and let other application acquire it.
        /// Should be called just before application closes.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _mutex?.ReleaseMutex();
            }
            catch
            {
                // swallow
            }
        }
    }
}
