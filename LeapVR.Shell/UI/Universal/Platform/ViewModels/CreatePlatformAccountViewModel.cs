#region Licence
/****************************************************************
 *  Filename: CreatePlatformAccountViewModel.cs
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
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.UI.Universal.Platform.Views;

namespace LeapVR.Shell.UI.Universal.Platform.ViewModels
{
    public class CreatePlatformAccountViewModel : Screen
    {
        private string _username;
        private string _password;
        private PasswordBox _passwordBox;
        public CreatePlatformAccountViewModel(Guid platformId) { PlatformId = platformId; }
        public Guid PlatformId { get; private set; }
        public string Username
        {
            get => _username;
            set
            {
                if(value == _username) return;
                _username = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanCreate));
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                if(value == _password) return;
                _password = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanCreate));
            }
        }

        public bool CanCreate => !String.IsNullOrWhiteSpace(Password) && !String.IsNullOrWhiteSpace(Username);
        public void Create()
        {
            TryClose(true);
        }
        public void Cancel()
        {
            TryClose(false);
        }

        protected override void OnViewAttached(object view, object context)
        {
            _passwordBox = ((CreatePlatformAccountView)view).PasswordBox;
            _passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            base.OnViewAttached(view, context);

        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            Password = _passwordBox.Password;
        }
    }
}
