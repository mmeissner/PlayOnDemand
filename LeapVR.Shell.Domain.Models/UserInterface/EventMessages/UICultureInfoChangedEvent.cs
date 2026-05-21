#region Licence
/****************************************************************
 *  Filename: UICultureInfoChangedEvent.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-7
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.Globalization;

namespace LeapVR.Shell.Domain.Models.UserInterface.EventMessages
{
    public interface IUILanguageChangedEvent
    {
        CultureInfo NewCultureInfo { get; }
    }

    public class UILanguageChangedEvent : IUILanguageChangedEvent
    {
        #region Fields & Properties
        public CultureInfo NewCultureInfo { get; }

        #endregion

        #region Constructors

        public UILanguageChangedEvent(CultureInfo cultureInfo)
        {
            NewCultureInfo = cultureInfo;
        }
        #endregion

        #region Methods

        #endregion
    }
}
