#region Licence
/****************************************************************
 *  Filename: NotificationKeyValuePair.cs
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
using Caliburn.Micro;

namespace LeapVR.Shell.UI.Core
{
    public class NotificationKeyValuePair<TKey, TValue> : PropertyChangedBase
    {
        private TKey _key;
        public TKey Key
        {
            get { return _key; }
            set
            {
                _key = value;
                NotifyOfPropertyChange();
            }
        }
        private TValue _value;
        public TValue Value
        {
            get { return _value; }
            set
            {
                _value = value;
                NotifyOfPropertyChange();
            }
        }
        public NotificationKeyValuePair(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }
    }

}
