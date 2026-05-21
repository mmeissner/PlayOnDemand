#region Licence
/****************************************************************
 *  Filename: DecimalCalculator.cs
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
namespace LeapVR.Shared.Lib.Classes.GenericCalculator
{
    public class DecimalCalculator : ICalculator<decimal>
    {
        public decimal Zero => 0m;

        public decimal Add(decimal a, decimal b) => a + b;
    }
}
