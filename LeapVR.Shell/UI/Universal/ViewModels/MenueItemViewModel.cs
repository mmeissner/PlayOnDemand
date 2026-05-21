#region Licence
/****************************************************************
 *  Filename: MenueItemViewModel.cs
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using NLog;
using Action = System.Action;

namespace LeapVR.Shell.UI.Universal.ViewModels
{
    /// <summary>
    /// Item for a Navigational Control that can be operated with Gamepad and is translateable
    /// The Item can have one Action that is executed by click or gamepad activation and two diffrent states with two images
    /// for beeing selected or in default state
    /// </summary>
    /// <seealso cref="Caliburn.Micro.Screen" />
    /// <seealso cref="IUILanguageChangedEvent" />
    /// <seealso cref="System.IDisposable" />
    public class MenueItemViewModel :Screen, IHandle<IUILanguageChangedEvent>, IDisposable
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Func<string> _getTranslatedName;
        private readonly IUIMessageBroker _messageBroker;
        private readonly Action _menueItemAction;
        private readonly ImageSource _defaultImage;
        private readonly ImageSource _selectedImage;
        private string _text;
        private bool _isEnabled = true;
        private bool _isBusy;

        public MenueItemViewModel(IUIMessageBroker messageBroker,Func<string> getTranslatedName,Action menueItemAction,ImageSource selectedImage, ImageSource defaultImage)
        {
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            _getTranslatedName = getTranslatedName;
            _menueItemAction = menueItemAction;
            _defaultImage = defaultImage;
            _selectedImage = selectedImage;
            Description = getTranslatedName.Invoke();
        }

        public ImageSource DefaultImage => _defaultImage;
        public ImageSource SelectedImage => _selectedImage;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if(value == _isBusy) return;
                _isBusy = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsEnabled   
        {
            get => _isEnabled;
            set
            {
                if(value == _isEnabled) return;
                _isEnabled = value;
                NotifyOfPropertyChange();
            }
        }
        public string Description
        {
            get => _text;
            set
            {
                if(value == _text) return;
                _text = value;
                NotifyOfPropertyChange();
            }
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            NotifyOfPropertyChange(() => IsActive);
        }
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            NotifyOfPropertyChange(() => IsActive);
        }
        public void MenueAction()
        {
            _menueItemAction?.Invoke();
        }
        public void Handle(IUILanguageChangedEvent message) { Description = _getTranslatedName.Invoke(); }
        public void Dispose()
        {
            _messageBroker?.Unsubscribe(this);
        }
    }
}
