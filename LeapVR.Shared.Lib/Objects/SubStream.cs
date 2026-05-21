#region Licence
/****************************************************************
 *  Filename: SubStream.cs
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
using System.IO;

namespace LeapVR.Shared.Lib.Objects
{
    /// <summary>
    /// Stream that limits I/O operations on underlying Stream to some specific locations in it.
    /// </summary>
    public class SubStream : Stream
    {
        #region Properties & Fields

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => !_hasLenghtLimit;
        public override bool CanTimeout => _baseStream.CanTimeout;
        public override long Length => _lengthLimit != null
            ? Math.Min(_baseStream.Length - _startPosition, _lengthLimit.Value)
            : _baseStream.Length - _startPosition;

        public override long Position
        {
            get => GetVirtualPosition(_baseStream.Position);
            set => throw new NotSupportedException("SubStream does not support Seeking!");
        }

        public override int ReadTimeout
        {
            get { return _baseStream.ReadTimeout; }
            set { _baseStream.ReadTimeout = value; }
        }

        private readonly long _startPosition;
        private readonly long? _lengthLimit;
        private readonly bool _leaveOpen;
        private readonly bool _hasLenghtLimit;

        private readonly Stream _baseStream;

        #endregion Properties & Fields

        #region Constructors

        public SubStream(Stream baseStream, long? lengthLimit = null, bool leaveOpen = false)
        {
            if (baseStream == null)
            {
                throw new ArgumentNullException();
            }
            if (lengthLimit.HasValue)
            {
                _hasLenghtLimit = true;
                if(lengthLimit.Value < 0) throw new ArgumentOutOfRangeException(nameof(lengthLimit), $"{lengthLimit} is less than 0.");
            }
            _baseStream = baseStream;
            _startPosition = baseStream.Position;
            _lengthLimit = lengthLimit;
            _leaveOpen = leaveOpen;
        }

        #endregion Constructors

        #region Methods

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long virtualPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    virtualPosition = offset;
                    break;
                case SeekOrigin.Current:
                    virtualPosition = Position + offset;
                    break;
                case SeekOrigin.End:
                    virtualPosition = (_lengthLimit ?? Length) + offset;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid value of {nameof(origin)} ({origin}).");
            }
            _baseStream.Seek(GetRealPosition(virtualPosition), SeekOrigin.Begin);
            return virtualPosition;
        }

        public override void SetLength(long value)
        {
            //throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return _baseStream.Read(buffer, offset, count);
            }
            if (Position >= _lengthLimit)
            {
                return 0;
            }
            if (Position + count > _lengthLimit)
            {
                return _baseStream.Read(buffer, offset, Convert.ToInt32(_lengthLimit - Position));
            }
            return _baseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_hasLenghtLimit)
                throw new NotImplementedException(
                    "Can't write to a size limited SubStream. This would result in corupted or unchanged data (mostly)!");
            if (count == 0)
            {
                _baseStream.Write(buffer, offset, count);
                return;
            }
            _baseStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                {
                    _baseStream.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private long GetRealPosition(long virtualPosition)
        {
            return _startPosition + virtualPosition;
        }

        private long GetVirtualPosition(long realPosition)
        {
            return realPosition - _startPosition;
        }

        #endregion Methods
    }
}
