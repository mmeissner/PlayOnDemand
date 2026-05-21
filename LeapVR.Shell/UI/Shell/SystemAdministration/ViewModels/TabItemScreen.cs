#region Licence
/****************************************************************
 *  Filename: TabItemScreen.cs
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
    /// Basic implementation of <see cref="ITabItemScreen"/>. This is an abstract class.
    /// </summary>
    public abstract class TabItemScreen : Screen, ITabItemScreen,IHandle<IUILanguageChangedEvent>, IDisposable
    {
        protected readonly IUIMessageBroker MessageBroker;
        protected TabItemScreen(IUIMessageBroker messageBroker,string iconKey)
        {
            IconKey = iconKey;
            MessageBroker = messageBroker;
            MessageBroker.Subscribe(this);
        }

        public virtual int DisplayOrder { get; set; }

        public string IconKey { get; }

        public int CompareTo(object other)
        {
            return Extensions.CompareTo(this, other);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                MessageBroker.Unsubscribe(this);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Handle(IUILanguageChangedEvent message)
        {
            NotifyOfPropertyChange(nameof(DisplayName));
            HandleLanguageChange(message);
        }

        protected abstract void HandleLanguageChange(IUILanguageChangedEvent message);
    }
}
