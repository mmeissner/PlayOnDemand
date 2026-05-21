#region Licence
/****************************************************************
 *  Filename: LogFileMonitorUtil.cs
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
using System.IO;
using System.Timers;

namespace LeapVR.Utilities.Windows
{
    public class LogFileMonitorUtil
    {
        private object _locker = new object();
        // file path + delimiter internals
        readonly string _path;
        readonly string _delimiter;
        // timer object
        private Timer _timer = null;
        // buffer for storing data at the end of the file that does not yet have a delimiter
        private string _buffer = string.Empty;
        // get the current size
        private long _currentSize = 0;
        // are we currently checking the log (stops the timer going in more than once)
        private bool _isCheckingLog = false;

        public LogFileMonitorUtil(string path, string delimiter = "\n")
        {
            _path = path;
            _delimiter = delimiter;
        }
        public EventHandler<LogFileMonitorLineEventArgs> OnLine { get; set; }
        protected bool StartCheckingLog()
        {
            lock (_locker)
            {
                if (_isCheckingLog)
                    return true;

                _isCheckingLog = true;
                return false;
            }
        }
        protected void DoneCheckingLog()
        {
            lock (_locker)
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
        public void Stop()
        {
            if (_timer == null)
                return;

            _timer.Stop();
        }

        private void CheckLog(object s, ElapsedEventArgs e)
        {
            if (StartCheckingLog())
            {
                try
                {
                    // get the new size
                    var newSize = new FileInfo(_path).Length;

                    // if they are the same then continue.. if the current size is bigger than the new size continue
                    if (_currentSize >= newSize)
                        return;

                    // read the contents of the file
                    using (var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        // seek to the current file position
                        sr.BaseStream.Seek(_currentSize, SeekOrigin.Begin);

                        // read from current position to the end of the file
                        var newData = _buffer + sr.ReadToEnd();

                        // if we don't end with a delimiter we need to store some data in the buffer for next time
                        if (!newData.EndsWith(_delimiter))
                        {
                            // we don't have any lines to process so save in the buffer for next time
                            if (newData.IndexOf(_delimiter) == -1)
                            {
                                _buffer += newData;
                                newData = String.Empty;
                            }
                            else
                            {
                                // we have at least one line so store the last section (without lines) in the buffer
                                var pos = newData.LastIndexOf(_delimiter) + _delimiter.Length;
                                _buffer = newData.Substring(pos);
                                newData = newData.Substring(0, pos);
                            }
                        }

                        // split the data into lines
                        var lines = newData.Split(new string[] { _delimiter }, StringSplitOptions.RemoveEmptyEntries);

                        // send back to caller, NOTE: this is done from a different thread!
                        foreach (var line in lines)
                        {
                            OnLine?.Invoke(this, new LogFileMonitorLineEventArgs { Line = line });
                        }
                    }

                    // set the new current position
                    _currentSize = newSize;
                }
                catch (Exception)
                {
                }

                // we done..
                DoneCheckingLog();
            }
        }
        public class LogFileMonitorLineEventArgs : EventArgs
        {
            public string Line { get; set; }
        }
    }
}