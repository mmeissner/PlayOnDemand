#region Licence
/****************************************************************
 *  Filename: LoggingExtensions.cs
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
using LeapVR.Shell.Domain.Models.App;

namespace LeapVR.Shell.Domain.Models
{
    public static class LoggingExtensions
    {
        public static string ToLog(this AppExecutionMessage executionMessage)
        {
            var retval = Environment.NewLine +
                         $" App-GUID: {executionMessage.AppExecutionData.LogicToExecute.ApplicationGuid}" +
                         Environment.NewLine +
                         //$" Phase: {nameof(executionMessage.Phase)}" + Environment.NewLine +
                         //$" ExecutionFile BaseDirectoryFilePath: {executionMessage.AppExecutionData.BaseDirectoryFilePath}" +
                         //Environment.NewLine +
                         //$" ExecutionFile: {executionMessage.AppExecutionData.ExecutableFile.AbsolutePath}" +
                         //Environment.NewLine +
                         //$" Execution Parameters: {executionMessage.AppExecutionData.LogicToExecute.ExecutionParameters}" +
                         //Environment.NewLine +
                         //$" Relative WorkingDirectory: {executionMessage.AppExecutionData.WorkingDirectory}" +
                         //Environment.NewLine +
                         $" Required VrModule GUID: {executionMessage.AppExecutionData.LogicToExecute.ReguiredVrModuleGuid}";
            if (executionMessage.AppExecutionData.LogicToExecute.OptionalModuleGuids != null)
            {
                foreach (string guid in executionMessage.AppExecutionData.LogicToExecute.OptionalModuleGuids)
                {
                    retval = retval + Environment.NewLine + $" OptionalModuleGuids GUID: {guid}";
                }
            }

            if (executionMessage.AppExecutionData.LogicToExecute.RequiredModuleGuids != null)
            {
                foreach (string guid in executionMessage.AppExecutionData.LogicToExecute.RequiredModuleGuids)
                {
                    retval = retval + Environment.NewLine + $" Reguired HardwareModule GUID: {guid}";
                }
            }
            return retval;
        }

    }
}
