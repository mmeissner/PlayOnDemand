#region Licence
/****************************************************************
 *  Filename: PlatformAccountsViewModel.cs
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
using System.Threading.Tasks;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.Dialog;

namespace LeapVR.Shell.UI.Universal.Platform.ViewModels
{
    // REWRITE TO WORK WITH PLATFORMSELECTOR
    public class PlatformAccountsViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly BindableCollection<IPlatformAccount> _platformAccounts = new BindableCollection<IPlatformAccount>();
        private bool _isLoading = false;
        private IPlatformAccount _selectedPlatformAccount;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if(value == _isLoading) return;
                _isLoading = value;
                NotifyOfPropertyChange();
            }
        }
        public PlatformAccountsViewModel(IWindowManager windowManager, ViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
            _windowManager = windowManager;
            PlatformSelector = viewModelFactory.BuildPlatformSelector();
            PlatformSelector.EnableSelect = true;
            PlatformSelector.PropertyChanged += PlatformSelector_PropertyChanged;
            if(PlatformSelector.SelectedPlatform != null) PlatformSelected(PlatformSelector.SelectedPlatform.Platform);
        }

        public PlatformSelectorViewModel PlatformSelector { get;set; }
        public IObservableCollection<IPlatformAccount> PlatformAccounts => _platformAccounts;
        public IPlatformAccount SelectedPlatformAccount
        {
            get => _selectedPlatformAccount;
            set
            {
                if(Equals(value, _selectedPlatformAccount)) return;
                _selectedPlatformAccount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CanAddAcount));
                NotifyOfPropertyChange(nameof(CanDeleteAccount));
            }
        }

        public bool CanAddAcount => PlatformSelector.SelectedPlatform != null;
        public void AddAccount()
        {
            var createAccountViewModel = new CreatePlatformAccountViewModel(PlatformSelector.SelectedPlatform.Platform.PlatformGuid);
            var retval = _windowManager.ShowDialog(createAccountViewModel,null,ShellClientHelper.GetUniversalDialogSettings());
            if(retval.HasValue && retval.Value)
            {
                if(PlatformSelector.SelectedPlatform.Platform.CreatePlatformAccount(createAccountViewModel.Username,createAccountViewModel.Password,createAccountViewModel.PlatformId, out var newPlatformAccount))
                {
                    _platformAccounts.Add(newPlatformAccount);
                    SelectedPlatformAccount = newPlatformAccount;
                }
            }
        }

        public bool CanDeleteAccount => SelectedPlatformAccount != null;
        public void DeleteAccount()
        {
            var accountToDelete = SelectedPlatformAccount;
            var userWantDeleteaccount = _windowManager.ShowDialog(
                    _viewModelFactory.Build(DialogType.AttemptToDeletePlatformAccount), null, ShellClientHelper.GetUniversalDialogSettings());
            if (userWantDeleteaccount == true)
            {
                PlatformSelector.SelectedPlatform.Platform.DeletePlatformAccount(accountToDelete);
                _platformAccounts.Remove(accountToDelete);
                SelectedPlatformAccount= _platformAccounts.FirstOrDefault();
            }
        }

        private void PlatformSelector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName== nameof(PlatformSelectorViewModel.SelectedPlatform))
            {
                SelectedPlatformAccount = null;
                PlatformAccounts.Clear();
                if(PlatformSelector.SelectedPlatform != null)
                {
                    PlatformSelected(PlatformSelector.SelectedPlatform.Platform);
                }
            }
        }

        private void PlatformSelected(IPlatform selectedPlatform)
        {
            AddOrUpdatePlatformAccouts(selectedPlatform.GetPlatformAccounts());
            SelectedPlatformAccount = _platformAccounts.FirstOrDefault();
        }

        private void AddOrUpdatePlatformAccouts(IEnumerable<IPlatformAccount> platformAccounts)
        {
            foreach(IPlatformAccount account in platformAccounts)
            {
                if(!_platformAccounts.Contains(account))_platformAccounts.Add(account);
            }
        }
    }
}
