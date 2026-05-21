#region Licence
/****************************************************************
 *  Filename: ProcessMonitorInstructionDto.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  RadoslawMedryk
 *  Date          2018-1-22
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
using System.Reflection;
using LeapVR.Shell.Domain.Models.Container;
using LeapVR.Shell.Domain.Models.Execution;
using ProtoBuf;

namespace LeapVR.Content.Shared.Container
{
    public class ProcessMonitorInstructionDto : IProcessMonitorInstructionDto
    {
        public string ExecutableRelativePathFileName { get; set; }
        public ProcessMonitorOption Instruction { get; set; }
    }
}