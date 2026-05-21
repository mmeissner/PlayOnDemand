#region Licence
/****************************************************************
 *  Filename: IPasswordHasher.cs
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
namespace Pod.Data.Models.Interfaces {
    public interface IPasswordHasher {
        /// <summary>
        /// Returns a hashed representation of the supplied <paramref name="password"/> for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed representation of the supplied <paramref name="password"/> for the specified <paramref name="user"/>.</returns>
        string HashPassword(string password);
        /// <summary>
        /// Returns a <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.
        /// </summary>
        /// <param name="user">The user whose password should be verified.</param>
        /// <param name="hashedPassword">The hash value for a user's stored password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>A <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.</returns>
        /// <remarks>Implementations of this method should be time consistent.</remarks>
        PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword);
    }

    /// <summary>
    /// Specifies the results for password verification.
    /// </summary>
    public enum PasswordVerificationResult
    {
        /// <summary>
        /// Indicates password verification failed.
        /// </summary>
        Failed = 0,

        /// <summary>
        /// Indicates password verification was successful.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Indicates password verification was successful however the password was encoded using a deprecated algorithm
        /// and should be rehashed and updated.
        /// </summary>
        SuccessRehashNeeded = 2
    }
}