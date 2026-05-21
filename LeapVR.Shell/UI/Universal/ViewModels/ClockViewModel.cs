#region Licence
/****************************************************************
 *  Filename: ClockViewModel.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2017-12-22
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
using System.Windows.Threading;
using Caliburn.Micro;
using NLog;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    public class ClockViewModel : Screen, IDisposable
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #region Fields & Properties
        public string Time => DateTime.Now.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.FullDateTimePattern);

        private DispatcherTimer _timer;

        #endregion

        #region Constructors
        public ClockViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += TimerOnTick;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();
        }
        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            try
            {
                NotifyOfPropertyChange(() => Time);
            }
            catch (Exception exception)
            {
                //Can throw on application shutdown
                Logger.Warn(exception,$"Exception thrown on {nameof(ClockViewModel)}");
            }
        }
        #endregion

        #region Methods
        public void Dispose()
        {
            _timer?.Stop();
            _timer = null;
        }

        #endregion
    }
}
