#region Licence
/****************************************************************
 *  Filename: BarracksStateModel.cs
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
namespace Steam.Models.DOTA2
{
    public class BarracksStateModel
    {
        public BarracksStateModel()
        {
        }

        public BarracksStateModel(int barracksState)
        {
            IsTopMeleeAlive = ((barracksState >> 0) & 1) == 1 ? true : false;
            IsTopRangedAlive = ((barracksState >> 1) & 1) == 1 ? true : false;
            IsMiddleMeleeAlive = ((barracksState >> 2) & 1) == 1 ? true : false;
            IsMiddleRangedAlive = ((barracksState >> 3) & 1) == 1 ? true : false;
            IsBottomMeleeAlive = ((barracksState >> 4) & 1) == 1 ? true : false;
            IsBottomRangedAlive = ((barracksState >> 5) & 1) == 1 ? true : false;
        }

        public bool IsTopMeleeAlive { get; set; }
        public bool IsTopRangedAlive { get; set; }
        public bool IsMiddleMeleeAlive { get; set; }
        public bool IsMiddleRangedAlive { get; set; }
        public bool IsBottomMeleeAlive { get; set; }
        public bool IsBottomRangedAlive { get; set; }
    }
}