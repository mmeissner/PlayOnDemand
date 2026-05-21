#region Licence
/****************************************************************
 *  Filename: SanityCheck.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2017-8-2
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
using System.Collections.Generic;
using System.Linq.Expressions;
using LeapVR.Shared.Lib.Classes.SanityRules;
using LeapVR.Shared.Lib.Interfaces;
using LeapVR.Shared.Lib.Helper;

namespace LeapVR.Shared.Lib.Classes
{
    public class SanityCheck<T> : ISanityCheck
    {
        private Expression<Func<T>> _value;
        public Expression<Func<T>> Value
        {
            get => _value;
            set
            {
                _value = value;
                ValueName = QuickLeap.GetFieldPropertyName(value);
                FetchedValue = value.Compile().Invoke();
            }
        }

        public IEnumerable<SanityRule<T>> Rules { get; set; }

        public string ValueName { get; private set; }
        public T FetchedValue { get; private set; }

        public SanityCheck()
        {
            
        }

        public SanityCheck(Expression<Func<T>> value, IEnumerable<SanityRule<T>> rules)
        {
            Value = value;
            Rules = rules;
        }

        public bool Check(out string errorMessage)
        {
            foreach (var rule in Rules)
            {
                if (!rule.CheckSanity(FetchedValue))
                {
                    errorMessage = $"Sanity check of field `{ValueName}` with value `{FetchedValue}` failed on rule `{rule.RuleName}`.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }
    }
}
