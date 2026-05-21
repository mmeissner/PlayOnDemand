#region Licence
/****************************************************************
 *  Filename: AuthConstants.cs
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
namespace Pod.Grpc.Base.Const
{
    /// <summary>
    /// Keys for Values in Headers of Grpc Messages
    /// The Values must be lower case!
    /// Grpc does not preserve upper case chars! 
    /// </summary>
    public static class AuthConstants
    {
        public const string ShellClientIdentityKey = "identity";
        public const string ShellClientPasswordKey = "password";
    }
}