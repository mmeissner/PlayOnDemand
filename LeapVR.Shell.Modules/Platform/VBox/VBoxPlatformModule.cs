#region Licence
/****************************************************************
 *  Filename: VBoxPlatformModule.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Platform;
using NLog;

namespace LeapVR.Shell.Modules.Platform.VBox
{
    public class VBoxPlatformModule : IPlatformModule
    {
        #region Properties & Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDiskController _diskController;
        private readonly object _currentExecutionLock = new object();
        private IApplicationExecution _currentExecution;
        private readonly IUIMessageBroker _messageBroker;

        public Guid ModuleId => Guid.Parse("aa14f747-5d15-4b06-a9c0-7187f0e206d3");
        public string ModuleName => "LeapVR Platform Module";
        public bool IsAvailable => true;
        public InstallationType SupportedInstallationTypes => InstallationType.Container;
        public AccountType SupportedAccountType => AccountType.None;
        public bool RequiresAccount => false;

        //Ignored as Platform only Supports Containers 
        public bool PlatformUninstallSupported => false;
        public string PlatformNameId { get; } = "LVR_____";
        #endregion Properties & Fields

        #region Constructors
        public VBoxPlatformModule(
            IDiskController diskController,
            IUIMessageBroker messageBroker
            )
        {
            QuickLeap.AssertNotNull(messageBroker,diskController);
            _diskController = diskController;
            _messageBroker = messageBroker;
        }
        #endregion Constructors

        #region Methods
        public bool IsApplicationAvailable(Guid appId) { return true;}
        public bool HasCache => false;
        public Dictionary<Guid, IAppPlatformData> GetLocalInstallations()
        {
            throw new NotSupportedException($"This feature {nameof(GetLocalInstallation)} is not supported byt this PlatformModule");
        }
        public IAppPlatformData GetLocalInstallation(Guid applicationId)
        {
            throw new NotSupportedException($"This feature {nameof(GetLocalInstallation)} is not supported byt this PlatformModule");
        }
        public bool IsLocalInstalled(Guid applicationId)
        {
            //NOTE: VBox is not a real platform, so practically there is no LocalInstalled state like for an App that is installed in Steam
            //So if the PlatformModule knows about the App, it's considered as local installed
            return true;
        }
        public void ClearCache()
        {
            throw new NotSupportedException($"This feature {nameof(ClearCache)} is not supported byt this PlatformModule");
        }
        public bool OnlineInstallation(
                Guid applicationId, IAccountAccess accountAccess, Action<PlatformInstallationPhase> progressReportCallBack,
                out IAppPlatformData installedApp)
        {
            throw new NotSupportedException($"This feature {nameof(OnlineInstallation)} is not supported byt this PlatformModule");
        }

#pragma warning disable 1998
        public async Task<IAppDisplayInfo> GetOnlineDisplayInfoAsync(Guid applicationId, bool addImage)

        {
            throw new NotSupportedException($"This feature {nameof(GetOnlineDisplayInfoAsync)} is not supported byt this PlatformModule");
        }

        public async Task<HashSet<Guid>> GetApplicationsFromAccountAsync(IPlatformAccount platformAccount)
        {
            throw new NotSupportedException($"This feature {nameof(GetApplicationsFromAccountAsync)} is not supported byt this PlatformModule");
        }
#pragma warning restore 1998

        public IApplicationExecution CreateExecution(IAppPlatformInfo appPlatformInfo,IAppPlatformData appPlatformData, IProcessExecutionLogic executionLogic)
        {
            QuickLeap.AssertNotNull(executionLogic);

            lock (_currentExecutionLock)
            {
                if (_currentExecution != null)
                {
                    Logger.Warn( $"Failed to {nameof(CreateExecution)}. Already other execution in progress. Will return null of type {nameof(IApplicationExecution)}.");
                    return null;
                }
                var applicationExecution = new VBoxApplicationExecution(appPlatformInfo,executionLogic, _diskController, _messageBroker);
                applicationExecution.WhenExecutionPhaseChange.Subscribe(OnCurrentExecutionPhaseChanged);
                return applicationExecution;
            }
        }
       
        private void OnCurrentExecutionPhaseChanged(AppExecutionMessage executionMessage)
        {
            switch (executionMessage.Phase)
            {
                case ExecutionPhase.NotStarted:
                case ExecutionPhase.BeforeStart:
                    break;
                case ExecutionPhase.OnPlatformStart:
                    lock (_currentExecutionLock)
                    {
                        _currentExecution = executionMessage.AppExecutionData;
                        _currentExecution.Run();
                    }
                    break;
                case ExecutionPhase.AfterStart:
                case ExecutionPhase.BeforeExit:
                    break;
                case ExecutionPhase.OnPlatformEnd:
                    lock (_currentExecutionLock)
                    {
                        _currentExecution = null;
                    }
                    break;
                case ExecutionPhase.AfterExit:
                case ExecutionPhase.OnFinished:
                    break;
            }
        }

        #endregion Methods
    }
}
