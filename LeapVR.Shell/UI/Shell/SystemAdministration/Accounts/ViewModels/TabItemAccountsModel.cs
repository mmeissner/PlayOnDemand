#region Licence
/****************************************************************
 *  Filename: TabItemAccountsModel.cs
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
using System.Collections.Specialized;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Platform.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;
using LeapVR.Shell.UI.Universal.Platform.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Accounts.ViewModels
{
    public sealed class TabItemAccountsViewModel : TabItemSystemScreen
    {
        #region Fields & Properties
        private bool _showNoAccountSelected;
        private readonly Dictionary<string,PlatformAccountAppsViewModel> _accountAppsViewModels = new Dictionary<string, PlatformAccountAppsViewModel>();
        private PlatformAccountAppsViewModel _platformAccountAppsViewModel;

        public override int DisplayOrder => 11;
        public override string DisplayName
        {
            get { return Resources.System_Accounts; }
            set { /* ignore */ }
        }
        #endregion

        public PlatformAccountsViewModel PlatformAccountSelector { get; }
        public PlatformAccountAppsViewModel PlatformAccountAppsViewModel   
        {
            get => _platformAccountAppsViewModel;
            set
            {
                if(Equals(value, _platformAccountAppsViewModel)) return;
                _platformAccountAppsViewModel = value;
                NotifyOfPropertyChange();
            }
        }
        public bool ShowNoAccountSelected
        {
            get => _showNoAccountSelected;
            set
            {
                if(value == _showNoAccountSelected) return;
                _showNoAccountSelected = value;
                NotifyOfPropertyChange();
            }
        }

        #region Constructors

        public TabItemAccountsViewModel(IUIMessageBroker messageBroker, PlatformAccountsViewModel platformAccountSelectorViewModel) : base(messageBroker,"IconAccounts")
        {
            ShowNoAccountSelected = true;
            PlatformAccountSelector = platformAccountSelectorViewModel;
            PlatformAccountSelector.PropertyChanged += AccountSelector_PropertyChanged;
            PlatformAccountSelector.PlatformAccounts.CollectionChanged += PlatformAccounts_CollectionChanged;
            if(PlatformAccountSelector.SelectedPlatformAccount != null)
            {
                AccountSelected(PlatformAccountSelector.PlatformSelector.SelectedPlatform.Platform,PlatformAccountSelector.SelectedPlatformAccount);
            }
        }

        private void PlatformAccounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach(object oldItem in e.OldItems)
                    {
                        if(oldItem is IPlatformAccount platformAccount)
                        {
                            _accountAppsViewModels.Remove(platformAccount.AccountId);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach(object oldItem in e.OldItems)
                    {
                        if(oldItem is IPlatformAccount platformAccount)
                        {
                            _accountAppsViewModels.Remove(platformAccount.AccountId);
                        }
                    }
                    //Cant Handle new Items as platform is unkown, however they get created on selection
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _accountAppsViewModels.Clear();
                    break;
            }
        }

        protected override void HandleLanguageChange(IUILanguageChangedEvent message)
        {
        }
        #endregion

        #region Methods
        private void AccountSelector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName== nameof(PlatformAccountsViewModel.SelectedPlatformAccount))
            {
                if(PlatformAccountSelector.SelectedPlatformAccount == null)
                {
                    PlatformAccountAppsViewModel = null;
                    ShowNoAccountSelected = true;
                }
                else
                {
                    AccountSelected(PlatformAccountSelector.PlatformSelector.SelectedPlatform.Platform,PlatformAccountSelector.SelectedPlatformAccount);
                }
            }
        }
        private void AccountSelected(IPlatform selectedPlatform, IPlatformAccount selectedAccount)
        {
           
            if (_accountAppsViewModels.ContainsKey(selectedAccount.AccountId))
            {
                PlatformAccountAppsViewModel = _accountAppsViewModels[selectedAccount.AccountId];
            }
            else
            {
                var accountAppsViewModel = new PlatformAccountAppsViewModel(selectedPlatform, selectedAccount, MessageBroker);
                _accountAppsViewModels.Add(selectedAccount.AccountId,accountAppsViewModel);
                PlatformAccountAppsViewModel = accountAppsViewModel;
            }
            ShowNoAccountSelected = false;
        }
        #endregion
    }
}
