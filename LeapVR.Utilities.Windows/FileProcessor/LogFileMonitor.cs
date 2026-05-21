#region Licence
/****************************************************************
 *  Filename: LogFileMonitor.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-1-23
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
using System.IO;
using System.Timers;

namespace LeapVR.Utilities.Windows.FileProcessor
{
    public class LogFileMonitor
    {
        private object _checkLogLocker = new object();
        public EventHandler<LogFileMonitorLineEventArgs> OnNewline;


        #region Fields & Properties
        // file path + delimiter internals
        readonly string _path;
        readonly string _delimiter;

        // timer object
        Timer _timer;

        // buffer for storing data at the end of the file that does not yet have a delimiter
        string _buffer = String.Empty;

        // get the current size
        long _currentSize = 0;

        // are we currently checking the log (stops the timer going in more than once)
        bool _isCheckingLog = false;

        #endregion

        #region Constructors
        public LogFileMonitor(string path, string delimiter = "\n")
        {
            _path = path;
            _delimiter = delimiter;
        }

        #endregion

        #region Methods
        protected bool StartCheckingLog()
        {
            lock (_checkLogLocker)
            {
                if (_isCheckingLog)
                    return true;

                _isCheckingLog = true;
                return false;
            }
        }

        protected void DoneCheckingLog()
        {
            lock (_checkLogLocker)
                _isCheckingLog = false;
        }

        public void Start()
        {
            // get the current size
            _currentSize = new FileInfo(_path).Length;

            // start the timer
            _timer = new Timer();
            _timer.Elapsed += CheckLog;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void CheckLog(object s, ElapsedEventArgs e)
        {
            if (!StartCheckingLog()) return;

            try
            {
                // get the new size
                var newSize = new FileInfo(_path).Length;
                // if they are the same then continue.. if the current size is bigger than the new size continue
                if (_currentSize >= newSize)
                    return;

                // read the contents of the file
                using (var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(stream))
                {
                    // seek to the current file position
                    sr.BaseStream.Seek(_currentSize, SeekOrigin.Begin);

                    // read from current position to the end of the file
                    var newData = _buffer + sr.ReadToEnd();

                    // if we don't end with a delimiter we need to store some data in the buffer for next time
                    if (!newData.EndsWith(_delimiter))
                    {
                        // we don't have any lines to process so save in the buffer for next time
                        if (newData.IndexOf(_delimiter, StringComparison.Ordinal) == -1)
                        {
                            _buffer += newData;
                            newData = string.Empty;
                        }
                        else
                        {
                            // we have at least one line so store the last section (without lines) in the buffer
                            var pos = newData.LastIndexOf(_delimiter, StringComparison.Ordinal) + _delimiter.Length;
                            _buffer = newData.Substring(pos);
                            newData = newData.Substring(0, pos);
                        }
                    }

                    // split the data into lines
                    var lines = newData.Split(new string[] { _delimiter }, StringSplitOptions.RemoveEmptyEntries);

                    // send back to caller, NOTE: this is done from a different thread!
                    foreach (var line in lines)
                    {
                        OnNewline?.Invoke(this, new LogFileMonitorLineEventArgs { Line = line });
                    }
                }

                // set the new current position
                _currentSize = newSize;
            }
            catch (Exception)
            {
                // ignored
            }

            // we done..
            DoneCheckingLog();
        }

        public void Stop()
        {
            _timer?.Stop();
        }

        #endregion
    }

}
