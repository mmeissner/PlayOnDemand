#region Licence
/****************************************************************
 *  Filename: PasswordHasher.cs
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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Pod.Data.Models;
using Pod.Data.Models.Interfaces;

namespace Pod.Data {

    /// <summary>
    /// Provides Hashing functionality for strings/passwords
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private readonly int _iterCount = 10000 ;
        private readonly RandomNumberGenerator _rng;
        /* =======================
         * HASHED PASSWORD FORMATS
         * =======================
         * 
         * Version 2:
         * PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
         * (See also: SDL crypto guidelines v5.1, Part III)
         * Format: { 0x00, salt, subkey }
         *
         * Version 3:
         * PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.
         * Format: { 0x01, prf (UInt32), iter count (UInt32), salt length (UInt32), salt, subkey }
         * (All UInt32s are stored big-endian.)
         */

        /// <summary>
        /// Creates a new instance of <see cref="PasswordHasher"/>.
        /// </summary>
        public PasswordHasher()
        {
            _rng = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Returns a hashed representation of the supplied <paramref name="password"/> for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>A hashed representation of the supplied <paramref name="password"/> for the specified <paramref name="user"/>.</returns>
        public string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            return Convert.ToBase64String(HashPasswordV3(password, _rng));
        }
        /// <summary>
        /// Returns a <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.
        /// </summary>
        /// <param name="user">The user whose password should be verified.</param>
        /// <param name="hashedPassword">The hash value for a user's stored password.</param>
        /// <param name="providedPassword">The password supplied for comparison.</param>
        /// <returns>A <see cref="PasswordVerificationResult"/> indicating the result of a password hash comparison.</returns>
        /// <remarks>Implementations of this method should be time consistent.</remarks>
        public  PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            if (providedPassword == null)
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }

            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            // read the format marker from the hashed password
            if (decodedHashedPassword.Length == 0)
            {
                return PasswordVerificationResult.Failed;
            }
            switch (decodedHashedPassword[0])
            {
                case 0x00:
                    throw new NotImplementedException("Password verification for V2 not implemented");

                case 0x01:
                    int embeddedIterCount;
                    if (VerifyHashedPasswordV3(decodedHashedPassword, providedPassword, out embeddedIterCount))
                    {
                        // If this hasher was configured with a higher iteration count, change the entry now.
                        return (embeddedIterCount < _iterCount)
                                ? PasswordVerificationResult.SuccessRehashNeeded
                                : PasswordVerificationResult.Success;
                    }
                    else
                    {
                        return PasswordVerificationResult.Failed;
                    }

                default:
                    return PasswordVerificationResult.Failed; // unknown format marker
            }
        }
        private byte[] HashPasswordV3(string password, RandomNumberGenerator rng)
        {
            return HashPasswordV3(password, rng,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterCount: _iterCount,
                    saltSize: 128 / 8,
                    numBytesRequested: 256 / 8);
        }

        private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
        {
            // Produce a version 3 (see comment above) text hash.
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // format marker
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
            return outputBytes;
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24)
                   | ((uint)(buffer[offset + 1]) << 16)
                   | ((uint)(buffer[offset + 2]) << 8)
                   | ((uint)(buffer[offset + 3]));
        }

        private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterCount)
        {
            iterCount = default(int);

            try
            {
                // Read header information
                KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

                // Read the salt: must be >= 128 bits
                if (saltLength < 128 / 8)
                {
                    return false;
                }
                byte[] salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

                // Read the subkey (the rest of the payload): must be >= 128 bits
                int subkeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subkeyLength < 128 / 8)
                {
                    return false;
                }
                byte[] expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // Hash the incoming password and verify it
                byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subkeyLength);
                return ByteArraysEqual(actualSubkey, expectedSubkey);
            }
            catch
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                return false;
            }
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        // Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

    }
}