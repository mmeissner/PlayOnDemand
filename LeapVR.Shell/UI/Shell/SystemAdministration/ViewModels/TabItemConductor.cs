#region Licence
/****************************************************************
 *  Filename: TabItemConductor.cs
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
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels
{
    /// <summary>
    /// Representing a view model that contains child screens implementing <see cref="ITabItemScreen"/> interfaces. This is an abstract class.
    /// </summary>
    /// <typeparam name="T">should be a class and derived from <see cref="IScreen"/></typeparam>
    public abstract class TabItemConductor<T> : Conductor<T>.Collection.OneActive,IHandle<IUILanguageChangedEvent>, ITabItemScreen, IDisposable where T : class, IScreen
    {
        private readonly IUIMessageBroker _messageBroker;
        protected TabItemConductor(IUIMessageBroker messageBroker,string iconKey)
        {
            IconKey = iconKey;
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
        }

        public virtual int DisplayOrder => 0;

        public string IconKey { get; }

        public int CompareTo(object other)
        {
            return Extensions.CompareTo(this, other);
        }

        public void Handle(IUILanguageChangedEvent message)
        {
            NotifyOfPropertyChange(nameof(DisplayName));
        }
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                _messageBroker.Unsubscribe(this);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
