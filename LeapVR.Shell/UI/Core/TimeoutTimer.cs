#region Licence
/****************************************************************
 *  Filename: TimeoutTimer.cs
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

namespace LeapVR.Shell.UI.Core
{
    public class TimeoutTimer : IDisposable
    {

        #region Fields & Properties
        // how long the heartbeat interval will be
        private readonly TimeSpan _heartbeatInterval;
        // specify how long it already ticks
        private TimeSpan _elapsed;
        private readonly System.Timers.Timer _ticker;
        private Action _callBack;

        public TimeSpan Timeout { get; }
        public bool IsTimeout { get; private set; }

        public Action CallBack
        {
            get { return _callBack; }
        }

        #endregion

        #region Constructors
        public TimeoutTimer(TimeSpan timeout,Action timeoutCallback)
        {
            Timeout = timeout;
            _heartbeatInterval = TimeSpan.FromSeconds(1);
            _elapsed = TimeSpan.Zero;
            IsTimeout = false;

            _ticker = new System.Timers.Timer
            {
                Interval = _heartbeatInterval.TotalMilliseconds,
                AutoReset = true
            };
            _ticker.Elapsed += _ticker_Elapsed;
            _callBack = timeoutCallback;
        }
        #endregion

        #region Methods
        public void Start()
        {
            _ticker.Start();
        }

        public void Reset()
        {
            _elapsed = TimeSpan.Zero;
        }

        public void Stop()
        {
            _ticker.Stop();
            IsTimeout = true;
        }

        public void SetCallBack(Action callbackAction)
        {
            _callBack = callbackAction;
        }
        private void _ticker_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_elapsed < Timeout)
            {
                _elapsed = _elapsed + _heartbeatInterval;
            }
            else
            {
                Stop();
                _callBack?.Invoke();
            }
        }
        #endregion


        public void Dispose()
        {
            _ticker?.Dispose();
        }
    }
}
