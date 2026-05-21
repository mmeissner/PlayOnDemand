#region Licence
/****************************************************************
 *  Filename: AllowedTypesRule.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeapVR.Shared.Lib.Helper;

namespace LeapVR.Shared.Lib.Classes.SanityRules
{
    public class AllowedTypesRule<T> : SanityRule<T>
    {
        private readonly TypeInfo[] _allowedTypes;

        public AllowedTypesRule(IEnumerable<Type> allowedTypes)
        {
            QuickLeap.AssertNotNullEx(allowedTypes);
            _allowedTypes = allowedTypes.Select(q => q.GetTypeInfo()).ToArray();
        }

        public override string RuleName => nameof(AllowedTypesRule<T>);
        public override bool CheckSanity(T value)
        {
            QuickLeap.AssertNotNull(value);

            return _allowedTypes.Any(allowedType => allowedType.IsAssignableFrom(value.GetType().GetTypeInfo()));
        }
    }
}
