#region Licence
/****************************************************************
 *  Filename: IAppExecuteInstruction.cs
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
using System.IO;
using LeapVR.Shared.Lib.Helper;
using LeapVR.Shell.Modules.Interfaces;

namespace LeapVR.Content.Creator.Logic
{
    public interface IAppExecuteInstruction
    {
        string InstructionName { get; }
        string ApplicationMainExecutablePath { get; }
        string ApplicationMainExecutableParameters { get; }
        string ApplicationMainExecutableWorkingDirectory { get; }
        VRSelectableModule SelectedRequiredVrModule { get;  }
        IEnumerable<Executable> ExecutablesInfo { get; }
    }

    public class Executable
    {
        public string ExecutableFilePath { get; }
        public string RelativeExecutableFilePath { get; }
        public bool KillOnExit { get; set; }
        public bool KillProcessOnNotResponding { get; set; }
        public bool IsMainExecutable { get; set; }

        public string ToName()
        {
            if(ExecutableFilePath != null)return Path.GetFileNameWithoutExtension(ExecutableFilePath);
            return "Default";
        }

        public Executable(string rootDirectory, string executableFilePath)
        {
            QuickLeap.TryGetRelativePath(executableFilePath, rootDirectory, out var relativePath);
            RelativeExecutableFilePath = relativePath;
            ExecutableFilePath = executableFilePath;
        }
    }

    public class VRSelectableModule
    {
        public static string NoneName = "None";
        public string DisplayName { get; }
        public Guid ModuleGuid { get; }
        public override string ToString()
        {
            return DisplayName;
        }

        public VRSelectableModule(IBaseModule module)
        {
            ModuleGuid = module.ModuleId;
            DisplayName = module.ModuleName;
        }
        public VRSelectableModule()
        {
            ModuleGuid = Guid.Empty;
            DisplayName = NoneName;
        }
    }
}
