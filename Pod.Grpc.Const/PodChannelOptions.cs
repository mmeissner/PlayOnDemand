#region Licence
/****************************************************************
 *  Filename: PodChannelOptions.cs
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
using System.Collections.Generic;
using Grpc.Core;

namespace Pod.Grpc.Base.Const {
    public static class PodChannelOptions
    {
        public static List<ChannelOption> DefaultServerOptions()
        {
            return new List<ChannelOption>()
                   {
                           new ChannelOption(
                                   ChannelOptions.
                                           MaxReceiveMessageLength,
                                   int.MaxValue),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           KeepAliveTimeMs,
                                   10000),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           KeepAliveTimeoutMs,
                                   10000),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           KeepAlivePermitWithoutCalls,
                                   1),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           Http2BdpProbe,
                                   1),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           Http2MinRecvPingIntervalWithoutDataMs,
                                   5000),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           Http2MinSentPingIntervalWithoutDataMs,
                                   10000),
                   };
        }
        public static List<ChannelOption> DefaultClientOptions()
        {
            return new List<ChannelOption>()
                   {
                           new ChannelOption(
                                   ChannelOptions.
                                           MaxReceiveMessageLength,
                                   int.MaxValue),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           KeepAliveTimeMs,
                                   10000),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           KeepAliveTimeoutMs,
                                   10000),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           KeepAlivePermitWithoutCalls,
                                   1),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           Http2BdpProbe,
                                   1),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           Http2MinRecvPingIntervalWithoutDataMs,
                                   5000),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           Http2MinSentPingIntervalWithoutDataMs,
                                   10000),
                           new ChannelOption(
                                   ChannelOptionsEx.
                                           Http2MaxPingsWithoutData,
                                   0)
                   };
        }
    }
}