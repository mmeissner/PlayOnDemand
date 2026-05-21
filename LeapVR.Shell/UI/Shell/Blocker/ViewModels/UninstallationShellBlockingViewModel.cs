#region Licence
/****************************************************************
 *  Filename: UninstallationShellBlockingViewModel.cs
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
using System.Threading.Tasks;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.Container.Installation;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Domain.Models.UserInterface.EventMessages;
using LeapVR.Shell.Language;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Blocker.Abstract;
using NLog;

namespace LeapVR.Shell.UI.Shell.Blocker.ViewModels
{
    public class UninstallationShellBlockingViewModel : ApplicationBlockShellViewModel, IHandle<IUIAppUninstalledEvent>
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IUninstallationProcessInfo _uninstallationProcess;
        #region Constructors

        public UninstallationShellBlockingViewModel(IUIMessageBroker messageBroker,IUninstallationProcessInfo process, IViewInputHandler inputHandler) : base(process, inputHandler)
        {
            QuickLeap.AssertNotNull(messageBroker,process);
            _uninstallationProcess = process;
            messageBroker.Subscribe(this);
        }
        #endregion

        public async void Handle(IUIAppUninstalledEvent message)
        {
            if(message.ApplicationGuid != ApplicationGuid)return;
            IsEnded = true;
            if (_uninstallationProcess.Exception == null)
            {
                HasError = false;
            }
            else
            {
                ErrorInfo = Resources.Shell_InstallationBlocking_UninstallationFailed;
                HasError = true;
            }
            await Task.Delay(2000);
            Close();
        }
    }
}
