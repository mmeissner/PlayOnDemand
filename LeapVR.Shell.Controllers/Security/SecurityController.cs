#region Licence
/****************************************************************
 *  Filename: SecurityController.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Reactive.Subjects;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Domain.Models.Billing;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;

namespace LeapVR.Shell.Controllers.Security
{

    public class SecurityController : ISecurityController, IHandle<IUISessionSetupChangedEvent>
    {
        #region Fields & Properties

        private readonly IConfigFileRepository<SecurityConfig> _configFileRepository;
        private readonly SecurityConfig _config;
        private readonly IUIMessageBroker _messageBroker;


        public bool IsSecurityEnabled { get; private set; }
        public TimeSpan SystemInactivityTimeout { get; }

        #endregion Fields & Properties

        #region Contructors

        public SecurityController(IConfigFileRepository<SecurityConfig> configFileRepository,IUIMessageBroker messageBroker)
        {
            QuickLeap.AssertNotNull(configFileRepository, messageBroker);
            _configFileRepository = configFileRepository;
            _config = _configFileRepository.Get();
            _messageBroker = messageBroker;
            _messageBroker.Subscribe(this);
            SystemInactivityTimeout = _config.SystemInactivityTimeout;
        }

        #endregion Contructors

        #region Methods

        public void SetSecurityIsEnabled(bool isSecurityEnabled)
        {
            if(isSecurityEnabled== IsSecurityEnabled)return;
            IsSecurityEnabled = isSecurityEnabled;
            OnSecurityChange(isSecurityEnabled);
        }

        public void SetSecurityCode(string code)
        {
            _config.SecurityCode = code;
            _configFileRepository.Store(_config);
        }

        public bool UnlockAdminAccess(string code)
        {
            var verifyResult = code.Equals(_config.SecurityCode);
            if(verifyResult) _messageBroker.Publish(new UIAdminAccessApprovedEvent());
            return verifyResult;
        }
        public event SecurityChanged WhenSecurityChanged;
        #endregion Methods

        public void Handle(IUISessionSetupChangedEvent message)
        {
            switch(message.Settings)
            {
                case AnonymousLoginSettingsBase _:
                    SetSecurityIsEnabled(false);
                    break;
                case QrCodeLoginSettingsBase _:
                    SetSecurityIsEnabled(true);
                    break;
            }
        }
        protected virtual void OnSecurityChange(bool isenabled) { WhenSecurityChanged?.Invoke(isenabled); }
    }
}
