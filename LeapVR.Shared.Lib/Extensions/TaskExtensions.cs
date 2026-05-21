#region Licence
/****************************************************************
 *  Filename: TaskExtensions.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-2
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NLog;

namespace LeapVR.Shared.Lib.Extensions
{
    public static class TaskExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Forgets about Task, however logging error message when it throws exception.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="callerName"></param>
        /// <returns></returns>
        public static Task Forget(this Task self, [CallerMemberName] string callerName = null)
        {
            self.ContinueWith(t => LogTaskException(t, callerName), TaskContinuationOptions.OnlyOnFaulted);
            return self;
        }

        private static void LogTaskException(Task t, string callerName)
        {
            Logger.Error($"In forgotten task (called from `{callerName}`) exception occured `{t.Exception}`.");
        }
    }
}
