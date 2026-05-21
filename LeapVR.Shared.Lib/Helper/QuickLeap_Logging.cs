#region Licence
/****************************************************************
 *  Filename: QuickLeap_Logging.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LeapVR.Shared.Lib.Helper
{
    public static partial class QuickLeap
    {
        public static StaticLogger Logger { get; } = new StaticLogger();

        internal static Dictionary<LogLevel, string> LogLevelStrings = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, "TRACE" },
            { LogLevel.Debug, "DEBUG" },
            { LogLevel.Info, "INFO" },
            { LogLevel.Warn, "WARN" },
            { LogLevel.Error, "ERROR" },
            { LogLevel.Fatal, "FATAL" },
        };

        internal static IEnumerable<Action<string, LogLevel>> LogActions;
        internal static bool LogDateTime;

        /// <summary>
        /// Configures global logging settings, specifying logging actions and parameters.
        /// </summary>
        /// <param name="logActions">Collection of actions to be executed every time logging is requested. Should redirect the message to other logging provider, do Console.WriteLine, or similar.</param>
        /// <param name="logDateTime">Indicates if current <see cref="DateTime"/> should be added to message when logging.</param>
        public static void ConfigureLogging(IEnumerable<Action<string, LogLevel>> logActions, bool logDateTime = false)
        {
            LogActions = logActions.ToList();
            LogDateTime = logDateTime;
        }

        /// <summary>
        /// Configures global logging settings, specifying logging actions and parameters.
        /// </summary>
        /// <param name="logActions">Collection of actions to be executed every time logging is requested. Should redirect the message to other logging provider, do Console.WriteLine, or similar.</param>
        /// <param name="logDateTime">Indicates if current <see cref="DateTime"/> should be added to message when logging.</param>
        public static void ConfigureLogging(Action<string, LogLevel> logAction, bool logDateTime = false)
        {
            LogActions = new []{logAction};
            LogDateTime = logDateTime;
        }

        [Obsolete]
        public static Expression<Func<object>>[] GetEnv(params Expression<Func<object>>[] env)
        {
            return env;
        }

        /// <summary>
        /// Gets name of member calling this method.
        /// </summary>
        /// <param name="callerName"></param>
        /// <returns></returns>
        public static string GetCallerName([CallerMemberName] string callerName = null)
        {
            return callerName;
        }

        public enum LogLevel
        {
            Unknown = 0,

            Trace = 1,
            Debug = 2,
            Info  = 3,
            Warn = 4,
            Error = 5,
            Fatal = 6,
        }

        public class StaticLogger
        {
            internal StaticLogger()
            {
                //
            }
        }

        public static class Print
        {
            public static string AqquireLock(object lockObj)
            {
                return $"Aquired Execution Lock of {nameof(lockObj)}";
            }
            public static string ReleaseLocK(object lockObj)
            {
                return $"Releaseing Execution Lock of {nameof(lockObj)}";
            }
        }
    }
}