#region Licence
/****************************************************************
 *  Filename: GrpcConnectionException.cs
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
    /// Thrown when GRPC call fails at network level (e.g. timeout, unreachable, etc.)
    /// </summary>
    public class GrpcConnectionException : Exception
    {
        public GrpcConnectionException()
        { }

        public GrpcConnectionException(string message)
            : base(message)
        { }

        public GrpcConnectionException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
