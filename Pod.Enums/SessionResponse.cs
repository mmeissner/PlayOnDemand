#region Licence
/****************************************************************
 *  Filename: SessionResponse.cs
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
namespace Pod.Enums {
    public enum SessionResponse
    {
        Undefined = 0,
        Success = 10,
        StateMismatch = 20,
        ConnectionMismatch = 30,
        Timeout =40,
    }
}