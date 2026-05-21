#region Licence
/****************************************************************
 *  Filename: ConstantsChallengeRoute.cs
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
namespace Pod.LetsEncrypt.Config
{
    /// <summary>
    /// Keeps Lets encrypt Constants
    /// </summary>
    public static class LetsEncryptConst
    {
        /// <summary>
        /// Lets Encrypt HTTP Challenge Path
        /// </summary>
        public static string ChallengePath => "/.well-known/acme-challenge";
    }
}
