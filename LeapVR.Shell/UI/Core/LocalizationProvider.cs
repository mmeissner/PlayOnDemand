#region Licence
/****************************************************************
 *  Filename: LocalizationProvider.cs
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
using WPFLocalizeExtension.Extensions;

namespace LeapVR.Shell.UI.Core
{
    public static class LocalizationProvider
    {
        /// <summary>
        /// Get localized value by specific key
        /// </summary>
        /// <typeparam name="T">type parameter to specify of which type the value should be.</typeparam>
        /// <param name="key">key for the value of type <typeparamref name="T"/></param>
        /// <returns>value of type <typeparamref name="T"/></returns>
        /// <exception cref="ArgumentNullException">throw when the key is null.</exception>
        /// <exception cref="KeyNotFoundException">throw when the key can not be found.</exception>
        public static T GetLocalizedValue<T>(string key)
        {
            return LocExtension.GetLocalizedValue<T>(key);
        }
    }
}
