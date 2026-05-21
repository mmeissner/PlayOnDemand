#region Licence
/****************************************************************
 *  Filename: EconItemAttributeModel.cs
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
namespace Steam.Models.GameEconomy
{
    public class EconItemAttributeModel
    {
        public int DefIndex { get; set; }
        public object Value { get; set; }

        public double FloatValue { get; set; }

        public EconItemAttributeAccountInfoModel AccountInfo { get; set; }
    }
}