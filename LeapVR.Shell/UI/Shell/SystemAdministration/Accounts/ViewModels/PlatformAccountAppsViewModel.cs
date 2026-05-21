#region Licence
/****************************************************************
 *  Filename: PlatformAccountAppsViewModel.cs
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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.ViewModels;

namespace LeapVR.Shell.UI.Shell.SystemAdministration.Accounts.ViewModels
{
    public class PlatformAccountAppsViewModel : Screen, IHandle<IUIPlatformAccountChanged>
    {
        private readonly IPlatform _platform;
        private readonly IPlatformAccount _platformAccount;
        private readonly IUIMessageBroker _uiMessageBroker;

        public Guid PlatformId => _platform.PlatformGuid;
        private ListViewModel<PlatformAppViewModel> _assignedApplications;
        private ListViewModel<PlatformAppViewModel> _unlicensedApplications;

        private PlatformAppViewModel _lastAssignedSelected;
        private PlatformAppViewModel _lastUnlicensedSelected;

        public ListViewModel<PlatformAppViewModel> AssignedApplications
        {
            get => _assignedApplications;
            set
            {
                if(Equals(value, _assignedApplications)) return;
                _assignedApplications = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SelectedAssignedApplication));
            }
        }
        public ListViewModel<PlatformAppViewModel> UnlicensedApplications
        {
            get => _unlicensedApplications;
            set
            {
                if(Equals(value, _unlicensedApplications)) return;
                _unlicensedApplications = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SelectedUnlicensedApplication));
            }
        }
        public PlatformAppViewModel SelectedAssignedApplication => AssignedApplications.SelectedItem;
        public PlatformAppViewModel SelectedUnlicensedApplication => UnlicensedApplications.SelectedItem;
        public bool CanAddLicense => _lastUnlicensedSelected?.ApplicationId != null && !_lastUnlicensedSelected.HasPlatformAccount();
        public PlatformAccountAppsViewModel(IPlatform platform,IPlatformAccount platformAccount,IUIMessageBroker uiMessageBroker)
        {
            _platform = platform;
            _platformAccount = platformAccount;
            _uiMessageBroker = uiMessageBroker;
            _uiMessageBroker.Subscribe(this);
            AssignedApplications =  new ListViewModel<PlatformAppViewModel>();
            UnlicensedApplications = new ListViewModel<PlatformAppViewModel>();
            UnlicensedApplications.PropertyChanged += UnlicensedApplicationPropertyChanged;
            AssignedApplications.PropertyChanged += AssignedApplicationPropertyChanged;
           
            AssignedApplications.ShowLoading = false;
            UnlicensedApplications.ShowLoading = false;
            platform.GetPlatformApps(PlatformAppReceivedCallback, PlatformAppReceivedCompleted,SynchronizationContext.Current );
        }
        public void AddLicense()
        {
            UnlicensedApplications.SelectedItem.AddLicense(_platformAccount);
        }

        public bool CanRemoveLicense =>_lastAssignedSelected?.ApplicationId != null &&
                _platformAccount.LicensedAppIds().Contains(_lastAssignedSelected.ApplicationId);
        public void RemoveLicense()
        {
            AssignedApplications.SelectedItem.RemoveLicense(_platformAccount);
        }

        private void PlatformAppReceivedCompleted() {  }

        private void PlatformAppReceivedCallback(IAppPlatformInfo obj)
        {

            if(!obj.IsLicenseRequired)return;

            var licenseInfo = obj.LicenseInfo();
            if(licenseInfo.CurrentLicenseCount == 0)
            {
                UnlicensedApplications.Items.Add(new PlatformAppViewModel(obj));
            }
            else
            {
                if(licenseInfo.Accounts.Any(x => x.AccountId.Equals(_platformAccount.AccountId)))
                {
                    AssignedApplications.Items.Add(new PlatformAppViewModel(obj));
                }
                //If we would like to allow multiple assignments of same license to diffrent accounts,
                //then we would need to handle it here
            }
        }

        private void AssignedApplicationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(ListViewModel<PlatformAppViewModel>.SelectedItem)))
            {

                var selectedItem = AssignedApplications.SelectedItem;
                if(_lastAssignedSelected != null)
                {
                    _lastAssignedSelected.PropertyChanged -= LastAssignedSelectedOnPropertyChanged; 
                }

                if(selectedItem != null)
                {
                    _lastAssignedSelected = selectedItem;
                    _lastAssignedSelected.PropertyChanged += LastAssignedSelectedOnPropertyChanged;
                }
                NotifyOfPropertyChange(nameof(SelectedAssignedApplication));
                NotifyOfPropertyChange(nameof(CanRemoveLicense));
            }
        }

        private void LastAssignedSelectedOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(PlatformAppViewModel.LicenseState)))
            {
                NotifyOfPropertyChange(nameof(CanRemoveLicense));
            }
        }

        private void UnlicensedApplicationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(ListViewModel<PlatformAppViewModel>.SelectedItem)))
            {
                var selectedItem = UnlicensedApplications.SelectedItem;
                if(_lastUnlicensedSelected != null)
                {
                    _lastUnlicensedSelected.PropertyChanged -= LastUnlicensedSelectedOnPropertyChanged; 
                }

                if(selectedItem != null)
                {
                    _lastUnlicensedSelected = selectedItem;
                    _lastUnlicensedSelected.PropertyChanged += LastUnlicensedSelectedOnPropertyChanged;
                }
                NotifyOfPropertyChange(nameof(SelectedUnlicensedApplication));
                NotifyOfPropertyChange(nameof(CanAddLicense));
            }
        }

        private void LastUnlicensedSelectedOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals(nameof(PlatformAppViewModel.LicenseState)))
            {
                NotifyOfPropertyChange(nameof(CanAddLicense));
            }
        }

        public void Handle(IUIPlatformAccountChanged message)
        {
            //If we would like to allow multiple assignments of same license to diffrent accounts,
            //then we would need to handle it here
            if(!message.PlatformId.Equals(PlatformId))return;
            Guid appId;
            switch(message.Type)
            {
                case AccountEventType.AddApps:
                    if(!message.ApplicationId.HasValue) break;
                    appId = message.ApplicationId.Value;
                    var appReceivedAccount =_unlicensedApplications.Items.FirstOrDefault(x=> x.ApplicationId.Equals(appId));
                    if(appReceivedAccount != null)
                    {
                        _unlicensedApplications.Items.Remove(appReceivedAccount);
                    }
                    //If this is the account it was acced to then we move it to the assigned apps
                    if(message.AccountId.Equals(_platformAccount.AccountId))
                    {
                        _assignedApplications.Items.Add(appReceivedAccount);
                    }
                    break;
                case AccountEventType.RemoveApps:
                    if(!message.ApplicationId.HasValue) break;
                    appId = message.ApplicationId.Value;
                    if(_platformAccount.AccountId.Equals(message.AccountId))
                    {
                        var appToRemoved =_assignedApplications.Items.FirstOrDefault(x=> x.ApplicationId.Equals(appId));
                        if(appToRemoved != null) _assignedApplications.Items.Remove(appToRemoved);
                    }
                    //Not Account but Platform specific, if an app was removed from one account is becomes availible to all 
                    _unlicensedApplications.Items.Add(new PlatformAppViewModel(_platform.GetPlatformApp(appId)));
                    break;
            }
        }
    }
}
