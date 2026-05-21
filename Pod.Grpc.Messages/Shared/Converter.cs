#region Licence
/****************************************************************
 *  Filename: Converter.cs
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
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

namespace Pod.Grpc.Messages.Shared
{
    partial class GuidAsBytes
    {
        public Guid? ToGuidNullable()
        {
            if(Value.IsEmpty) return null;
            return new Guid(Value.ToByteArray());
        }
        public Guid ToGuid()
        {
            return new Guid(Value.ToByteArray());
        }
    }

    partial class TimeSpanAsLong
    {
        public TimeSpan? ToTimeSpanNullable()
        {
            if(!HasValue) return null;
            return new TimeSpan(Value);
        }

        public bool ToTimeSpan(out TimeSpan timeSpan)
        {
            if(!HasValue)
            {
                timeSpan = TimeSpan.Zero;
                return false;
            }
            timeSpan = new TimeSpan(Value);
            return true;
        }
    }

    partial class DateTimeUtcAsLong
    {
        public DateTime? ToDateTimeUtcNullable()
        {
            if(!HasValue) return null;
            return new DateTime(Value,DateTimeKind.Utc);
        }

        public DateTime? ToDateTimeUtcNullable(TimeSpan timeskew)
        {
            if(!HasValue) return null;
            return new DateTime(Value,DateTimeKind.Utc).Add(timeskew);
        }

        public DateTime ToDateTimeUtc(TimeSpan timeskew)
        {
            if(!HasValue) return DateTime.MinValue;
            return new DateTime(Value,DateTimeKind.Utc).Add(timeskew);
        }
        public DateTime ToDateTimeUtc()
        {
            if (!HasValue) return DateTime.MinValue;
            return new DateTime(Value, DateTimeKind.Utc);
        }
        public bool ToDateTimeUtc(out DateTime dateTimeUtc)
        {
            if(!HasValue)
            {
                dateTimeUtc = DateTime.MinValue;
                return false;
            }
            dateTimeUtc = new DateTime(Value,DateTimeKind.Utc);
            return true;
        }
    }

    public static class ConverterExtensions
    {
        public static GuidAsBytes ToGuidAsBytes(this Guid? guid)
        {
            var retval = new GuidAsBytes();
            if(guid.HasValue)retval.Value = ByteString.CopyFrom(guid.Value.ToByteArray());
            else retval.Value = ByteString.CopyFrom(Guid.Empty.ToByteArray());
            return retval;
        }
        public static GuidAsBytes ToGuidAsBytes(this Guid guid)
        {
            var retval = new GuidAsBytes();
            retval.Value = ByteString.CopyFrom(guid.ToByteArray());
            return retval;
        }

        public static TimeSpanAsLong ToTimeSpanAsLong(this TimeSpan? timeSpan)
        {
            var retval = new TimeSpanAsLong();
            if(timeSpan.HasValue)
            {
                retval.HasValue = true;
                retval.Value = timeSpan.Value.Ticks;
            }

            return retval;
        }

        public static TimeSpanAsLong ToTimeSpanAsLong(this TimeSpan timeSpan)
        {
            var retval = new TimeSpanAsLong {HasValue = true, Value = timeSpan.Ticks};
            return retval;
        }

        public static DateTimeUtcAsLong ToDateTimeUtcAsLong(this DateTime? dateTime)
        {
            var retval = new DateTimeUtcAsLong();
            if(dateTime.HasValue)
            {
                retval.HasValue = true;
                retval.Value = dateTime.Value.ToUniversalTime().Ticks;
            }

            return retval;
        }

        public static DateTimeUtcAsLong ToDateTimeUtcAsLong(this DateTime dateTime)
        {
            var retval = new DateTimeUtcAsLong {HasValue = true, Value = dateTime.ToUniversalTime().Ticks};
            return retval;
        }
    }
}
