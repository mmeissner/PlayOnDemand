#region Licence
/****************************************************************
 *  Filename: MockLogger.cs
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Pod.Services.Test.TestFixtures
{
    /// <summary>
    /// Tiny helper that returns a no-op <see cref="ILogger{T}"/> for tests that need to
    /// satisfy a logger constructor parameter but don't care about the logging output.
    /// </summary>
    public static class MockLogger
    {
        /// <summary>
        /// Returns a <see cref="NullLogger{T}.Instance"/>.
        /// </summary>
        public static ILogger<T> For<T>() => NullLogger<T>.Instance;
    }
}
