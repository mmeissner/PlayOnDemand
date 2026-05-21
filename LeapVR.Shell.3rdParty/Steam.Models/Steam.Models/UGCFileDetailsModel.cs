#region Licence
/****************************************************************
 *  Filename: UGCFileDetailsModel.cs
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
namespace Steam.Models
{
    public class UGCFileDetailsModel
    {
        public string FileName { get; set; }
        public string URL { get; set; }
        public int Size { get; set; }
    }
}