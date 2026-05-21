#region Licence
/****************************************************************
 *  Filename: UnitTest_PasswordHasher.cs
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
using FluentAssertions;
using Pod.Data;
using Pod.Data.Models.Interfaces;
using Xunit;

namespace Pod.Data.Test
{
    public class UnitTest_PasswordHasher
    {
        // ---- Original characterization tests (PBKDF2 round-trip) ----------

        [Fact]
        public void HashPassword_RoundTrip_Succeeds()
        {
            const string testPwd = "Password123-3425FDc";
            const string wrongPwd = "Password123-3425FD";

            var hashedPassword = new PasswordHasher().HashPassword(testPwd);

            new PasswordHasher().VerifyHashedPassword(hashedPassword, testPwd)
                                .Should().Be(PasswordVerificationResult.Success);
            new PasswordHasher().VerifyHashedPassword(hashedPassword, wrongPwd)
                                .Should().Be(PasswordVerificationResult.Failed);
        }

        // ---- Added characterization tests for migration safety ------------

        [Fact]
        public void HashPassword_TwoIdenticalPasswords_ProduceDifferentHashes()
        {
            // PBKDF2 with random per-password salt -> distinct hashes for the same plaintext.
            var hasher = new PasswordHasher();
            var h1 = hasher.HashPassword("hello");
            var h2 = hasher.HashPassword("hello");
            h1.Should().NotBe(h2);
        }

        [Fact]
        public void Verify_EmptyString_TreatedAsLegitimateInput()
        {
            // Empty string is a valid (if poor) password; should round-trip just like any other.
            var hasher = new PasswordHasher();
            var hash = hasher.HashPassword(string.Empty);
            hasher.VerifyHashedPassword(hash, string.Empty)
                  .Should().Be(PasswordVerificationResult.Success);
            hasher.VerifyHashedPassword(hash, "x")
                  .Should().Be(PasswordVerificationResult.Failed);
        }

        [Fact]
        public void Verify_GarbageHash_BehaviorDocumented()
        {
            // Characterization: PasswordHasher.VerifyHashedPassword on a malformed (non-base64)
            // input THROWS rather than returning Failed. This is the production behaviour —
            // callers must validate format upstream, or wrap in try/catch.
            var hasher = new PasswordHasher();
            var act = () => hasher.VerifyHashedPassword("not-a-valid-base64-hash", "anything");
            act.Should().Throw<System.FormatException>();
        }

        [Fact]
        public void HashPassword_HashStartsWith_FormatMarkerByte_0x01()
        {
            // V3 format marker (PBKDF2-HMAC-SHA256) is byte 0x01 at offset 0 of the decoded blob.
            // The hash is base64-encoded; first byte decoded should be 0x01.
            var hasher = new PasswordHasher();
            var encoded = hasher.HashPassword("abc");
            var bytes = System.Convert.FromBase64String(encoded);
            bytes[0].Should().Be(0x01, "format marker for V3 PBKDF2-HMAC-SHA256");
        }
    }

}
