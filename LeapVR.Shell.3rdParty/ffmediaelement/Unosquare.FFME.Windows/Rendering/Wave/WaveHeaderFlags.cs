#region Licence
/****************************************************************
 *  Filename: WaveHeaderFlags.cs
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
namespace Unosquare.FFME.Rendering.Wave
{
    using System;

    /// <summary>
    /// Wave Header Flags enumeration
    /// </summary>
    [Flags]
    internal enum WaveHeaderFlags
    {
        /// <summary>
        /// WHDR_BEGINLOOP
        /// This buffer is the first buffer in a loop.  This flag is used only with output buffers.
        /// </summary>
        BeginLoop = 0x00000004,

        /// <summary>
        /// WHDR_DONE
        /// Set by the device driver to indicate that it is finished with the buffer and is returning it to the application.
        /// </summary>
        Done = 0x00000001,

        /// <summary>
        /// WHDR_ENDLOOP
        /// This buffer is the last buffer in a loop.  This flag is used only with output buffers.
        /// </summary>
        EndLoop = 0x00000008,

        /// <summary>
        /// WHDR_INQUEUE
        /// Set by Windows to indicate that the buffer is queued for playback.
        /// </summary>
        InQueue = 0x00000010,

        /// <summary>
        /// WHDR_PREPARED
        /// Set by Windows to indicate that the buffer has been prepared with the waveInPrepareHeader or waveOutPrepareHeader function.
        /// </summary>
        Prepared = 0x00000002
    }
}
