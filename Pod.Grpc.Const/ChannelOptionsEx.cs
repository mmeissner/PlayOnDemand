#region Licence
/****************************************************************
 *  Filename: ChannelOptionsEx.cs
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
namespace Pod.Grpc.Base.Const {
    public static class ChannelOptionsEx
    {
        //public const string KeepAliveTimeMs = "GRPC_ARG_KEEPALIVE_TIME_MS";
        //public const string KeepAliveTimeoutMs = "GRPC_ARG_KEEPALIVE_TIMEOUT_MS";
        //public const string KeepAlivePermitWithoutCalls = "GRPC_ARG_KEEPALIVE_PERMIT_WITHOUT_CALLS";
        //public const string Http2BdpProbe ="GRPC_ARG_HTTP2_BDP_PROBE";
        //public const string Http2MinRecvPingIntervalWithoutDataMs = "GRPC_ARG_HTTP2_MIN_RECV_PING_INTERVAL_WITHOUT_DATA_MS";
        //public const string Http2MinSentPingIntervalWithoutDataMs =
        //        "GRPC_ARG_HTTP2_MIN_SENT_PING_INTERVAL_WITHOUT_DATA_MS";
        //public const string Http2MaxPingsWithoutData = "GRPC_ARG_HTTP2_MAX_PINGS_WITHOUT_DATA";
        public const string KeepAliveTimeMs = "grpc.keepalive_time_ms";
        public const string KeepAliveTimeoutMs = "grpc.keepalive_timeout_ms";
        public const string KeepAlivePermitWithoutCalls = "grpc.keepalive_permit_without_calls";
        public const string Http2BdpProbe = "grpc.http2.bdp_probe";
        public const string Http2MinRecvPingIntervalWithoutDataMs = "grpc.http2.min_ping_interval_without_data_ms";
        public const string Http2MinSentPingIntervalWithoutDataMs =
                "grpc.http2.min_time_between_pings_ms";
        public const string Http2MaxPingsWithoutData = "grpc.http2.max_pings_without_data";
    }
}