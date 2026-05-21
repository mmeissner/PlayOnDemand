#region Licence
/****************************************************************
 *  Filename: VrConstants.cs
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

namespace LeapVR.Shell.Modules.Vr {
    public static class VrConstants
    {
        private static Guid OpenVrModuleGuid => Guid.Parse("c974d901-21ad-484f-958b-aa14ed6bcd9f");
        private static Guid OculusVrModuleGuid => Guid.Parse("5756C9A9-35E1-4CEF-ADEA-2408230CE354");

        public static Guid OpenVrModuleId => OpenVrModuleGuid;
        public const string OpenVrModuleName = "OpenVR";

        public static Guid OculusVrModuleId => OculusVrModuleGuid;
        public const string OculusVrModuleName = "OculusVR";
    }
}