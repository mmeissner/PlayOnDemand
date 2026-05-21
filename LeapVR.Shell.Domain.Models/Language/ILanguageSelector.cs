#region Licence
/****************************************************************
 *  Filename: ILanguageSelector.cs
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeapVR.Shell.Domain.Models.Language
{
    public interface ILanguageSelector
    {
        CultureInfo CurrentCulture { get; }
        CultureInfo DefaultCulture { get; }
        CultureInfo[] SupportedCultures { get; }

        void ActivateCultureInfo(CultureInfo newCulture);
        void ActivateCultureInfo(string cultureShortName);
        void ActivateDefaultCultureInfo();
        bool SetDefaultCulture(CultureInfo culture);
    }
}
