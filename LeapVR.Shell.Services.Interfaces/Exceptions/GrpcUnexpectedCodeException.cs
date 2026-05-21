#region Licence
/****************************************************************
 *  Filename: GrpcUnexpectedCodeException.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-7-14
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

namespace LeapVR.Shell.Services.Interfaces.Exceptions
{
    /// <summary>
    /// Thrown when GRPC call is performed with no problems, but it responses with error code indicating no success when success was expected.
    /// </summary>
    public class GrpcUnexpectedCodeException : Exception
    {
        public int Code { get; set; }

        public GrpcUnexpectedCodeException(int code)
        {
            Code = code;
        }

        public GrpcUnexpectedCodeException(string message, int code)
            : base(message)
        {
            Code = code;
        }

        public GrpcUnexpectedCodeException(string message, Exception innerException, int code)
            : base(message, innerException)
        {
            Code = code;
        }
    }
}
