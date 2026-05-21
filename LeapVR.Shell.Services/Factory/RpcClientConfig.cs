#region Licence
/****************************************************************
 *  Filename: RpcClientConfig.cs
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
using LeapVR.Shell.Domain.Models.Customization;

namespace LeapVR.Shell.Services.Factory
{

    public class RpcClientConfig : ConfigObject
    {
        public uint GrpcCallTimeout { get; set; } = 5000;
    }
}
