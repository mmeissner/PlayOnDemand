#region Licence
/****************************************************************
 *  Filename: VBoxApplicationExecution.cs
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
using System.Diagnostics;
using System.IO;
using LeapVR.Shell.Domain.Models.App;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Platform.Account;
using LeapVR.Shell.Domain.Models.UserInterface;

// ReSharper disable ExplicitCallerInfoArgument

namespace LeapVR.Shell.Modules.Platform.VBox
{
    /// <summary>
    /// Application Execution Class for VBOX Platform
    /// </summary>
    public class VBoxApplicationExecution : BaseApplicationExecution
    {
        private Process _processToStart;
        private readonly IDiskController _diskController;
        #region Constructors

        public VBoxApplicationExecution(
            IAppPlatformInfo platformAppInfo,
            IProcessExecutionLogic logicToExecute,
            IDiskController diskController,
            IUIMessageBroker messageBroker
        )
            : base(
                platformAppInfo,
                logicToExecute,
                messageBroker
            )
        {
            _diskController = diskController;
        }

        #endregion Methods

        protected override bool IsStartable()
        {
            _processToStart = CreateProcess();
            return _processToStart != null;
        }

        protected override void OnPlatformStart()
        {
            _processToStart.Start();
        }

        protected override void OnPlatformStop()
        {
            _processToStart?.Dispose();
        }

        private Process CreateProcess()
        {
            var process = new Process();
            var baseDirectoryFilePath = _diskController.GetContentDirectory(ContentType.GameFiles, DisplayInfo.ApplicationGuid);
            var executableFilePath = _diskController.GetFilePath(LogicToExecute.ExecutionFile);
            var startInfo = new ProcessStartInfo(executableFilePath,LogicToExecute.ExecutionParameters);
            process.StartInfo = startInfo;
            var workingDirectory = !string.IsNullOrEmpty(LogicToExecute.RelativeWorkingDirectory) ? Path.Combine(baseDirectoryFilePath, LogicToExecute.RelativeWorkingDirectory) : Path.GetDirectoryName(executableFilePath);
            process.StartInfo.UseShellExecute = false;
            if (!String.IsNullOrEmpty(workingDirectory)) process.StartInfo.WorkingDirectory = workingDirectory;
            return process;
        }
    }
}
