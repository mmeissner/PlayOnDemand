#region Licence
/****************************************************************
 *  Filename: NLogLogger.cs
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
using Caliburn.Micro;

namespace LeapVR.Shell
{
    public class NLogLogger : ILog
    {
        #region Fields

        private readonly NLog.Logger _innerLogger;

        #endregion

        #region Constructors

        public NLogLogger(Type type)
        {
            _innerLogger = NLog.LogManager.GetLogger(type.Name);
        }

        #endregion

        #region ILog Members

        public void Error(Exception exception)
        {
            _innerLogger.Error(exception, exception.Message);
        }

        public void Info(string format, params object[] args)
        {
            _innerLogger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _innerLogger.Warn(format, args);
        }

        #endregion
    }
}