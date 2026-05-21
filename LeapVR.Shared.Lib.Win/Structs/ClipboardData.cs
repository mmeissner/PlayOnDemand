#region Licence
/****************************************************************
 *  Filename: ClipboardData.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-15
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Drawing;

namespace LeapVR.Shared.Lib.Win.Structs
{
    public static class ClipboardData
    {
        private static long _timestampLastChange;
        private static volatile Bitmap _clipboardImage;
        private static object _updateLock = new object();

        public static void UpdateClipboardImage(Bitmap image)
        {
            lock (_updateLock)
            {
                _clipboardImage = image;
                _timestampLastChange = DateTime.Now.Ticks;
            }
        }

        public static long GetClipboardBitmap(out Bitmap clipboardBitmap)
        {
            lock (_updateLock)
            {
                clipboardBitmap = _clipboardImage ;
                return _timestampLastChange;
            }
        }
    }
}
