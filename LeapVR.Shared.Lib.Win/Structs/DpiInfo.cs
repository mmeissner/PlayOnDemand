#region Licence
/****************************************************************
 *  Filename: DpiInfo.cs
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

using System.Windows;

namespace LeapVR.Shared.Lib.Win.Structs
{
    public struct DpiInfo
    {
        public Size ScreenSize;
        public double FactorX;
        public double FactorY;
    }
}